//
//  iOSDeviceTracker.swift
//  iOSNativeFrameworkUnity
//
//  Created by Ali Hussain on 04/04/24.
//

import Foundation
import UIKit
import Metal

// Host CPU Info Constants
private let HOST_CPU_LOAD_INFO_COUNT      : mach_msg_type_number_t =
UInt32(MemoryLayout<host_cpu_load_info_data_t>.size / MemoryLayout<integer_t>.size)

// Host RAM Info Constants
private let HOST_VM_INFO64_COUNT          : mach_msg_type_number_t =
UInt32(MemoryLayout<vm_statistics64_data_t>.size / MemoryLayout<integer_t>.size)

// Declare the delegate
public typealias onUpdateStatHandler = @convention(c) (UnsafeMutablePointer<CChar>) -> Void

public class DeviceMonitor {
    public static let instance = DeviceMonitor()
    private var framesRendered = 0
    let machHost = mach_host_self()
    var loadPrevious = host_cpu_load_info()
    let mtlDevice: MTLDevice? = MTLCreateSystemDefaultDevice()
    let PAGE_SIZE = vm_kernel_page_size
    var callback: onUpdateStatHandler?
    var timer: Timer?
    
    init() {}
    
    func startTracking(handler: onUpdateStatHandler) {
        print("Tracking Started on Native Plugin")
        self.callback = handler
        self.timer = Timer.scheduledTimer(timeInterval: 1, target: self, selector: #selector(doTracking), userInfo: nil, repeats: true)
    }
    
    func startTracking() {
        print("Tracking Started on Native Plugin")
    }
    
    @objc func doTracking() {
        let cpuUsage = cpuUsage()
        let gpuUsage = gpuUsage()
        let ramUsage = ramUsage()
        let trackedData: [String: Any] = [
            "cpuUsage": [
                "system": cpuUsage.system,
                "user": cpuUsage.user,],
            "gpuUsage": [
                "max": gpuUsage.max,
                "allocated": gpuUsage.curr
            ],
            "ramUsage": [
                "free": ramUsage.free,
                "active": ramUsage.active,
            ]
        ]
        
        // Call handler and send data to Unity
        callback?(
            iOSUtils.convertStringToCSString(
                text: iOSUtils.convertDictionaryToJsonString(dictionary: trackedData)
            )
        )
    }
    
    func stopTrackingWithInterval() -> Void {
        self.timer?.invalidate()
        self.timer = nil
        self.callback = nil
    }
    
    func stopTracking() -> String {
        let cpuUsage = cpuUsage()
        let gpuUsage = gpuUsage()
        let ramUsage = ramUsage()
        let trackedData: [String: Any] = [
            "cpuUsage": [
                "system": cpuUsage.system,
                "user": cpuUsage.user,
            ],
            "gpuUsage": [
                "max": gpuUsage.max,
                "allocated": gpuUsage.curr
            ],
            "ramUsage": [
                "free": ramUsage.free,
                "active": ramUsage.active,
            ]
        ]
        
        return iOSUtils.convertDictionaryToJsonString(dictionary: trackedData)
    }
    
    func hostCPULoadInfo() -> host_cpu_load_info {
        var size     = HOST_CPU_LOAD_INFO_COUNT
        let hostInfo = host_cpu_load_info_t.allocate(capacity: 1)
        _ = hostInfo.withMemoryRebound(to: integer_t.self, capacity: Int(size)) {
            host_statistics(machHost, HOST_CPU_LOAD_INFO,
                            $0,
                            &size)
        }
        let data = hostInfo.move()
        hostInfo.deallocate()
        return data
    }
    func VMStatistics64() -> vm_statistics64 {
            var size     = HOST_VM_INFO64_COUNT
            let hostInfo = vm_statistics64_t.allocate(capacity: 1)
            let result = hostInfo.withMemoryRebound(to: integer_t.self, capacity: Int(size)) {
                host_statistics64(machHost,
                                  HOST_VM_INFO64,
                                  $0,
                                  &size)
            }
            let data = hostInfo.move()
            hostInfo.deallocate()
            return data
        }
    
    // Get CPU usage by host_cpu_load_info_t
    private func cpuUsage() -> (
        system: Double,
        user: Double)
        {
            let load = hostCPULoadInfo()
            
            let userDiff = Double(load.cpu_ticks.0 - loadPrevious.cpu_ticks.0)
            let sysDiff  = Double(load.cpu_ticks.1 - loadPrevious.cpu_ticks.1)
            
            let totalTicks = userDiff + sysDiff
            
            let sys  = (sysDiff  / totalTicks) * 100.0
            let user = (userDiff / totalTicks) * 100.0
            
            loadPrevious = load
            
            return (sys, user)
        }
    // Get RAM usage
    private func ramUsage() -> (
        free: Double, // Total RAM
        active: Double) // Used RAM
        {
            let stats = VMStatistics64()
            
            let freeBytes = Double(stats.wire_count + stats.active_count + stats.inactive_count + stats.free_count) * Double(PAGE_SIZE)
            let activeBytes = Double(stats.wire_count + stats.active_count) * Double(PAGE_SIZE)
            
            let free     = iOSUtils.convertByte(
                value: freeBytes,
                target: iOSUtils.UnitType.GB)
            let active   = iOSUtils.convertByte(
                value: activeBytes,
                target: iOSUtils.UnitType.GB)
            
            return (free, active)
        }
    
    // Get the GPU usage by Metal API
    private func gpuUsage() -> (max: UInt64, curr: Int) {
        let maxGPUMem = mtlDevice?.recommendedMaxWorkingSetSize
        let currAllocatedGPUMem = mtlDevice?.currentAllocatedSize
        return (iOSUtils.convertByte(value: maxGPUMem!, target: iOSUtils.UnitType.MB),
                iOSUtils.convertByte(value: currAllocatedGPUMem!, target: iOSUtils.UnitType.MB))
    }

}
// Expose to Unity
@_cdecl("startTrackingWithInterval")
public func startTrackingWithInterval(handler: onUpdateStatHandler) -> Void {
    DeviceMonitor.instance.startTracking(handler: handler)
}

@_cdecl("stopTrackingWithInterval")
public func stopTrackingWithInterval() -> Void {
    DeviceMonitor.instance.stopTrackingWithInterval()
}

@_cdecl("startTracking")
public func startTracking() -> Void {
    DeviceMonitor.instance.startTracking()
}

@_cdecl("stopTracking")
public func stopTracking() -> UnsafeMutablePointer<CChar> {
    return iOSUtils.convertStringToCSString(text: DeviceMonitor.instance.stopTracking())
}


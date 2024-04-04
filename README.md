# iOS Performance Monitoring Native Framework For Unity

This repository contains code for tracking device statistics (CPU, RAM, GPU) on iOS devices and communicating with Unity.

## Overview

The project consists of two main parts:

1. Xcode Side: Contains Swift code for monitoring CPU and RAM usage, as well as GPU usage using Metal API. It also includes utilities for converting between different units and communicating with Unity.

2. Unity Side: Contains C# code for handling communication with the iOS side, updating UI elements with device statistics, and providing user controls for starting and stopping tracking.

## Getting Started

To use this project:

1. Clone the repository to your local machine.
2. Open the Xcode project and build the iOS Native Framework - Execute the command ./build.sh to initiate the building process. Once completed, the most recent version of the library, named within iOSNativeFrameworkUnity.xcodeproj, will be generated and accessible in the Products folder.
3. Import the generated framework into your Unity project.
4. Attach the provided C# scripts to GameObjects in your Unity scene.
5. Use the provided buttons to start and stop tracking device statistics.

## Usage

### Xcode Side

- `iOSDeviceTracker.swift`: Main class for tracking device statistics.
- `iOSUtils.swift`: Utility functions for unit conversion and data serialization.

### Unity Side

- `PluginController.cs`: Main script for controlling tracking and updating UI.
- `iOSDeviceTracker.cs`: Unity wrapper for interacting with iOS native methods.
- `Stat.cs`: Data structure for holding device statistics.

# Development Environment

| Tools | Version     |
| ----- | ----------- |
| Xcode | 15.3        |
| Unity | 2022.3.22f1 |
| MacOS | 14.4      |


# References and Credits

This library was developed with insights and guidance from various repositories, examples, and documentation:

1. [SystemKit](https://github.com/beltex/SystemKit)
   - Provided understanding on accessing CPU and RAM usage in Swift.

2. [UnityPluginXcodeTemplate](https://github.com/fuziki/UnityPluginXcodeTemplate)
   - Offered assistance in data transition and format between Unity and native code.
   - Facilitated the setup of delegates between Unity and iOS native plugins using Swift.

3. [Metal API Documentation - MLTDevice](https://developer.apple.com/documentation/metal/mtldevice)
   - Utilized for implementing GPU information retrieval within the Swift library for Unity.
     - [currentAllocatedSize](https://developer.apple.com/documentation/metal/mtldevice/2915745-currentallocatedsize) - Represents the total memory usage, in bytes, by the GPU device across all resources.
     - [recommendedMaxWorkingSetSize](https://developer.apple.com/documentation/metal/mtldevice/2369280-recommendedmaxworkingsetsize) - Provides an estimate of the maximum memory allocation, in bytes, that the GPU device can handle without impacting its runtime performance.

4. [Interval in Swift](https://stackoverflow.com/a/40148293)

Gratitude goes to the authors and contributors of these resources for their invaluable insights and examples that greatly contributed to the development of this library.


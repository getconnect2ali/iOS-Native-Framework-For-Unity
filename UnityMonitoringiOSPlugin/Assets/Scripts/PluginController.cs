using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;
using Plugins.iOS.TrackingUsage;

public class PluginController : MonoBehaviour
{
    private const string FormatStatStr = "{0}: {1}";

    private bool _isIntervalMode = false;

    [Header("Reference")]
    public Button startTrackingBtn;
    public Button stopTrackingBtn;

    [Header("CPU Usage UI")]
    public TextMeshProUGUI cpuSystemLbl;
    public TextMeshProUGUI cpuUserLbl;

    [Header("RAM Usage UI")]
    public TextMeshProUGUI ramFreeLbl;
    public TextMeshProUGUI ramActiveLbl;

    [Header("GPU Usage UI")]
    public TextMeshProUGUI gpuAllocatedLbl;
    public Slider gpuAllocatedSlider;

    private void Start()
    {
        UpdateUIState();
    }

    void UpdateUIState()
    {
        // Hide the dashboard at the beginning
        startTrackingBtn.interactable = !iOSDeviceTracker.Instance.IsNowTracking();
        stopTrackingBtn.interactable = iOSDeviceTracker.Instance.IsNowTracking();
    }

    #region Event Handlers

    public void OnModeToggleClicked()
    {
        _isIntervalMode = !_isIntervalMode;
    }

    public void OnStartTrackingClicked()
    {
        if (_isIntervalMode)
        {
            if (iOSDeviceTracker.Instance.StartTrackingWithInterval(OnStatReceived))
            {
                UpdateUIState();
            }
        }
        else
        {
            if (iOSDeviceTracker.Instance.StartTracking())
            {
                UpdateUIState();
            }
        }
    }

    public void OnStopTrackingClicked()
    {
        if (_isIntervalMode)
        {
            iOSDeviceTracker.Instance.StopTrackingWithInterval();
            UpdateUIState();
        }
        else
        {
            var stat = iOSDeviceTracker.Instance.StopTracking();
            UpdateUIState();
            OnStatReceived(stat);
        }
    }

    public void OnStatReceived(Stat stat)
    {
        if (stat != null)
        {
            // CPU Usage
            // - Mapping data to the UI
            cpuSystemLbl.text = String.Format(FormatStatStr, "System", $"{stat.cpuUsage.system:0.00}%");
            cpuUserLbl.text = String.Format(FormatStatStr, "User", $"{stat.cpuUsage.user:0.00}%");

            // RAM Usage
            // - Set the maximum RAM value by summing the RAM values
            float maxRam = stat.ramUsage.active + stat.ramUsage.free ;

            // - Mapping data to the UI
            ramFreeLbl.text = String.Format(FormatStatStr, "Total", $"{stat.ramUsage.free:0.00}GB");
            ramActiveLbl.text = String.Format(FormatStatStr, "Used", $"{stat.ramUsage.active:0.00}GB");

            // GPU Usage
            // - Set the maximum GPU value
            gpuAllocatedSlider.maxValue = stat.gpuUsage.max;

            // - Mapping data to the UI
            gpuAllocatedLbl.text = String.Format(FormatStatStr, "Used",
                $"{stat.gpuUsage.allocated:0.00}MB / Max: {stat.gpuUsage.max}MB");
            gpuAllocatedSlider.value = stat.gpuUsage.allocated;
        }
    }

    #endregion
}
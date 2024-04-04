using System;

namespace Plugins.iOS.TrackingUsage
{
    [Serializable]
    public class Stat
    {
        public CPUUsage cpuUsage;
        public RamUsage ramUsage;
        public GPUUsage gpuUsage;
    }
    [Serializable]
    public class CPUUsage
    {
        public float system;
        public float user;
    }

    [Serializable]
    public class RamUsage
    {
        public float active;
        public float free;
    }

    [Serializable]
    public class GPUUsage
    {
        public float allocated;
        public float max;
    }
}
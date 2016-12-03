using System;

namespace OpenSatelliteProject {
    public struct ImageNavigationRecord {
        public string ProjectionName; // 32 bytes
        public UInt32 ColumnScalingFactor;
        public UInt32 LineScalingFactor;
        public UInt32 ColumnOffset;
        public UInt32 LineOffset;
    }
}


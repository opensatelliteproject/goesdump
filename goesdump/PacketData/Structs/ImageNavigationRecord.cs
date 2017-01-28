using System;
using System.Runtime.InteropServices;

namespace OpenSatelliteProject.PacketData.Structs {
    
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ImageNavigationRecord {
        public byte type;
        public UInt16 size;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
        public string ProjectionName;
        public UInt32 ColumnScalingFactor;
        public UInt32 LineScalingFactor;
        public UInt32 ColumnOffset;
        public UInt32 LineOffset;
    }
}


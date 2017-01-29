using System;
using System.Runtime.InteropServices;

namespace OpenSatelliteProject.PacketData.Structs {
    
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct NOAASpecificRecord {
        public byte type;
        public UInt16 size;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 4)]
        public string Signature;

        public UInt16 ProductID;
        public UInt16 ProductSubID;
        public UInt16 Parameter;
        public byte Compression;
    }
}


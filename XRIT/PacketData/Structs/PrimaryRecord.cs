using System;
using System.Runtime.InteropServices;

namespace OpenSatelliteProject.PacketData.Structs {

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct PrimaryRecord {
        public byte type;
        public UInt16 size;
        public byte FileTypeCode;
        public UInt32 HeaderLength;
        public UInt64 DataLength;
    }
}


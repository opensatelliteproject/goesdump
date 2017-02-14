using System;
using System.Runtime.InteropServices;

namespace OpenSatelliteProject.PacketData.Structs {

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct RiceCompressionRecord {
        public byte type;
        public UInt16 size;

        public UInt16 Flags;
        public byte Pixel;
        public byte Line;
    }
}


using System;
using System.Runtime.InteropServices;

namespace OpenSatelliteProject.PacketData.Structs {

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ImageStructureRecord {
        public byte type;
        public UInt16 size;
        public byte BitsPerPixel;
        public UInt16 Columns;
        public UInt16 Lines;
        public byte Compression;
    }
}


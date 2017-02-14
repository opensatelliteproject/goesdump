using System;
using System.Runtime.InteropServices;

namespace OpenSatelliteProject.PacketData.Structs {

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct SegmentIdentificationRecord {
        public byte type;
        public UInt16 size;

        public UInt16 ImageID;
        public UInt16 Sequence;
        public UInt16 StartColumn;
        public UInt16 StartLine;
        public UInt16 MaxSegments;
        public UInt16 MaxColumns;
        public UInt16 MaxRows;
    }
}


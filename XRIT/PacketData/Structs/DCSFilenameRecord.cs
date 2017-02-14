using System;

namespace OpenSatelliteProject.PacketData.Structs {
    public struct DCSFilenameRecord {
        public byte type;
        public UInt16 size;
        public string Filename;
    }
}


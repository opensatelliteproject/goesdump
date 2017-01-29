using System;

namespace OpenSatelliteProject.PacketData.Structs {
    
    public struct HeaderStructuredRecord {
        public byte type;
        public UInt16 size;
        public string Data;
    }
}


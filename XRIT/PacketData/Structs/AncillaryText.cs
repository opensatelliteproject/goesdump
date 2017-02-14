using System;
using System.Runtime.InteropServices;

namespace OpenSatelliteProject.PacketData.Structs {
    
    public struct AncillaryText {
        public byte type;
        public UInt16 size;
        public string Data;
    }
}


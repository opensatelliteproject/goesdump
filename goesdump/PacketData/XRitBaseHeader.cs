using System;
using OpenSatelliteProject.PacketData.Enums;

namespace OpenSatelliteProject.PacketData {
    public class XRITBaseHeader {
        public HeaderType Type { get; set; }
        public byte[] RawData { get; set; }

        public XRITBaseHeader() {
            Type = HeaderType.Unknown;
            RawData = new byte[0];
        }

        public XRITBaseHeader(HeaderType type, byte[] rawData) {
            Type = type;
            RawData = rawData;
        }
    }
}


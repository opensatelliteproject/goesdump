using System;
using OpenSatelliteProject.PacketData.Enums;

namespace OpenSatelliteProject.PacketData {
    public class XRitBaseHeader {
        public HeaderType Type { get; set; }
        public byte[] RawData { get; set; }

        public XRitBaseHeader() {
            Type = HeaderType.Unknown;
            RawData = new byte[0];
        }

        public XRitBaseHeader(HeaderType type, byte[] rawData) {
            Type = type;
            RawData = rawData;
        }
    }
}


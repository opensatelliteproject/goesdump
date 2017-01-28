using System;
using OpenSatelliteProject.PacketData.Structs;
using OpenSatelliteProject.PacketData.Enums;

namespace OpenSatelliteProject.PacketData {
    public class AncillaryHeader: XRitBaseHeader {
        public String Filename { get; set; }

        public AncillaryHeader(AncillaryText data) {
            Type = HeaderType.AncillaryTextRecord;
            Filename = data.Data;
        }
    }
}


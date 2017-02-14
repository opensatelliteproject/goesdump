using System;
using OpenSatelliteProject.PacketData.Structs;
using OpenSatelliteProject.PacketData.Enums;

namespace OpenSatelliteProject.PacketData {
    public class HeaderStructuredHeader: XRITBaseHeader {

        public string Data { get; set;}

        public HeaderStructuredHeader(HeaderStructuredRecord data) {
            Type = HeaderType.HeaderStructuredRecord;
            Data = data.Data;
        }
    }
}


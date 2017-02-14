using System;
using OpenSatelliteProject.PacketData.Structs;
using OpenSatelliteProject.PacketData.Enums;

namespace OpenSatelliteProject.PacketData {
    public class DCSFilenameHeader: XRITBaseHeader {
        public string Filename { get; set; }

        public DCSFilenameHeader(DCSFilenameRecord data) {
            Type = HeaderType.DCSFileNameRecord;    
            Filename = data.Filename;
        }
    }
}


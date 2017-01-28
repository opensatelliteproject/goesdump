using System;
using OpenSatelliteProject.PacketData.Structs;
using OpenSatelliteProject.PacketData.Enums;

namespace OpenSatelliteProject.PacketData {
    public class AnnotationHeader: XRitBaseHeader {
        public string Filename { get; set;}

        public AnnotationHeader(AnnotationRecord data) {
            Type = HeaderType.AnnotationRecord;
            Filename = data.Filename;
        }
    }
}


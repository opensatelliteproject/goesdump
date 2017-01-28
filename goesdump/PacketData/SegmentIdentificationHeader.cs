using System;
using OpenSatelliteProject.PacketData.Enums;
using OpenSatelliteProject.PacketData.Structs;

namespace OpenSatelliteProject.PacketData {
    public class SegmentIdentificationHeader: XRitBaseHeader {

        public UInt16 ImageID { get; set; }
        public UInt16 Sequence { get; set; }
        public UInt16 StartColumn { get; set; }
        public UInt16 StartLine { get; set; }
        public UInt16 MaxSegments { get; set; }
        public UInt16 MaxColumns { get; set; }
        public UInt16 MaxRows { get; set; }

        public SegmentIdentificationHeader(SegmentIdentificationRecord data) {
            Type = HeaderType.SegmentIdentificationRecord;
            ImageID = data.ImageID;
            Sequence = data.Sequence;
            StartColumn = data.StartColumn;
            StartLine = data.StartLine;
            MaxSegments = data.MaxSegments;
            MaxRows = data.MaxRows;
        }
    }
}


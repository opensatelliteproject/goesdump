using System;
using OpenSatelliteProject.PacketData.Structs;
using OpenSatelliteProject.PacketData.Enums;

namespace OpenSatelliteProject.PacketData {
    public class ImageNavigationHeader: XRITBaseHeader {
        
        public string ProjectionName { get; set; }
        public UInt32 ColumnScalingFactor { get; set; }
        public UInt32 LineScalingFactor { get; set; }
        public Int32 ColumnOffset { get; set; }
        public Int32 LineOffset { get; set; }

        public ImageNavigationHeader(ImageNavigationRecord data) {
            Type = HeaderType.ImageNavigationRecord;
            ProjectionName = data.ProjectionName;
            ColumnScalingFactor = data.ColumnScalingFactor;
            LineScalingFactor = data.LineScalingFactor;
            ColumnOffset = data.ColumnOffset;
            LineOffset = data.LineOffset;
        }
    }
}


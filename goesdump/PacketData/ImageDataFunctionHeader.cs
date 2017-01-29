using System;
using OpenSatelliteProject.PacketData.Structs;
using OpenSatelliteProject.PacketData.Enums;

namespace OpenSatelliteProject.PacketData {
    public class ImageDataFunctionHeader: XRITBaseHeader {
        public string Data { get; set; }
        public ImageDataFunctionHeader(ImageDataFunctionRecord data) {
            Type = HeaderType.ImageDataFunctionRecord;
            Data = data.Data;
        }
    }
}


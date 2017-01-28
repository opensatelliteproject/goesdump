using System;
using OpenSatelliteProject.PacketData.Structs;
using OpenSatelliteProject.PacketData.Enums;

namespace OpenSatelliteProject.PacketData {
    public class ImageStructureHeader: XRITBaseHeader {

        public byte BitsPerPixel { get; set; }
        public UInt16 Columns { get; set; } 
        public UInt16 Lines { get; set; }
        public CompressionType Compression { get; set; }

        public ImageStructureHeader(ImageStructureRecord data) {
            Type = HeaderType.ImageStructureRecord;
            BitsPerPixel = data.BitsPerPixel;
            Columns = data.Columns;
            Lines = data.Lines;
            Compression = (CompressionType)data.Compression;
        }
    }
}


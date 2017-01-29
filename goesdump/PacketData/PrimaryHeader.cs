using System;
using OpenSatelliteProject.PacketData.Structs;
using OpenSatelliteProject.PacketData.Enums;

namespace OpenSatelliteProject.PacketData {
    public class PrimaryHeader: XRITBaseHeader {
        
        public FileTypeCode FileType { get; set; }
        public UInt32 HeaderLength;
        public UInt64 DataLength;

        public PrimaryHeader(PrimaryRecord data) {
            Type = HeaderType.PrimaryHeader;
            FileType = (FileTypeCode)data.FileTypeCode;
            HeaderLength = data.HeaderLength;
            DataLength = data.DataLength;
        }
    }
}


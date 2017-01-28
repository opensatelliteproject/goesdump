using System;
using OpenSatelliteProject.PacketData.Structs;
using OpenSatelliteProject.PacketData.Enums;

namespace OpenSatelliteProject.PacketData {
    public class RiceCompressionHeader: XRitBaseHeader {
        public UInt16 Flags;
        public byte Pixel;
        public byte Line;

        public RiceCompressionHeader(RiceCompressionRecord data) {
            Type = HeaderType.RiceCompressionRecord;
            Flags = data.Flags;
            Pixel = data.Pixel;
            Line = data.Line;
        }
    }
}


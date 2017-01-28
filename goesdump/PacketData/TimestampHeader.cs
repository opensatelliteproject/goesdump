using System;
using OpenSatelliteProject.PacketData.Enums;
using OpenSatelliteProject.PacketData.Structs;

namespace OpenSatelliteProject.PacketData {
    public class TimestampHeader: XRitBaseHeader {

        public DateTime DateTime { get; set; }

        public TimestampHeader(TimestampRecord data) {
            Type = HeaderType.TimestampRecord;
            DateTime = data.getDateTime();
        }
    }
}


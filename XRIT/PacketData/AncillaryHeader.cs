using System;
using OpenSatelliteProject.PacketData.Structs;
using OpenSatelliteProject.PacketData.Enums;
using System.Collections.Generic;
using System.Linq;

namespace OpenSatelliteProject.PacketData {
    public class AncillaryHeader: XRITBaseHeader {
        public String RawString { get; set; }
        public Dictionary<string, string> Values { get; set; }

        public AncillaryHeader(AncillaryText data) {
            Type = HeaderType.AncillaryTextRecord;
            RawString = data.Data;

            Values = data.Data.Split(';')
                .Where(x => x.Trim().Length > 0) // Filter out empty items
                .Select(x => x.Trim())           // Trim strings
                .Select(x => x.Split('='))       // Split by '=' (key = value)
                .ToDictionary(x => x[0].Trim(), x => x.Length > 1 ? x[1].Trim() : ""); // Map to Dictionary
        }
    }
}


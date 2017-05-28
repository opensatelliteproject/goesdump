using System;

namespace OpenSatelliteProject {
    public class ConfigEntryInfo {
        public string Name { get; set; }
        public object Value { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public object DefaultValue { get; set; }
    }
}


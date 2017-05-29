using System;
using System.Collections.Generic;

namespace OpenSatelliteProject {
    public class ConfigEntryModel: BaseModel {
        public Dictionary<string, ConfigEntryInfo> Configuration { get; set; }

        public ConfigEntryModel(Dictionary<string, ConfigEntryInfo> configuration) : base("configList") {
            this.Configuration = configuration;
        }
    }
}


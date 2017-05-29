using System;

namespace OpenSatelliteProject {
    public class ConfigDescription: Attribute {
        public string Description;
        public object Default;

        public ConfigDescription (string description) : this(description, null) {

        }

        public ConfigDescription (string description, object def) {
            this.Description = description;
            this.Default = def;
        }
    }
}


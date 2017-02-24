using System;

namespace OpenSatelliteProject {
    public class ConsoleModel: BaseModel {
        public string message { get; set; }
        public string level { get; set; }

        public ConsoleModel(string level, string message) : base("console") {
            this.message = message;
            this.level = level;
        }
    }
}


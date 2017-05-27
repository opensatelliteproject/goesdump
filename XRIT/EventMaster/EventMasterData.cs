using System;

namespace OpenSatelliteProject {
    public class EventMasterData {
        public string Type { get; private set; }
        public object Data { get; private set; }

        public EventMasterData(string type) {
            Type = type;
        }

        public EventMasterData(string type, object data) {
            Type = type;
            Data = data;
        }
    }
}


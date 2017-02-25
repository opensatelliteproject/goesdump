using System;
using Newtonsoft.Json;

namespace OpenSatelliteProject {
    public class BaseModel {

        public string DataType { get; set; }

        public BaseModel(string type) { this.DataType = type; }
        public string toJSON() {
            return JsonConvert.SerializeObject(this);
        }
    }
}


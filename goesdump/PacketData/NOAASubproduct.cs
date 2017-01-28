using System;

namespace OpenSatelliteProject.PacketData {
    public class NOAASubproduct {
        public int ID { get; set; }
        public string Name;

        public NOAASubproduct(int id, string name) {
            ID = id;
            Name = name;
        }
    }
}


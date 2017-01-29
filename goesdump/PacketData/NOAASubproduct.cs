using System;

namespace OpenSatelliteProject.PacketData {
    public class NOAASubproduct {
        public int ID { get; set; }

        public string Name;

        public NOAASubproduct(int id) {
            ID = id;
            Name = "Unknown";
        }

        public NOAASubproduct(ScannerSubProduct id, string name) : this((int)id, name) {

        }

        public NOAASubproduct(int id, string name) {
            ID = id;
            Name = name;
        }
    }
}


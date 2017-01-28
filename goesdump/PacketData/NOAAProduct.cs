using System;
using System.Collections.Generic;

namespace OpenSatelliteProject.PacketData {
    public class NOAAProduct {
        public int ID { get; set; }
        public string Name { get; set; }
        public Dictionary<int, NOAASubproduct> SubProducts;

        public NOAAProduct(int id) {
            ID = id;
            Name = "Unknown";
        }

        public NOAAProduct(int id, string name) {
            ID = id;
            Name = name;
            SubProducts = new Dictionary<int, NOAASubproduct>();
        }

        public NOAAProduct(int id, string name, Dictionary<int, NOAASubproduct> subProducts) {
            ID = id;
            Name = name;
            SubProducts = new Dictionary<int, NOAASubproduct>();
            foreach (int key in subProducts.Keys) {
                SubProducts.Add(key, subProducts[key]);
            }
        }

        public NOAASubproduct getSubProduct(int id) {
            if (SubProducts.ContainsKey(id)) {
                return SubProducts[id];
            } else {
                return new NOAASubproduct(id, "Unknown");
            }
        }
    }
}


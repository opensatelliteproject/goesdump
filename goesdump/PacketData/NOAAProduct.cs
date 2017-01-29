using System;
using System.Collections.Generic;
using OpenSatelliteProject.PacketData.Enums;

namespace OpenSatelliteProject.PacketData {
    public class NOAAProduct {
        public int ID { get; set; }

        public string Name { get; set; }

        public Dictionary<int, NOAASubproduct> SubProducts;

        public NOAAProduct(int id) {
            ID = id;
            Name = "Unknown";
            SubProducts = new Dictionary<int, NOAASubproduct>();
        }

        public NOAAProduct(int id, string name) {
            ID = id;
            Name = name;
            SubProducts = new Dictionary<int, NOAASubproduct>();
        }

        public NOAAProduct(NOAAProductID id, string name) : this((int)id, name) {

        }

        public NOAAProduct(NOAAProductID id, string name, Dictionary<int, NOAASubproduct> subProducts) : this((int)id, name, subProducts) {
        
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


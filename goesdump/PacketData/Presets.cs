using System;
using System.Collections.Generic;

namespace OpenSatelliteProject.PacketData {
    public static class Presets {

        private readonly static Dictionary<int, NOAAProduct> noaaProducts;

        static Presets() {
            noaaProducts = new Dictionary<int, NOAAProduct>();
            // From https://github.com/opensatelliteproject/xritparser/blob/master/xrit/packetmanager/__init__.py

            noaaProducts.Add(1, new NOAAProduct(1, "NOAA Text"));

            noaaProducts.Add(3  , new NOAAProduct(3, "Other Satellites", new Dictionary<int, NOAASubproduct>() {
                { 0, new NOAASubproduct(0, "None") },
                { 1, new NOAASubproduct(1, "Infrared Full Disk") },
                { 3, new NOAASubproduct(3, "Visible Full Disk") }
            }));

            noaaProducts.Add(4, new NOAAProduct(4, "Other Satellites", new Dictionary<int, NOAASubproduct>() {
                { 0, new NOAASubproduct(0, "None") },
                { 1, new NOAASubproduct(1, "Infrared Full Disk") },
                { 3, new NOAASubproduct(3, "Visible Full Disk") }
            }));

            noaaProducts.Add(6, new NOAAProduct(6, "Weather Data"));

            noaaProducts.Add(8, new NOAAProduct(8, "DCS"));

            noaaProducts.Add(13, new NOAAProduct(13, "Scanner Image", new Dictionary<int, NOAASubproduct>() {
                {  0, new NOAASubproduct( 0, "None") },
                {  1, new NOAASubproduct( 1, "Infrared Full Disk") },
                {  2, new NOAASubproduct( 2, "Infrared Northern Hemisphere") },
                {  3, new NOAASubproduct( 3, "Infrared Southern Hemisphere") },
                {  5, new NOAASubproduct( 5, "Infrared Area of Interest") },
                { 11, new NOAASubproduct(11, "Visible Full Disk") },
                { 12, new NOAASubproduct(12, "Visible Northern Hemisphere") },
                { 13, new NOAASubproduct(13, "Visible Southern Hemisphere") },
                { 15, new NOAASubproduct(15, "Visible Area of Interest") },
                { 21, new NOAASubproduct(21, "Water Vapour Full Disk") },
                { 22, new NOAASubproduct(22, "Water Vapour Northern Hemisphere") },
                { 23, new NOAASubproduct(23, "Water Vapour Southern Hemisphere") },
                { 25, new NOAASubproduct(25, "Water Vapour Area of Interest") }
            }));

            noaaProducts.Add(15, new NOAAProduct(15, "Scanner Image", new Dictionary<int, NOAASubproduct>() {
                {  0, new NOAASubproduct( 0, "None") },
                {  1, new NOAASubproduct( 1, "Infrared Full Disk") },
                {  2, new NOAASubproduct( 2, "Infrared Northern Hemisphere") },
                {  3, new NOAASubproduct( 3, "Infrared Southern Hemisphere") },
                {  5, new NOAASubproduct( 5, "Infrared Area of Interest") },
                { 11, new NOAASubproduct(11, "Visible Full Disk") },
                { 12, new NOAASubproduct(12, "Visible Northern Hemisphere") },
                { 13, new NOAASubproduct(13, "Visible Southern Hemisphere") },
                { 15, new NOAASubproduct(15, "Visible Area of Interest") },
                { 21, new NOAASubproduct(21, "Water Vapour Full Disk") },
                { 22, new NOAASubproduct(22, "Water Vapour Northern Hemisphere") },
                { 23, new NOAASubproduct(23, "Water Vapour Southern Hemisphere") },
                { 25, new NOAASubproduct(25, "Water Vapour Area of Interest") }
            }));


            noaaProducts.Add(42, new NOAAProduct(42, "EMWIN"));
        }

        public static NOAAProduct GetProductById(int productId) {
            if (noaaProducts.ContainsKey(productId)) {
                return noaaProducts[productId];
            } else {
                return new NOAAProduct(productId);
            }
        }
    }
}


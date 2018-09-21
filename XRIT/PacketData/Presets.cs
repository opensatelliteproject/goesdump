using System;
using System.Collections.Generic;
using OpenSatelliteProject.PacketData.Enums;

namespace OpenSatelliteProject.PacketData {
    public static class Presets {

        private static readonly Dictionary<int, NOAAProduct> noaaProducts;

        static Presets() {
            noaaProducts = new Dictionary<int, NOAAProduct>();
            // From https://github.com/opensatelliteproject/xritparser/blob/master/xrit/packetmanager/__init__.py

            noaaProducts.Add((int)NOAAProductID.NOAA_TEXT, new NOAAProduct(NOAAProductID.NOAA_TEXT, "NOAA Text"));

            noaaProducts.Add((int)NOAAProductID.OTHER_SATELLITES_1, new NOAAProduct(NOAAProductID.OTHER_SATELLITES_1, "Other Satellites", new Dictionary<int, NOAASubproduct>() {
                { 0, new NOAASubproduct(0, "None") },
                { 1, new NOAASubproduct(1, "Infrared Full Disk") },
                { 3, new NOAASubproduct(3, "Visible Full Disk") }
            }));

            noaaProducts.Add((int)NOAAProductID.OTHER_SATELLITES_2, new NOAAProduct(NOAAProductID.OTHER_SATELLITES_2, "Other Satellites", new Dictionary<int, NOAASubproduct>() {
                { 0, new NOAASubproduct(0, "None") },
                { 1, new NOAASubproduct(1, "Infrared Full Disk") },
                { 3, new NOAASubproduct(3, "Visible Full Disk") }
            }));

            noaaProducts.Add((int)NOAAProductID.WEATHER_DATA, new NOAAProduct(NOAAProductID.WEATHER_DATA, "Weather Data"));

            noaaProducts.Add((int)NOAAProductID.DCS, new NOAAProduct(NOAAProductID.DCS, "DCS"));

            noaaProducts.Add((int)NOAAProductID.HRIT_EMWIN, new NOAAProduct(NOAAProductID.HRIT_EMWIN, "HRIT EMWIN TEXT"));

            noaaProducts.Add((int)NOAAProductID.ABI_RELAY, new NOAAProduct(NOAAProductID.ABI_RELAY, "ABI RELAY", new Dictionary<int, NOAASubproduct>() { // So far, only received GOES 13 images. Coecidence?
                { (int) ScannerSubProduct.NONE,                         new NOAASubproduct(ScannerSubProduct.NONE,                          "None") },
                { (int) ScannerSubProduct.INFRARED_FULLDISK,            new NOAASubproduct(ScannerSubProduct.INFRARED_FULLDISK,             "Infrared Full Disk") },
                { (int) ScannerSubProduct.INFRARED_NORTHERN,            new NOAASubproduct(ScannerSubProduct.INFRARED_NORTHERN,             "Infrared Northern Hemisphere") },
                { (int) ScannerSubProduct.INFRARED_SOUTHERN,            new NOAASubproduct(ScannerSubProduct.INFRARED_SOUTHERN,             "Infrared Southern Hemisphere") },
                { (int) ScannerSubProduct.INFRARED_UNITEDSTATES,        new NOAASubproduct(ScannerSubProduct.INFRARED_UNITEDSTATES,         "Infrared United States") },
                { (int) ScannerSubProduct.INFRARED_AREA_OF_INTEREST,    new NOAASubproduct(ScannerSubProduct.INFRARED_AREA_OF_INTEREST,     "Infrared Area of Interest") },
                { (int) ScannerSubProduct.VISIBLE_FULLDISK,             new NOAASubproduct(ScannerSubProduct.VISIBLE_FULLDISK,              "Visible Full Disk") },
                { (int) ScannerSubProduct.VISIBLE_NORTHERN,             new NOAASubproduct(ScannerSubProduct.VISIBLE_NORTHERN,              "Visible Northern Hemisphere") },
                { (int) ScannerSubProduct.VISIBLE_SOUTHERN,             new NOAASubproduct(ScannerSubProduct.VISIBLE_SOUTHERN,              "Visible Southern Hemisphere") },
                { (int) ScannerSubProduct.VISIBLE_UNITEDSTATES,         new NOAASubproduct(ScannerSubProduct.VISIBLE_UNITEDSTATES,          "Visible United States") },
                { (int) ScannerSubProduct.VISIBLE_AREA_OF_INTEREST,     new NOAASubproduct(ScannerSubProduct.VISIBLE_AREA_OF_INTEREST,      "Visible Area of Interest") },
                { (int) ScannerSubProduct.WATERVAPOUR_FULLDISK,         new NOAASubproduct(ScannerSubProduct.WATERVAPOUR_FULLDISK,          "Water Vapour Full Disk") },
                { (int) ScannerSubProduct.WATERVAPOUR_NORTHERN,         new NOAASubproduct(ScannerSubProduct.WATERVAPOUR_NORTHERN,          "Water Vapour Northern Hemisphere") },
                { (int) ScannerSubProduct.WATERVAPOUR_SOUTHERN,         new NOAASubproduct(ScannerSubProduct.WATERVAPOUR_SOUTHERN,          "Water Vapour Southern Hemisphere") },
                { (int) ScannerSubProduct.WATERVAPOUR_UNITEDSTATES,     new NOAASubproduct(ScannerSubProduct.WATERVAPOUR_UNITEDSTATES,      "Water Vapour United States") },
                { (int) ScannerSubProduct.WATERVAPOUR_AREA_OF_INTEREST, new NOAASubproduct(ScannerSubProduct.WATERVAPOUR_AREA_OF_INTEREST,  "Water Vapour Area of Interest") }
            }));

            noaaProducts.Add((int)NOAAProductID.GOES13_ABI, new NOAAProduct(NOAAProductID.GOES13_ABI, "GOES 13 ABI", new Dictionary<int, NOAASubproduct>() { // So far, only received GOES 13 images. Coecidence?
                { (int) ScannerSubProduct.NONE,                         new NOAASubproduct(ScannerSubProduct.NONE,                          "None") },
                { (int) ScannerSubProduct.INFRARED_FULLDISK,            new NOAASubproduct(ScannerSubProduct.INFRARED_FULLDISK,             "Infrared Full Disk") },
                { (int) ScannerSubProduct.INFRARED_NORTHERN,            new NOAASubproduct(ScannerSubProduct.INFRARED_NORTHERN,             "Infrared Northern Hemisphere") },
                { (int) ScannerSubProduct.INFRARED_SOUTHERN,            new NOAASubproduct(ScannerSubProduct.INFRARED_SOUTHERN,             "Infrared Southern Hemisphere") },
                { (int) ScannerSubProduct.INFRARED_UNITEDSTATES,        new NOAASubproduct(ScannerSubProduct.INFRARED_UNITEDSTATES,         "Infrared United States") },
                { (int) ScannerSubProduct.INFRARED_AREA_OF_INTEREST,    new NOAASubproduct(ScannerSubProduct.INFRARED_AREA_OF_INTEREST,     "Infrared Area of Interest") },
                { (int) ScannerSubProduct.VISIBLE_FULLDISK,             new NOAASubproduct(ScannerSubProduct.VISIBLE_FULLDISK,              "Visible Full Disk") },
                { (int) ScannerSubProduct.VISIBLE_NORTHERN,             new NOAASubproduct(ScannerSubProduct.VISIBLE_NORTHERN,              "Visible Northern Hemisphere") },
                { (int) ScannerSubProduct.VISIBLE_SOUTHERN,             new NOAASubproduct(ScannerSubProduct.VISIBLE_SOUTHERN,              "Visible Southern Hemisphere") },
                { (int) ScannerSubProduct.VISIBLE_UNITEDSTATES,         new NOAASubproduct(ScannerSubProduct.VISIBLE_UNITEDSTATES,          "Visible United States") },
                { (int) ScannerSubProduct.VISIBLE_AREA_OF_INTEREST,     new NOAASubproduct(ScannerSubProduct.VISIBLE_AREA_OF_INTEREST,      "Visible Area of Interest") },
                { (int) ScannerSubProduct.WATERVAPOUR_FULLDISK,         new NOAASubproduct(ScannerSubProduct.WATERVAPOUR_FULLDISK,          "Water Vapour Full Disk") },
                { (int) ScannerSubProduct.WATERVAPOUR_NORTHERN,         new NOAASubproduct(ScannerSubProduct.WATERVAPOUR_NORTHERN,          "Water Vapour Northern Hemisphere") },
                { (int) ScannerSubProduct.WATERVAPOUR_SOUTHERN,         new NOAASubproduct(ScannerSubProduct.WATERVAPOUR_SOUTHERN,          "Water Vapour Southern Hemisphere") },
                { (int) ScannerSubProduct.WATERVAPOUR_UNITEDSTATES,     new NOAASubproduct(ScannerSubProduct.WATERVAPOUR_UNITEDSTATES,      "Water Vapour United States") },
                { (int) ScannerSubProduct.WATERVAPOUR_AREA_OF_INTEREST, new NOAASubproduct(ScannerSubProduct.WATERVAPOUR_AREA_OF_INTEREST,  "Water Vapour Area of Interest") }
            }));

            noaaProducts.Add((int)NOAAProductID.GOES15_ABI, new NOAAProduct(NOAAProductID.GOES15_ABI, "GOES 15 ABI", new Dictionary<int, NOAASubproduct>() { // So far, only received GOES 15 images. Coecidence? 
                { (int) ScannerSubProduct.NONE,                         new NOAASubproduct(ScannerSubProduct.NONE,                          "None") },
                { (int) ScannerSubProduct.INFRARED_FULLDISK,            new NOAASubproduct(ScannerSubProduct.INFRARED_FULLDISK,             "Infrared Full Disk") },
                { (int) ScannerSubProduct.INFRARED_NORTHERN,            new NOAASubproduct(ScannerSubProduct.INFRARED_NORTHERN,             "Infrared Northern Hemisphere") },
                { (int) ScannerSubProduct.INFRARED_SOUTHERN,            new NOAASubproduct(ScannerSubProduct.INFRARED_SOUTHERN,             "Infrared Southern Hemisphere") },
                { (int) ScannerSubProduct.INFRARED_UNITEDSTATES,        new NOAASubproduct(ScannerSubProduct.INFRARED_UNITEDSTATES,         "Infrared United States") },
                { (int) ScannerSubProduct.INFRARED_AREA_OF_INTEREST,    new NOAASubproduct(ScannerSubProduct.INFRARED_AREA_OF_INTEREST,     "Infrared Area of Interest") },
                { (int) ScannerSubProduct.VISIBLE_FULLDISK,             new NOAASubproduct(ScannerSubProduct.VISIBLE_FULLDISK,              "Visible Full Disk") },
                { (int) ScannerSubProduct.VISIBLE_NORTHERN,             new NOAASubproduct(ScannerSubProduct.VISIBLE_NORTHERN,              "Visible Northern Hemisphere") },
                { (int) ScannerSubProduct.VISIBLE_SOUTHERN,             new NOAASubproduct(ScannerSubProduct.VISIBLE_SOUTHERN,              "Visible Southern Hemisphere") },
                { (int) ScannerSubProduct.VISIBLE_UNITEDSTATES,         new NOAASubproduct(ScannerSubProduct.VISIBLE_UNITEDSTATES,          "Visible United States") },
                { (int) ScannerSubProduct.VISIBLE_AREA_OF_INTEREST,     new NOAASubproduct(ScannerSubProduct.VISIBLE_AREA_OF_INTEREST,      "Visible Area of Interest") },
                { (int) ScannerSubProduct.WATERVAPOUR_FULLDISK,         new NOAASubproduct(ScannerSubProduct.WATERVAPOUR_FULLDISK,          "Water Vapour Full Disk") },
                { (int) ScannerSubProduct.WATERVAPOUR_NORTHERN,         new NOAASubproduct(ScannerSubProduct.WATERVAPOUR_NORTHERN,          "Water Vapour Northern Hemisphere") },
                { (int) ScannerSubProduct.WATERVAPOUR_SOUTHERN,         new NOAASubproduct(ScannerSubProduct.WATERVAPOUR_SOUTHERN,          "Water Vapour Southern Hemisphere") },
                { (int) ScannerSubProduct.WATERVAPOUR_UNITEDSTATES,     new NOAASubproduct(ScannerSubProduct.WATERVAPOUR_UNITEDSTATES,      "Water Vapour United States") },
                { (int) ScannerSubProduct.WATERVAPOUR_AREA_OF_INTEREST, new NOAASubproduct(ScannerSubProduct.WATERVAPOUR_AREA_OF_INTEREST,  "Water Vapour Area of Interest") }
            }));

            noaaProducts.Add((int)NOAAProductID.GOES16_ABI, new NOAAProduct(NOAAProductID.GOES16_ABI, "GOES 16 ABI", new Dictionary<int, NOAASubproduct>() {
                { (int) ScannerSubProduct.NONE,                         new NOAASubproduct(ScannerSubProduct.NONE,                          "None") },
                {                            1,                         new NOAASubproduct(                     1,                          "Channel 1") },
                {                            2,                         new NOAASubproduct(                     2,                          "Channel 2") },
                {                            3,                         new NOAASubproduct(                     3,                          "Channel 3") },
                {                            4,                         new NOAASubproduct(                     4,                          "Channel 4") },
                {                            5,                         new NOAASubproduct(                     5,                          "Channel 5") },
                {                            6,                         new NOAASubproduct(                     6,                          "Channel 6") },
                {                            7,                         new NOAASubproduct(                     7,                          "Channel 7") },
                {                            8,                         new NOAASubproduct(                     8,                          "Channel 8") },
                {                            9,                         new NOAASubproduct(                     9,                          "Channel 9") },
                {                           10,                         new NOAASubproduct(                    10,                          "Channel 10") },
                {                           11,                         new NOAASubproduct(                    11,                          "Channel 11") },
                {                           12,                         new NOAASubproduct(                    12,                          "Channel 12") },
                {                           13,                         new NOAASubproduct(                    13,                          "Channel 13") },
                {                           14,                         new NOAASubproduct(                    14,                          "Channel 14") },
                {                           15,                         new NOAASubproduct(                    15,                          "Channel 15") },
                {                           16,                         new NOAASubproduct(                    16,                          "Channel 16") }
            }));

            noaaProducts.Add((int)NOAAProductID.GOES17_ABI, new NOAAProduct(NOAAProductID.GOES17_ABI, "GOES 17 ABI", new Dictionary<int, NOAASubproduct>() {
                { (int) ScannerSubProduct.NONE,                         new NOAASubproduct(ScannerSubProduct.NONE,                          "None") },
                {                            1,                         new NOAASubproduct(                     1,                          "Channel 1") },
                {                            2,                         new NOAASubproduct(                     2,                          "Channel 2") },
                {                            3,                         new NOAASubproduct(                     3,                          "Channel 3") },
                {                            4,                         new NOAASubproduct(                     4,                          "Channel 4") },
                {                            5,                         new NOAASubproduct(                     5,                          "Channel 5") },
                {                            6,                         new NOAASubproduct(                     6,                          "Channel 6") },
                {                            7,                         new NOAASubproduct(                     7,                          "Channel 7") },
                {                            8,                         new NOAASubproduct(                     8,                          "Channel 8") },
                {                            9,                         new NOAASubproduct(                     9,                          "Channel 9") },
                {                           10,                         new NOAASubproduct(                    10,                          "Channel 10") },
                {                           11,                         new NOAASubproduct(                    11,                          "Channel 11") },
                {                           12,                         new NOAASubproduct(                    12,                          "Channel 12") },
                {                           13,                         new NOAASubproduct(                    13,                          "Channel 13") },
                {                           14,                         new NOAASubproduct(                    14,                          "Channel 14") },
                {                           15,                         new NOAASubproduct(                    15,                          "Channel 15") },
                {                           16,                         new NOAASubproduct(                    16,                          "Channel 16") }
            }));

            noaaProducts.Add((int)NOAAProductID.HIMAWARI8_ABI, new NOAAProduct(NOAAProductID.HIMAWARI8_ABI, "HIMAWARI8 ABI", new Dictionary<int, NOAASubproduct>() {
                { (int) ScannerSubProduct.NONE,                         new NOAASubproduct(ScannerSubProduct.NONE,                          "None") },
                {                            1,                         new NOAASubproduct(                     1,                          "Channel 1") },
                {                            2,                         new NOAASubproduct(                     2,                          "Channel 2") },
                {                            3,                         new NOAASubproduct(                     3,                          "Channel 3") },
                {                            4,                         new NOAASubproduct(                     4,                          "Channel 4") },
                {                            5,                         new NOAASubproduct(                     5,                          "Channel 5") },
                {                            6,                         new NOAASubproduct(                     6,                          "Channel 6") },
                {                            7,                         new NOAASubproduct(                     7,                          "Channel 7") },
                {                            8,                         new NOAASubproduct(                     8,                          "Channel 8") },
                {                            9,                         new NOAASubproduct(                     9,                          "Channel 9") },
                {                           10,                         new NOAASubproduct(                    10,                          "Channel 10") },
                {                           11,                         new NOAASubproduct(                    11,                          "Channel 11") },
                {                           12,                         new NOAASubproduct(                    12,                          "Channel 12") },
                {                           13,                         new NOAASubproduct(                    13,                          "Channel 13") },
                {                           14,                         new NOAASubproduct(                    14,                          "Channel 14") },
                {                           15,                         new NOAASubproduct(                    15,                          "Channel 15") },
                {                           16,                         new NOAASubproduct(                    16,                          "Channel 16") }
            }));

            noaaProducts.Add((int)NOAAProductID.EMWIN, new NOAAProduct(NOAAProductID.EMWIN, "EMWIN"));
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


using System;
using System.Collections.Generic;
using OpenSatelliteProject.PacketData.Enums;

namespace OpenSatelliteProject.PacketData {
    public static class Presets {

        private readonly static Dictionary<int, NOAAProduct> noaaProducts;

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

            noaaProducts.Add((int)NOAAProductID.SCANNER_DATA_1, new NOAAProduct(NOAAProductID.SCANNER_DATA_1, "Scanner Image", new Dictionary<int, NOAASubproduct>() { // So far, only received GOES 13 images. Coecidence?
                { (int) ScannerSubProduct.NONE,                         new NOAASubproduct(ScannerSubProduct.NONE,                          "N(int) one") },
                { (int) ScannerSubProduct.INFRARED_FULLDISK,            new NOAASubproduct(ScannerSubProduct.INFRARED_FULLDISK,             "I(int) nfrared Full Disk") },
                { (int) ScannerSubProduct.INFRARED_NORTHERN,            new NOAASubproduct(ScannerSubProduct.INFRARED_NORTHERN,             "I(int) nfrared Northern Hemisphere") },
                { (int) ScannerSubProduct.INFRARED_SOUTHERN,            new NOAASubproduct(ScannerSubProduct.INFRARED_SOUTHERN,             "I(int) nfrared Southern Hemisphere") },
                { (int) ScannerSubProduct.INFRARED_UNITEDSTATES,        new NOAASubproduct(ScannerSubProduct.INFRARED_UNITEDSTATES,         "I(int) nfrared United States") },
                { (int) ScannerSubProduct.INFRARED_AREA_OF_INTEREST,    new NOAASubproduct(ScannerSubProduct.INFRARED_AREA_OF_INTEREST,     "I(int) nfrared Area of Interest") },
                { (int) ScannerSubProduct.VISIBLE_FULLDISK,             new NOAASubproduct(ScannerSubProduct.VISIBLE_FULLDISK,              "V(int) isible Full Disk") },
                { (int) ScannerSubProduct.VISIBLE_NORTHERN,             new NOAASubproduct(ScannerSubProduct.VISIBLE_NORTHERN,              "V(int) isible Northern Hemisphere") },
                { (int) ScannerSubProduct.VISIBLE_SOUTHERN,             new NOAASubproduct(ScannerSubProduct.VISIBLE_SOUTHERN,              "V(int) isible Southern Hemisphere") },
                { (int) ScannerSubProduct.VISIBLE_UNITEDSTATES,         new NOAASubproduct(ScannerSubProduct.VISIBLE_UNITEDSTATES,          "V(int) isible United States") },
                { (int) ScannerSubProduct.VISIBLE_AREA_OF_INTEREST,     new NOAASubproduct(ScannerSubProduct.VISIBLE_AREA_OF_INTEREST,      "V(int) isible Area of Interest") },
                { (int) ScannerSubProduct.WATERVAPOUR_FULLDISK,         new NOAASubproduct(ScannerSubProduct.WATERVAPOUR_FULLDISK,          "W(int) ater Vapour Full Disk") },
                { (int) ScannerSubProduct.WATERVAPOUR_NORTHERN,         new NOAASubproduct(ScannerSubProduct.WATERVAPOUR_NORTHERN,          "W(int) ater Vapour Northern Hemisphere") },
                { (int) ScannerSubProduct.WATERVAPOUR_SOUTHERN,         new NOAASubproduct(ScannerSubProduct.WATERVAPOUR_SOUTHERN,          "W(int) ater Vapour Southern Hemisphere") },
                { (int) ScannerSubProduct.WATERVAPOUR_UNITEDSTATES,     new NOAASubproduct(ScannerSubProduct.WATERVAPOUR_UNITEDSTATES,      "W(int) ater Vapour United States") },
                { (int) ScannerSubProduct.WATERVAPOUR_AREA_OF_INTEREST, new NOAASubproduct(ScannerSubProduct.WATERVAPOUR_AREA_OF_INTEREST,  "Water Vapour Area of Interest") }
            }));

            noaaProducts.Add((int)NOAAProductID.SCANNER_DATA_2, new NOAAProduct(NOAAProductID.SCANNER_DATA_2, "Scanner Image", new Dictionary<int, NOAASubproduct>() { // So far, only received GOES 15 images. Coecidence? 
                { (int) ScannerSubProduct.NONE,                         new NOAASubproduct(ScannerSubProduct.NONE,                          "N(int) one") },
                { (int) ScannerSubProduct.INFRARED_FULLDISK,            new NOAASubproduct(ScannerSubProduct.INFRARED_FULLDISK,             "I(int) nfrared Full Disk") },
                { (int) ScannerSubProduct.INFRARED_NORTHERN,            new NOAASubproduct(ScannerSubProduct.INFRARED_NORTHERN,             "I(int) nfrared Northern Hemisphere") },
                { (int) ScannerSubProduct.INFRARED_SOUTHERN,            new NOAASubproduct(ScannerSubProduct.INFRARED_SOUTHERN,             "I(int) nfrared Southern Hemisphere") },
                { (int) ScannerSubProduct.INFRARED_UNITEDSTATES,        new NOAASubproduct(ScannerSubProduct.INFRARED_UNITEDSTATES,         "I(int) nfrared United States") },
                { (int) ScannerSubProduct.INFRARED_AREA_OF_INTEREST,    new NOAASubproduct(ScannerSubProduct.INFRARED_AREA_OF_INTEREST,     "I(int) nfrared Area of Interest") },
                { (int) ScannerSubProduct.VISIBLE_FULLDISK,             new NOAASubproduct(ScannerSubProduct.VISIBLE_FULLDISK,              "V(int) isible Full Disk") },
                { (int) ScannerSubProduct.VISIBLE_NORTHERN,             new NOAASubproduct(ScannerSubProduct.VISIBLE_NORTHERN,              "V(int) isible Northern Hemisphere") },
                { (int) ScannerSubProduct.VISIBLE_SOUTHERN,             new NOAASubproduct(ScannerSubProduct.VISIBLE_SOUTHERN,              "V(int) isible Southern Hemisphere") },
                { (int) ScannerSubProduct.VISIBLE_UNITEDSTATES,         new NOAASubproduct(ScannerSubProduct.VISIBLE_UNITEDSTATES,          "V(int) isible United States") },
                { (int) ScannerSubProduct.VISIBLE_AREA_OF_INTEREST,     new NOAASubproduct(ScannerSubProduct.VISIBLE_AREA_OF_INTEREST,      "V(int) isible Area of Interest") },
                { (int) ScannerSubProduct.WATERVAPOUR_FULLDISK,         new NOAASubproduct(ScannerSubProduct.WATERVAPOUR_FULLDISK,          "W(int) ater Vapour Full Disk") },
                { (int) ScannerSubProduct.WATERVAPOUR_NORTHERN,         new NOAASubproduct(ScannerSubProduct.WATERVAPOUR_NORTHERN,          "W(int) ater Vapour Northern Hemisphere") },
                { (int) ScannerSubProduct.WATERVAPOUR_SOUTHERN,         new NOAASubproduct(ScannerSubProduct.WATERVAPOUR_SOUTHERN,          "W(int) ater Vapour Southern Hemisphere") },
                { (int) ScannerSubProduct.WATERVAPOUR_UNITEDSTATES,     new NOAASubproduct(ScannerSubProduct.WATERVAPOUR_UNITEDSTATES,      "W(int) ater Vapour United States") },
                { (int) ScannerSubProduct.WATERVAPOUR_AREA_OF_INTEREST, new NOAASubproduct(ScannerSubProduct.WATERVAPOUR_AREA_OF_INTEREST,  "Water Vapour Area of Interest") }
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


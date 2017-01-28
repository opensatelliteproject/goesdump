using System;
using OpenSatelliteProject.PacketData.Structs;
using OpenSatelliteProject.PacketData.Enums;

namespace OpenSatelliteProject.PacketData {
    public class NOAASpecificHeader: XRitBaseHeader {

        public string Signature { get; set; }
        public NOAAProduct Product { get; set; }
        public NOAASubproduct SubProduct { get; set; }
        public UInt16 Parameter { get; set; }
        public CompressionType Compression;

        public NOAASpecificHeader(NOAASpecificRecord data) {
            Type = HeaderType.NOAASpecificHeader;
            Signature = data.Signature;
            Product = Presets.GetProductById(data.ProductID);
            SubProduct = Product.getSubProduct(data.ProductSubID);
            Parameter = data.Parameter;
            Compression = (CompressionType)data.Compression;
        }
    }
}


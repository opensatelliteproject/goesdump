using System.Linq;
using OpenSatelliteProject.PacketData.Enums;
using OpenSatelliteProject.PacketData;

namespace OpenSatelliteProject {
    public class Head9Header: XRITBaseHeader {

        public byte[] Data { get; set; }
        public string FileName { get; set; }

        public Head9Header(Head9 data) {
            Type = HeaderType.Head9;
            Data = data.Data;
        }
    }
}

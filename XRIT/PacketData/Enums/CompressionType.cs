using System;

namespace OpenSatelliteProject.PacketData {
    public enum CompressionType {
        NO_COMPRESSION = 0,
        LRIT_RICE = 1,
        JPEG = 2,
        GIF = 5,
        ZIP = 10
    }
}


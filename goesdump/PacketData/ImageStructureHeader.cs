using System;

namespace OpenSatelliteProject {
    public struct ImageStructureHeader {
        byte BitsPerPixel;
        UInt16 Columns;
        UInt16 Lines;
        byte Compression;
    }
}


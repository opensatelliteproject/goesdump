using System;

namespace OpenSatelliteProject {
    public struct FirstHeader {
        byte FileTypeCode;
        UInt32 HeaderLength;
        UInt64 DataLength;
    }
}


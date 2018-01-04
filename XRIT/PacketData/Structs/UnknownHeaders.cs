using System;
namespace OpenSatelliteProject {
    /**
     *  Head with code 9, found on GOES-15 after GOES-16 transition.
     *  It's a weird header, but consistent. Under LRIT spec is reserved.
     */
    public struct Head9 {
        public byte type;
        public byte[] Data;
    }
}

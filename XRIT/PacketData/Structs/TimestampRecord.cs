using System;
using System.Runtime.InteropServices;

namespace OpenSatelliteProject.PacketData.Structs {

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct TimestampRecord {
        public byte type;
        public UInt16 size;
        public UInt16 Days;
        public UInt32 Milisseconds;

        public DateTime getDateTime() {
            DateTime t = new DateTime(1958, 1, 1);
            t.AddDays(Days);
            t.AddMilliseconds(Milisseconds);
            return t;
        }
    }
}


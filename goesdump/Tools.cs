using System;

namespace OpenSatelliteProject {
    public static class Tools {
        public static DateTime UnixTimeStampToDateTime(long unixTimeStamp) {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }

        public static int CalcCRC(byte[] data) {
            byte lsb = 0xFF, msb = 0xFF, x;

            foreach (byte b in data) {
                x = (byte)(b ^ msb);
                x ^= (byte)(x >> 4);
                msb = (byte)(lsb ^ (x >> 3) ^ (x << 4));
                lsb = (byte)(x ^ (x << 5));
            }

            return (((int)msb) << 8) + lsb;
        }

        public static int CRC(this byte[] data) {
            return CalcCRC(data);
        }
    }
}


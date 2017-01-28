using System;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Linq;

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

        public static T ByteArrayToStruct<T>(byte[] bytes) where T: struct {
            GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            T stuff = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
            handle.Free();
            return stuff;
        }

        public static T StructToSystemEndian<T>(T data) where T: struct {
            if (BitConverter.IsLittleEndian) {
                Type tType = typeof(T);
                FieldInfo[] fi = tType.GetFields(BindingFlags.Public | BindingFlags.Instance);
                foreach (FieldInfo info in fi) {
                    switch (info.FieldType) {
                        case typeof(UInt16):
                            UInt16 u16 = (UInt16)info.GetValue(data);
                            byte[] bu16 = BitConverter.GetBytes(u16).Reverse().ToArray();
                            info.SetValue(data, BitConverter.ToUInt16(bu16));
                            break;
                        case typeof(UInt32):
                            UInt16 u32 = (UInt16)info.GetValue(data);
                            byte[] bu32 = BitConverter.GetBytes(u32).Reverse().ToArray();
                            info.SetValue(data, BitConverter.ToUInt16(bu32));
                            break;
                        case typeof(UInt64):
                            UInt16 u64 = (UInt16)info.GetValue(data);
                            byte[] bu64 = BitConverter.GetBytes(u64).Reverse().ToArray();
                            info.SetValue(data, BitConverter.ToUInt16(bu64));
                            break;
                        case typeof(Int16):
                            UInt16 i16 = (UInt16)info.GetValue(data);
                            byte[] b16 = BitConverter.GetBytes(i16).Reverse().ToArray();
                            info.SetValue(data, BitConverter.ToUInt16(b16));
                            break;
                        case typeof(Int32):
                            UInt16 i32 = (UInt16)info.GetValue(data);
                            byte[] b32 = BitConverter.GetBytes(i32).Reverse().ToArray();
                            info.SetValue(data, BitConverter.ToUInt16(b32));
                            break;
                        case typeof (Int64):
                            UInt16 i64 = (UInt16)info.GetValue(data);
                            byte[] b64 = BitConverter.GetBytes(i64).Reverse().ToArray();
                            info.SetValue(data, BitConverter.ToUInt16(b64));
                            break;
                    }
                    Console.WriteLine(info.Name);
                }
            }

            return data;
        }
    }
}


using System;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

namespace OpenSatelliteProject.Tools {
    public static class LLTools {
        private static readonly string[] hrsSuffix = { "B", "KB", "MB", "GB", "TB", "PB", "EB" };
        public static bool IsLinux {
            get {
                int p = (int)Environment.OSVersion.Platform;
                return (p == 4) || (p == 6) || (p == 128);
            }
        }

        public static String BytesToString(long byteCount) {
            
            if (byteCount == 0)
                return "0" + hrsSuffix[0];
            long bytes = Math.Abs(byteCount);
            int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return (Math.Sign(byteCount) * num).ToString() + hrsSuffix[place];
        }

        public static DateTime UnixTimeStampToDateTime(long unixTimeStamp) {
            // Unix timestamp is seconds past epoch
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dtDateTime;
        }

        public static List<T> Clone<T>(this List<T> listToClone) where T: ICloneable {
            return listToClone.Select(item => (T)item.Clone()).ToList();
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
            object d = data;
            if (BitConverter.IsLittleEndian) {
                Type tType = typeof(T);
                FieldInfo[] fi = tType.GetFields(BindingFlags.Public | BindingFlags.Instance);
                foreach (FieldInfo info in fi) {
                    if (info.FieldType == typeof(UInt16)) {
                        UInt16 u16 = (UInt16)info.GetValue(data);
                        byte[] bu16 = BitConverter.GetBytes(u16);
                        Array.Reverse(bu16);
                        info.SetValue(d, BitConverter.ToUInt16(bu16, 0));
                    } else if (info.FieldType == typeof(UInt32)) {
                        UInt32 u32 = (UInt32)info.GetValue(data);
                        byte[] bu32 = BitConverter.GetBytes(u32);
                        Array.Reverse(bu32);
                        info.SetValue(d, BitConverter.ToUInt32(bu32, 0));
                    } else if (info.FieldType == typeof(UInt64)) {
                        UInt64 u64 = (UInt64)info.GetValue(data);
                        byte[] bu64 = BitConverter.GetBytes(u64);
                        Array.Reverse(bu64);
                        info.SetValue(d, BitConverter.ToUInt64(bu64, 0));
                    } else if (info.FieldType == typeof(Int16)) {
                        Int16 i16 = (Int16)info.GetValue(data);
                        byte[] b16 = BitConverter.GetBytes(i16);
                        Array.Reverse(b16);
                        info.SetValue(d, BitConverter.ToInt16(b16, 0));
                    } else if (info.FieldType == typeof(Int32)) {
                        Int32 i32 = (Int32)info.GetValue(data);
                        byte[] b32 = BitConverter.GetBytes(i32);
                        Array.Reverse(b32);
                        info.SetValue(d, BitConverter.ToInt32(b32, 0));
                    } else if (info.FieldType == typeof(Int64)) {
                        Int64 i64 = (Int64)info.GetValue(data);
                        byte[] b64 = BitConverter.GetBytes(i64);
                        Array.Reverse(b64);
                        info.SetValue(d, BitConverter.ToInt64(b64, 0));
                    }
                }
            }

            return (T)d;
        }
    }
}


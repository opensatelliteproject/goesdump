using System;
using System.Linq;

namespace OpenSatelliteProject {
    public class MSDU {
        #region Properties
        public int Version { get; set; }

        public int SHF { get; set; }

        public int APID { get; set; }

        public int Type { get; set; }

        public bool SecondHeader { get; set; }

        public SequenceType Sequence { get; set; }

        public int PacketNumber { get; set; }

        public int PacketLength { get; set; }

        public byte[] Data { get; set; }

        public byte[] RemainingData { get; set; }

        public bool FrameLost { get; set; }

        public bool Full { 
            get {
                return Data.Length == PacketLength + 2;
            } 
        }

        public int CRC {
            get {
                byte[] o = Data.Skip(Data.Length - 2).ToArray();
                if (BitConverter.IsLittleEndian) {
                    Array.Reverse(o);
                }
                return BitConverter.ToUInt16(o, 0);
            }
        }

        public bool Valid {
            get {
                return Data.Take(Data.Length - 2).ToArray().CRC() == CRC;
            }
        }

        public bool FillPacket {
            get {
                return APID == 2047;
            }
        }
        #endregion

        #region Constructor / Destructor
        private MSDU() {
        }

        #endregion

        #region Methods
        public void addDataBytes(byte[] data) {            
            if (data.Length + Data.Length > PacketLength + 2) {
                UIConsole.GlobalConsole.Warn("Overflow in MSDU!");
            }
            byte[] newData = new byte[Data.Length + data.Length];
            Array.Copy(Data, newData, Data.Length);
            Array.Copy(data, 0, newData, Data.Length, data.Length);
            if (newData.Length > PacketLength + 2) {
                Data = newData.Take(PacketLength + 2).ToArray();
            } else {
                Data = newData;
            }

        }
        #endregion


        #region Builders / Parsers
        public static MSDU parseMSDU(byte[] data) {
            MSDU msdu = new MSDU();

            byte[] ob = data.Take(2).ToArray();
            if (BitConverter.IsLittleEndian) {
                Array.Reverse(ob);
            }

            UInt16 o = BitConverter.ToUInt16(ob, 0);

            msdu.Version = (o & 0xE000) >> 13;
            msdu.Type = (o & 0x1000) >> 12;
            msdu.SecondHeader = ((o & 0x800) >> 11) > 0;
            msdu.APID = o & 0x7FF;


            ob = data.Skip(2).Take(2).ToArray();
            if (BitConverter.IsLittleEndian) {
                Array.Reverse(ob);
            }

            o = BitConverter.ToUInt16(ob, 0);

            msdu.Sequence = (SequenceType)((o & 0xC000) >> 14);
            msdu.PacketNumber = (o & 0x3FFF);

            ob = data.Skip(4).Take(2).ToArray();
            if (BitConverter.IsLittleEndian) {
                Array.Reverse(ob);
            }

            msdu.PacketLength = BitConverter.ToUInt16(ob, 0) - 1;
            data = data.Skip(6).ToArray();
            if (data.Length > msdu.PacketLength + 2) {
                msdu.RemainingData = data.Skip(msdu.PacketLength + 2).ToArray();
                data = data.Take(msdu.PacketLength + 2).ToArray();
            } else {
                msdu.RemainingData = new byte[0];
            }

            msdu.Data = data.ToArray();

            msdu.FrameLost = false;

            return msdu;
        }
        #endregion
    }
}


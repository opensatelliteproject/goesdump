using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Diagnostics;


namespace OpenSatelliteProject {
    public class Demuxer {
        private readonly int FRAMESIZE = 892;

        private Dictionary<int, MSDU> temporaryStorage;

        private int lastAPID;
        private int lastFrame;
        private long frameDrops;
        private int startnum = -1;
        private int endnum = -1;
        private string filename;
        private int channelId;
        private bool isCompressed;
        private int pixels;

        private byte[] buffer;

        public int CRCFails { get; set; }

        public int Bugs { get; set; }

        public int Packets { get; set; }

        public Demuxer() {
            temporaryStorage = new Dictionary<int, MSDU>();
            buffer = new byte[0];
            lastAPID = -1;
            lastFrame = -1;
            frameDrops = 0;
            CRCFails = 0;
            Bugs = 0;
            Packets = 0;
        }

        public Tuple<int, byte[]> CreatePacket(byte[] data) {
            int apid = -1;
            while (true) {
                if (data.Length < 6) {
                    return Tuple.Create(-1, data);
                }

                MSDU msdu = MSDU.parseMSDU(data);

                if (msdu.APID != 2047) {
                    temporaryStorage[msdu.APID] = msdu;
                    apid = msdu.APID;
                } else {
                    apid = -1;
                }

                if (msdu.RemainingData.Length > 0 || msdu.Full) {
                    data = msdu.RemainingData;
                    msdu.RemainingData = new byte[0];
                    FinishMSDU(msdu);
                    temporaryStorage.Remove(msdu.APID);
                    apid = -1;
                } else {
                    break;
                }
            }
            return Tuple.Create(apid, new byte[0]);
        }

        public void FinishMSDU(MSDU msdu) {
            try {
                if (msdu.APID == 2047) {
                    // Skip fill packet
                    return;
                }

                bool firstOrSinglePacket = msdu.Sequence == SequenceType.FIRST_SEGMENT || msdu.Sequence == SequenceType.SINGLE_DATA;

                Packets++;

                if (!msdu.Valid) {
                    CRCFails++;
                }

                if (!msdu.Valid || !msdu.Full) {
                    UIConsole.GlobalConsole.Error("Got a invalid MSDU :(");
                    UIConsole.GlobalConsole.Debug(String.Format("New Packet for APID {0} - Valid CRC: {1} - Full: {2} - Remaining Bytes: {3} - Frame Lost: {4}", msdu.APID, msdu.Valid, msdu.Full, msdu.RemainingData.Length, msdu.FrameLost));
                    UIConsole.GlobalConsole.Debug(String.Format("\t\tTotal Size: {0} Current Size: {1}", msdu.PacketLength + 2, msdu.Data.Length)); 
                }

                if (msdu.Sequence == SequenceType.FIRST_SEGMENT) {
                    isCompressed = PacketManager.IsCompressed(msdu.Data.Skip(10).ToArray());
                    pixels = PacketManager.GetPixels(msdu.Data.Skip(10).ToArray());
                    startnum = msdu.PacketNumber;
                } else if (msdu.Sequence == SequenceType.LAST_SEGMENT) {
                    endnum = msdu.PacketNumber;

                    if (startnum == -1) {
                        UIConsole.GlobalConsole.Debug("Orphan Packet. Dropping");
                        return;
                    }
                } else if (msdu.Sequence != SequenceType.SINGLE_DATA && startnum == -1) {
                    UIConsole.GlobalConsole.Debug("Orphan Packet. Dropping");
                    return;
                }

                string path = String.Format("channels/{0}", channelId);
                if (!Directory.Exists(path)) {
                    Directory.CreateDirectory(path);
                }

                if (isCompressed) {
                    filename = String.Format("channels/{0}/{1}_{2}_{3}.lrit", channelId, msdu.APID, msdu.Version, msdu.PacketNumber);
                } else {
                    filename = String.Format("channels/{0}/{1}_{2}.lrit", channelId, msdu.APID, msdu.Version);
                }

                using (FileStream fs = new FileStream(filename, firstOrSinglePacket || isCompressed ? FileMode.Create : FileMode.Append, FileAccess.Write)) {
                    using (BinaryWriter sw = new BinaryWriter(fs)) {
                        byte[] dataToSave = msdu.Data.Skip(firstOrSinglePacket ? 10 : 0).Take(firstOrSinglePacket ? msdu.PacketLength - 10 : msdu.PacketLength).ToArray(); 
                        sw.Write(dataToSave);
                    }
                }

                if (msdu.Sequence == SequenceType.LAST_SEGMENT || msdu.Sequence == SequenceType.SINGLE_DATA) {
                    if (isCompressed) {
                        string decompressed = PacketManager.Decompressor(String.Format("channels/{0}/{1}_{2}_", channelId, msdu.APID, msdu.Version), pixels, startnum, endnum);
                        PacketManager.ManageFile(decompressed);
                        startnum = -1;
                        endnum = -1;
                    } else {
                        PacketManager.ManageFile(filename);
                    }
                }
            } catch (Exception e) {
                UIConsole.GlobalConsole.Error(String.Format("Exception on FinishMSDU: {0}", e));
            }
        }

        public void ParseBytes(byte[] data) {
            uint counter;

            if (data.Length < FRAMESIZE) {
                throw new Exception(String.Format("Not enough data. Expected {0} and got {1}", FRAMESIZE, data.Length));
            }

            channelId = (data[1] & 0x3F);

            byte[] cb = data.Skip(2).Take(4).ToArray();
            if (BitConverter.IsLittleEndian) {
                Array.Reverse(cb);
            }

            counter = BitConverter.ToUInt32(cb, 0);
            counter &= 0xFFFFFF00;
            counter >>= 8;

            if (lastFrame != -1 && lastFrame + 1 != counter) {
                UIConsole.GlobalConsole.Error(String.Format("Lost {0} frames.", counter - lastFrame - 1));
                if (lastAPID != -1) {
                    temporaryStorage[lastAPID].FrameLost = true;
                }
            }

            if (lastFrame != -1) {
                frameDrops += counter - lastFrame - 1;
            }

            cb = data.Skip(6).Take(2).ToArray();
            if (BitConverter.IsLittleEndian) {
                Array.Reverse(cb);
            }

            int fhp = BitConverter.ToUInt16(cb, 0) & 0x7FF;

            data = data.Skip(8).ToArray();

            // Data is now TP_PDU
            var p = Tuple.Create(0, new byte[0]);
            if (fhp != 2047) { // Has a packet start
                if (lastAPID == -1 && buffer.Length > 0) {
                    //  There was not enough data to packetize last time. So lets fill the buffer until the fhp and create packet.
                    if (fhp > 0) {
                        buffer = buffer.Concat(data.Take(fhp)).ToArray();
                    }           

                    p = CreatePacket(buffer);
                    lastAPID = p.Item1;

                    if (lastAPID == -1) {
                        buffer = p.Item2;
                    } else {
                        buffer = new byte[0];
                    }

                } 

                if (lastAPID != -1) {
                    if (fhp > 0) {
                        temporaryStorage[lastAPID].addDataBytes(buffer.Concat(data.Take(fhp)).ToArray());
                    }

                    if (!temporaryStorage[lastAPID].Full) {
                        Bugs++;
                        StackFrame callStack = new StackFrame(1, true);
                        UIConsole.GlobalConsole.Debug(String.Format("Problem at line {0} in file {1}! Not full! Check code for bugs!", callStack.GetFileLineNumber(), callStack.GetFileName()));
                    }
                    FinishMSDU(temporaryStorage[lastAPID]);
                    temporaryStorage.Remove(lastAPID);
                    lastAPID = -1;
                }

                buffer = buffer.Concat(data.Skip(fhp)).ToArray();
                p = CreatePacket(buffer);
                lastAPID = p.Item1;
                if (lastAPID == -1) {
                    buffer = p.Item2;
                } else {
                    buffer = new byte[0];
                }
            } else {
                if (buffer.Length > 0 && lastAPID != -1) {
                    buffer = buffer.Concat(data).ToArray();
                    p = CreatePacket(buffer);
                    lastAPID = p.Item1;
                    if (lastAPID == -1) {
                        buffer = p.Item2;
                    } else {
                        buffer = new byte[0];
                    }
                } else if (lastAPID == -1) {
                    buffer = buffer.Concat(data).ToArray();
                    p = CreatePacket(buffer);
                    lastAPID = p.Item1;
                    if (lastAPID == -1) {
                        buffer = p.Item2;
                    } else {
                        buffer = new byte[0];
                    }
                } else {
                    temporaryStorage[lastAPID].addDataBytes(data);
                }
            }
        }
    }
}


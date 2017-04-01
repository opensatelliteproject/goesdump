using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Diagnostics;
using OpenSatelliteProject.PacketData;
using OpenSatelliteProject.Tools;


namespace OpenSatelliteProject {
    public class Demuxer {
        private readonly int FRAMESIZE = 892;

        private Dictionary<int, MSDU> temporaryStorage;

        private int lastAPID;
        private int lastFrame;
        private int startnum = -1;
        private int endnum = -1;
        private string filename;
        private int channelId;
        private XRITHeader fileHeader;
        private byte[] buffer;
        private DemuxManager manager;
        private MSDU lastMSDU;
       

        public int CRCFails { get; set; }
        public int Bugs { get; set; }
        public int Packets { get; set; }
        public int LengthFails { get; set; }
        public long FrameLoss { get; set; }

        public Demuxer() {
            temporaryStorage = new Dictionary<int, MSDU>();
            buffer = new byte[0];
            lastAPID = -1;
            lastFrame = -1;
            FrameLoss = 0;
            LengthFails = 0;
            CRCFails = 0;
            Bugs = 0;
            Packets = 0;
            manager = null;
        }

        public Demuxer(DemuxManager manager) : this() {
            this.manager = manager;
        }

        public Tuple<int, byte[]> CreatePacket(byte[] data) {
            int apid = -1;
            while (true) {
                if (data.Length < 6) {
                    return Tuple.Create(-1, data);
                }

                MSDU msdu = MSDU.parseMSDU(data);

                temporaryStorage[msdu.APID] = msdu;
                apid = msdu.APID;

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
                if (manager != null) {
                    manager.Packets++;
                }

                if (!msdu.Valid) {
                    CRCFails++;
                    if (manager != null) {
                        manager.CRCFails++;
                    }
                }

                if (manager != null) {
                    LengthFails++;
                    if (!msdu.Full) {
                        manager.LengthFails++;
                    }
                }

                if (!msdu.Valid || !msdu.Full) {
                    if (msdu.FrameLost) {
                        UIConsole.GlobalConsole.Error(String.Format("Lost some frames on MSDU, the file will be corrupted. CRC Match: {0} - Size Match: {1}", msdu.Valid, msdu.Full));
                    } else {
                        UIConsole.GlobalConsole.Error(String.Format("Corrupted MSDU. CRC Match: {0} - Size Match: {1}", msdu.Valid, msdu.Full));
                    }
                }

                if (msdu.Sequence == SequenceType.FIRST_SEGMENT || msdu.Sequence == SequenceType.SINGLE_DATA) {
                    if (startnum != -1) {
                        UIConsole.GlobalConsole.Error("Received First Segment but last data wasn't finished! Forcing dump.");
                        // This can only happen for multi-segment file.
                        filename = String.Format("channels/{0}/{1}_{2}.lrit", channelId, lastMSDU.APID, lastMSDU.Version);
                        FileHandler.HandleFile(filename, fileHeader);
                        startnum = -1;
                        endnum = -1;
                    }

                    fileHeader = FileParser.GetHeader(msdu.Data.Skip(10).ToArray());

                    if (msdu.Sequence == SequenceType.FIRST_SEGMENT) {
                        startnum = msdu.PacketNumber;
                    }
                } else if (msdu.Sequence == SequenceType.LAST_SEGMENT) {
                    endnum = msdu.PacketNumber;

                    if (startnum == -1) {
                        // Orphan Packet
                        endnum = -1;
                        return;
                    }
                } else if (msdu.Sequence != SequenceType.SINGLE_DATA && startnum == -1) {
                    // Orphan Packet
                    return;
                }

                string path = String.Format("channels/{0}", channelId);
                if (!Directory.Exists(path)) {
                    Directory.CreateDirectory(path);
                }

                filename = String.Format("channels/{0}/{1}_{2}.lrit", channelId, msdu.APID, msdu.Version);

                byte[] dataToSave = msdu.Data.Skip(firstOrSinglePacket ? 10 : 0).Take(firstOrSinglePacket ? msdu.PacketLength - 10 : msdu.PacketLength).ToArray(); 

                if (fileHeader.Compression == CompressionType.LRIT_RICE && !firstOrSinglePacket) {
                    int missedPackets = lastMSDU.PacketNumber - msdu.PacketNumber - 1;

                    if (lastMSDU.PacketNumber == 16383 && msdu.PacketNumber == 0) {
                        missedPackets = 0;
                    }

                    if (missedPackets > 0)  {
                        UIConsole.GlobalConsole.Warn(String.Format("Missed {0} packets on image. Filling with null bytes. Last Packet Number: {1} Current: {2}", missedPackets, lastMSDU.PacketNumber, msdu.PacketNumber));
                        byte[] fill = PacketManager.GenerateFillData(fileHeader.ImageStructureHeader.Columns);
                        using (FileStream fs = new FileStream(filename, FileMode.Append, FileAccess.Write)) {
                            using (BinaryWriter sw = new BinaryWriter(fs)) {
                                while (missedPackets > 0) {
                                    sw.Write(fill);
                                    missedPackets--;
                                }
                                sw.Flush();
                            }
                        }
                    }
                    dataToSave = PacketManager.InMemoryDecompress(dataToSave, fileHeader.ImageStructureHeader.Columns, fileHeader.RiceCompressionHeader.Pixel, fileHeader.RiceCompressionHeader.Flags);
                }

                lastMSDU = msdu;

                using (FileStream fs = new FileStream(filename, firstOrSinglePacket ? FileMode.Create : FileMode.Append, FileAccess.Write)) {
                    using (BinaryWriter sw = new BinaryWriter(fs)) {
                        sw.Write(dataToSave);
                        sw.Flush();
                    }
                }

                if (msdu.Sequence == SequenceType.LAST_SEGMENT || msdu.Sequence == SequenceType.SINGLE_DATA) {
                    FileHandler.HandleFile(filename, fileHeader);
                    startnum = -1;
                    endnum = -1;
                }
            } catch (Exception e) {
                UIConsole.GlobalConsole.Error(String.Format("Exception on FinishMSDU: {0}", e));
            }
        }

        public void ParseBytes(byte[] data) {
            uint counter;
            bool replayFlag;

            if (data.Length < FRAMESIZE) {
                throw new Exception(String.Format("Not enough data. Expected {0} and got {1}", FRAMESIZE, data.Length));
            }

            channelId = (data[1] & 0x3F);

            byte[] cb = data.Skip(2).Take(4).ToArray();

            if (BitConverter.IsLittleEndian) {
                Array.Reverse(cb);
            }

            cb[0] = 0x00;


            counter = BitConverter.ToUInt32(cb, 0);
            //counter &= 0xFFFFFF00;
            counter >>= 8;
            replayFlag = (data[5] & 0x80) > 0;

            if (replayFlag) {
                UIConsole.GlobalConsole.Log("Replay Flag set. Skipping packet.");
                return;
            }

            if (counter - lastFrame - 1 == -1) {
                UIConsole.GlobalConsole.Warn("Last packet same ID as the current one but no replay bit set! Skipping packet.");
                return;
            }

            if (lastFrame != -1 && lastFrame + 1 != counter) {
                UIConsole.GlobalConsole.Error(String.Format("Lost {0} frames. Last Frame #{1} - Current Frame #{2} on VCID {3}", counter - lastFrame - 1, lastFrame, counter, channelId));
                if (lastAPID != -1) {
                    temporaryStorage[lastAPID].FrameLost = true;
                }
            }

            if (lastFrame != -1) {
                FrameLoss += counter - lastFrame - 1;
                if (manager != null) {
                    manager.FrameLoss += counter - lastFrame - 1;
                }
            }

            lastFrame = (int)counter;

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
                        data = data.Skip(fhp).ToArray();
                        fhp = 0;
                    }           

                    p = CreatePacket(buffer);
                    lastAPID = p.Item1;
                    buffer = p.Item2;
                } 

                if (lastAPID != -1) {
                    if (fhp > 0) {
                        temporaryStorage[lastAPID].addDataBytes(buffer.Concat(data.Take(fhp)).ToArray());
                        data = data.Skip(fhp).ToArray();
                        fhp = 0;
                    }

                    if (!temporaryStorage[lastAPID].Full && !temporaryStorage[lastAPID].FrameLost && lastAPID != 2047) {
                        Bugs++;
                        if (manager != null) {
                            manager.Bugs++;
                        }
                        StackFrame callStack = new StackFrame(0, true);
                        UIConsole.GlobalConsole.Debug(String.Format("Problem at line {0} in file {1}! Not full! Check code for bugs!", callStack.GetFileLineNumber(), callStack.GetFileName()));
                    }
                    FinishMSDU(temporaryStorage[lastAPID]);
                    temporaryStorage.Remove(lastAPID);
                    lastAPID = -1;
                }

                buffer = buffer.Concat(data.Skip(fhp)).ToArray();
                p = CreatePacket(buffer);
                lastAPID = p.Item1;
                buffer = p.Item2;
            } else {
                if (buffer.Length > 0 && lastAPID != -1) {
                    buffer = buffer.Concat(data).ToArray();
                    p = CreatePacket(buffer);
                    lastAPID = p.Item1;
                    buffer = p.Item2;
                } else if (lastAPID == -1) {
                    buffer = buffer.Concat(data).ToArray();
                    p = CreatePacket(buffer);
                    lastAPID = p.Item1;
                    buffer = p.Item2;
                } else if (buffer.Length > 0) {
                    Console.WriteLine("EDGE CASE!");
                } else {
                    temporaryStorage[lastAPID].addDataBytes(data);
                }
            }
        }
    }
}


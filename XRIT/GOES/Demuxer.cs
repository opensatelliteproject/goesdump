using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Diagnostics;
using OpenSatelliteProject.PacketData;
using OpenSatelliteProject.Tools;


namespace OpenSatelliteProject {
    public class Demuxer {
        readonly int FRAMESIZE = 892;
        /// <summary>
        /// More than that, we will not count as loss, but as a corrupted frame.
        /// </summary>
        readonly int MAX_ACCOUTABLE_LOSSES = 1000;
        readonly Dictionary<int, MSDU> temporaryStorage;

        int lastAPID;
        int lastFrame;
        int channelId;
        byte[] buffer;
        readonly DemuxManager manager;
        readonly Dictionary<int, MSDUInfo> msduCache = new Dictionary<int, MSDUInfo>();


        public int CRCFails { get; set; }
        public int Bugs { get; set; }
        public int Packets { get; set; }
        public int LengthFails { get; set; }
        public long FrameLoss { get; set; }
        public int FrameJumps { get; set; }

        /// <summary>
        /// Ignores the overflow-like jumps on Frame Loss counter
        /// </summary>
        /// <value><c>true</c> if ignore counter jump; otherwise, <c>false</c>.</value>
        public static bool IgnoreCounterJump { get; set; }

        static Demuxer() {
            IgnoreCounterJump = true;
        }

        public Demuxer() {
            temporaryStorage = new Dictionary<int, MSDU>();
            buffer = new byte[0];
            lastAPID = -1;
            lastFrame = -1;
            FrameLoss = 0;
            LengthFails = 0;
            CRCFails = 0;
            FrameJumps = 0;
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
                        UIConsole.Error($"Lost some frames on MSDU, the file will be corrupted. CRC Match: {msdu.Valid} - Size Match: {msdu.Full}");
                    } else {
                        UIConsole.Error($"Corrupted MSDU. CRC Match: {msdu.Valid} - Size Match: {msdu.Full}");
                    }
                }

                if (msdu.Sequence == SequenceType.FIRST_SEGMENT || msdu.Sequence == SequenceType.SINGLE_DATA) {
                    if (msduCache.ContainsKey(msdu.APID)) {
                        var minfo = msduCache[msdu.APID];
                        UIConsole.Warn($"Received First Segment for {msdu.APID:X3} but last data wasn't saved to disk yet! Forcing dump.");
                        string ofilename = Path.Combine(FileHandler.TemporaryFileFolder, channelId.ToString());
                        ofilename = Path.Combine(ofilename, minfo.FileName);
                        FileHandler.HandleFile(ofilename, minfo.Header, manager);
                        msduCache.Remove(msdu.APID);
                    }

                    var msInfo = new MSDUInfo() {
                        APID = msdu.APID,
                        FileName = msdu.TemporaryFilename,
                        Header = FileParser.GetHeader(msdu.Data.Skip(10).ToArray()),
                        LastPacketNumber = msdu.PacketNumber,
                    };

                    msduCache.Add(msdu.APID, msInfo);
                } else if (msdu.Sequence == SequenceType.LAST_SEGMENT || msdu.Sequence == SequenceType.CONTINUED_SEGMENT) {
                    if (!msduCache.ContainsKey(msdu.APID)) {
                        UIConsole.Debug($"Orphan Packet for APID {msdu.APID}!");
                        return;
                    }
                }

                var msduInfo = msduCache[msdu.APID];
                msduInfo.Refresh();

                // LRIT EMWIN
                /* Uncomment to enable EMWIN Ingestor 
                 * Its broken right now
                if (fileHeader.PrimaryHeader.FileType == FileTypeCode.EMWIN) {
                    //Ingestor
                    int offset = 10 + (int)fileHeader.PrimaryHeader.HeaderLength;
                    EMWIN.Ingestor.Process(msdu.Data.Skip(offset).ToArray());
                    return;
                }
                */

                string path = Path.Combine(FileHandler.TemporaryFileFolder, channelId.ToString());
                if (!Directory.Exists(path)) {
                    Directory.CreateDirectory(path);
                }

                string filename = Path.Combine(path, msduInfo.FileName);

                byte[] dataToSave = msdu.Data.Skip(firstOrSinglePacket ? 10 : 0).Take(firstOrSinglePacket ? msdu.PacketLength - 10 : msdu.PacketLength).ToArray(); 

                if (msduInfo.Header.Compression == CompressionType.LRIT_RICE && !firstOrSinglePacket) {
                    int missedPackets = msduInfo.LastPacketNumber - msdu.PacketNumber - 1;

                    if (msduInfo.LastPacketNumber == 16383 && msdu.PacketNumber == 0) {
                        missedPackets = 0;
                    }

                    if (missedPackets > 0)  {
                        UIConsole.Warn(String.Format("Missed {0} packets on image. Filling with null bytes. Last Packet Number: {1} Current: {2}", missedPackets, msduInfo.LastPacketNumber, msdu.PacketNumber));
                        byte[] fill = Decompress.GenerateFillData(msduInfo.Header.ImageStructureHeader.Columns);
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
                    if (msduInfo.Header.RiceCompressionHeader == null) { // Fix bug for GOES-15 TX after GOES-16 switch. Weird, but let's try defaults
                        dataToSave = Decompress.InMemoryDecompress(dataToSave, msduInfo.Header.ImageStructureHeader.Columns, 16, AEC.ALLOW_K13_OPTION_MASK | AEC.MSB_OPTION_MASK | AEC.NN_OPTION_MASK); // 49
                    } else {
                        dataToSave = Decompress.InMemoryDecompress(dataToSave, msduInfo.Header.ImageStructureHeader.Columns, msduInfo.Header.RiceCompressionHeader.Pixel, msduInfo.Header.RiceCompressionHeader.Flags);
                    }
                }
                msduInfo.LastPacketNumber = msdu.PacketNumber;

                using (FileStream fs = new FileStream(filename, firstOrSinglePacket ? FileMode.Create : FileMode.Append, FileAccess.Write)) {
                    using (BinaryWriter sw = new BinaryWriter(fs)) {
                        sw.Write(dataToSave);
                        sw.Flush();
                    }
                }

                if (msdu.Sequence == SequenceType.LAST_SEGMENT || msdu.Sequence == SequenceType.SINGLE_DATA) {
                    FileHandler.HandleFile(filename, msduInfo.Header, manager);
                    msduCache.Remove(msdu.APID);
                }
            } catch (Exception e) {
                UIConsole.Error(String.Format("Exception on FinishMSDU: {0}", e));
            }
        }

        public void ParseBytes(byte[] data) {
            uint counter;
            bool replayFlag;
            bool ovfVcnt;
            bool ovfVcntProblem;
            bool frameJump;

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
                UIConsole.Debug("Replay Flag set. Skipping packet.");
                return;
            }

            if (counter - lastFrame - 1 == -1) {
                UIConsole.Warn("Last packet same ID as the current one but no replay bit set! Skipping packet.");
                return;
            }

            frameJump = lastFrame > counter;
            ovfVcnt = frameJump && counter == 0;
            ovfVcntProblem = ovfVcnt && (0xFFFFFF - lastFrame) + counter - 1 > 0;

            if (frameJump && !ovfVcnt) {
                UIConsole.Warn($"Frame Jump occured. Current Frame: {counter} Last Frame: {lastFrame}");
                if (lastAPID != -1) {
                    temporaryStorage[lastAPID].FrameLost = true;
                }
            } else if (lastFrame != -1 && lastFrame + 1 != counter && !ovfVcnt) {
                UIConsole.Error(String.Format("Lost {0} frames. Last Frame #{1} - Current Frame #{2} on VCID {3}", counter - lastFrame - 1, lastFrame, counter, channelId));
                if (lastAPID != -1) {
                    temporaryStorage[lastAPID].FrameLost = true;
                }
            } else if (!IgnoreCounterJump && lastFrame != -1 && ovfVcntProblem) {
                UIConsole.Error(String.Format("Lost {0} frames. Last Frame #{1} - Current Frame #{2} on VCID {3}", (0xFFFFFF - lastFrame) + counter  - 1, lastFrame, counter, channelId));
                if (lastAPID != -1) {
                    temporaryStorage[lastAPID].FrameLost = true;
                }
            }

            if (ovfVcntProblem && IgnoreCounterJump || frameJump && IgnoreCounterJump) {
                UIConsole.Warn($"Frame Jump detected from {lastFrame} to {counter} on VCID {channelId} but IgnoreCounterJump is set to true. Ignoring...");
            }

            if (lastFrame != -1) {
                if (frameJump && !ovfVcnt) {
                    manager.FrameLoss++;
                } else if (!IgnoreCounterJump && ovfVcnt) {
                    int losses = (int) Math.Abs((0xFFFFFF - lastFrame) + counter - 1);
                    if (losses < MAX_ACCOUTABLE_LOSSES) {
                        FrameLoss += losses;
                        if (manager != null) {
                            manager.FrameLoss += losses;
                        }
                    } else {
                        UIConsole.Warn($"Frame Lost ({losses}) in this section is higher than max accountable losses. Not accounting for it (probably corrupt frame).");
                    }
                } else if (!ovfVcnt) {
                    int losses = (int) Math.Abs(counter - lastFrame - 1);
                    if (losses < MAX_ACCOUTABLE_LOSSES) {
                        FrameLoss += losses;
                        if (manager != null) {
                            manager.FrameLoss += losses;
                        }
                    } else {
                        UIConsole.Warn($"Frame Lost ({losses}) in this section is higher than max accountable losses. Not accounting for it (probably corrupt frame).");
                    }
                }
            }

            if (frameJump && !ovfVcnt) {
                FrameJumps++;
                if (manager != null) {
                    manager.FrameJumps++;
                }
            }

            if (lastFrame < counter || ovfVcnt || frameJump) {
                lastFrame = (int)counter;
            } else {
                UIConsole.Warn($"LastFrame is bigger than currentFrame ({lastFrame} > {counter}). Not changing current number...");
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
                        UIConsole.Debug(String.Format("Problem at line {0} in file {1}! Not full! Check code for bugs!", callStack.GetFileLineNumber(), callStack.GetFileName()));
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


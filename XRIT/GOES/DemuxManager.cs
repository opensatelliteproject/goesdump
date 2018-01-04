using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using OpenSatelliteProject.Tools;
using System.Threading;

namespace OpenSatelliteProject {
    public class DemuxManager {
        readonly static int FILL_VCID = 63;
        readonly Dictionary<int, Demuxer> demuxers;
        bool recordFile = false;
        string fileName;
        FileStream fStream;
        readonly Mutex recordMutex;
        readonly Mutex resetMutex;
        Dictionary<int, long> productsReceived;

        public int CRCFails { get; set; }
        public int Bugs { get; set; }
        public int Packets { get; set; }
        public int LengthFails { get; set; }
        public long FrameLoss { get; set; }
        public uint FrameJumps { get; set; }

        public Dictionary<int, long> ProductsReceived {
            get {
                Dictionary<int, long> o = new Dictionary<int, long>();
                lock (productsReceived) {
                    foreach (var k in productsReceived) {
                        o[k.Key] = k.Value;
                    }
                }
                return o;
            }
        }

        public bool RecordToFile { 
            get { return recordFile; } 
            set {
                try {
                    recordMutex.WaitOne();
                    recordFile = value;
                    if (value && fStream == null) {
                        fileName = string.Format("demuxdump-{0}.bin", LLTools.Timestamp());
                        UIConsole.Log($"Starting dump on file {fileName}");
                        fStream = File.OpenWrite(fileName);
                    } else if (!value && fStream != null) {
                        UIConsole.Log($"Closing dump on file {fileName}");
                        try {
                            fStream.Close();
                            fStream = null;
                        } catch (Exception) {
                            // Ignore
                        }
                    }
                    recordMutex.ReleaseMutex();
                } catch (Exception e) {
                    CrashReport.Report (e);
                }
            }
        }

        public DemuxManager() {
            try {
                demuxers = new Dictionary<int, Demuxer>();
                productsReceived = new Dictionary<int, long>();
                CRCFails = 0;
                Bugs = 0;
                Packets = 0;
                LengthFails = 0;
                FrameLoss = 0;
                FrameJumps = 0;
                recordMutex = new Mutex();
                resetMutex = new Mutex();
                if (RecordToFile) {
                    fileName = string.Format("demuxdump-{0}.bin", LLTools.Timestamp());
                    UIConsole.Log(string.Format("Demux Dump filename: {0}", Path.GetFileName(fileName)));
                    fStream = File.OpenWrite(fileName);
                }
            } catch(Exception e) {
                CrashReport.Report (e);
                throw e;
            }
        }

        ~DemuxManager() {
            RecordToFile = false;
        }

        public void incProductCount(int productId) {
            lock (productsReceived) {
                if (!productsReceived.ContainsKey(productId)) {
                    productsReceived.Add(productId, 1);
                } else {
                    productsReceived[productId]++;
                }
            }
        }

        /// <summary>
        /// Reset this instance.
        /// </summary>
        public void reset() {
            try {
                resetMutex.WaitOne();
                CRCFails = 0;
                Bugs = 0;
                Packets = 0;
                LengthFails = 0;
                FrameLoss = 0;
                FrameJumps = 0;
                productsReceived = new Dictionary<int, long>();
                lock (demuxers) {
                    var ks = demuxers.Keys.ToList();
                    foreach (var k in ks) {
                        demuxers[k] = new Demuxer(this);
                    }
                }
                bool lastState = RecordToFile;
                RecordToFile = false;
                RecordToFile = lastState;
                resetMutex.ReleaseMutex();
            } catch (Exception e) {
                CrashReport.Report (e);
                throw e;
            }
        }

        public void parseBytes(byte[] data) {
            try {
                int scid = ((data[0] & 0x3F) << 2) | ((data[1] & 0xC0) >> 6);
                int vcid = (data[1] & 0x3F);
                int vcnt = (data[2] << 16 | data[3] << 8 | data[4]);

                // UIConsole.Log($"Satellite ID: {scid}");

                EventMaster.Post(EventTypes.FrameEvent, new FrameEventData { ChannelID = vcid, PacketNumber = vcnt });

                if (vcid != FILL_VCID) {
                    resetMutex.WaitOne();
                    lock (demuxers) {
                        if (!demuxers.ContainsKey(vcid)) {
                            UIConsole.Log($"I don't have a demuxer for VCID {vcid}. Creating...");
                            demuxers.Add(vcid, new Demuxer(this));
                        }
                    }
                    recordMutex.WaitOne();
                    if (RecordToFile) {
                        try {
                            fStream.Write(data, 0, data.Length);
                        } catch (Exception e) {
                            UIConsole.Error($"Error writting demuxdump file: {e}");
                        }
                    }
                    recordMutex.ReleaseMutex();
                    demuxers[vcid].ParseBytes(data);
                    resetMutex.ReleaseMutex();
                }
            } catch (Exception e) {
                CrashReport.Report (e);
                throw e;
            }
        }
    }
}


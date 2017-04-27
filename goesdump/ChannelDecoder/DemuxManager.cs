using System;
using System.Collections.Generic;
using System.IO;
using OpenSatelliteProject.Tools;
using OpenSatelliteProject.PacketData.Enums;
using System.Threading;

namespace OpenSatelliteProject {
    public class DemuxManager {
        private readonly static int FILL_VCID = 63;
        private Dictionary<int, Demuxer> demuxers;
        private bool recordFile = false;
        private string fileName;
        private FileStream fStream;
        private Mutex recordMutex;
        private Mutex resetMutex;

        public int CRCFails { get; set; }
        public int Bugs { get; set; }
        public int Packets { get; set; }
        public int LengthFails { get; set; }
        public long FrameLoss { get; set; }
        public uint FrameJumps { get; set; }

        public Dictionary<int, long> productsReceived;
        public delegate void FrameEventData(int vcid, int vccnt);

        public event FrameEventData FrameEvent;

        public bool RecordToFile { 
            get { return recordFile; } 
            set {
                recordFile = value;
                recordMutex.WaitOne();
                if (value && fStream == null) {
                    fileName = string.Format("demuxdump-{0}.bin", LLTools.Timestamp());
                    UIConsole.GlobalConsole.Log($"Starting dump on file {fileName}");
                    fStream = File.OpenWrite(fileName);
                } else if (!value && fStream != null) {
                    UIConsole.GlobalConsole.Log($"Closing dump on file {fileName}");
                    try {
                        fStream.Close();
                    } catch(Exception) {
                        // Ignore
                    }
                }
                recordMutex.ReleaseMutex();
            }
        }

        public DemuxManager() {
            demuxers = new Dictionary<int, Demuxer>();
            productsReceived = new Dictionary<int, long>();
            CRCFails = 0;
            Bugs = 0;
            Packets = 0;
            LengthFails = 0;
            FrameLoss = 0;
            FrameJumps = 0;
            if (RecordToFile) {
                fileName = string.Format("demuxdump-{0}.bin", LLTools.Timestamp());
                UIConsole.GlobalConsole.Log(string.Format("Demux Dump filename: {0}", fileName));
                fStream = File.OpenWrite(fileName);
            }
        }

        ~DemuxManager() {
            RecordToFile = false;
        }

        public void incProductCount(int productId) {
            if (!productsReceived.ContainsKey(productId)) {
                productsReceived.Add(productId, 1);
            } else {
                productsReceived[productId]++;
            }
        }

        /// <summary>
        /// Reset this instance.
        /// </summary>
        public void reset() {
            resetMutex.WaitOne();
            CRCFails = 0;
            Bugs = 0;
            Packets = 0;
            LengthFails = 0;
            FrameLoss = 0;
            FrameJumps = 0;
            productsReceived = new Dictionary<int, long>();
            foreach (var k in demuxers.Keys) {
                demuxers[k] = new Demuxer(this);
            }
            bool lastState = RecordToFile;
            RecordToFile = false;
            RecordToFile = lastState;
            resetMutex.ReleaseMutex();
        }

        public void parseBytes(byte[] data) {
            int vcid = (data[1] & 0x3F);
            int vcnt = (data[2] << 16 | data[3] << 8 | data[4]);

            FrameEvent?.Invoke(vcid, vcnt);

            if (vcid != FILL_VCID) {
                resetMutex.WaitOne();
                if (!demuxers.ContainsKey(vcid)) {
                    UIConsole.GlobalConsole.Log(String.Format("I don't have a demuxer for VCID {0}. Creating...", vcid));
                    demuxers.Add(vcid, new Demuxer(this));
                }
                if (RecordToFile) {
                    recordMutex.WaitOne();
                    try {
                        fStream.Write(data, 0, data.Length);
                    } catch (Exception e) {
                        UIConsole.GlobalConsole.Error(String.Format("Error writting demuxdump file: {0}", e));
                    }
                    recordMutex.ReleaseMutex();
                }
                demuxers[vcid].ParseBytes(data);
                resetMutex.ReleaseMutex();
            }
        }
    }
}


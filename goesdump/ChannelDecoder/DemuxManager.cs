using System;
using System.Collections.Generic;
using System.IO;
using OpenSatelliteProject.Tools;

namespace OpenSatelliteProject {
    public class DemuxManager {
        private readonly static int FILL_VCID = 63;
        private Dictionary<int, Demuxer> demuxers;

        public int CRCFails { get; set; }
        public int Bugs { get; set; }
        public int Packets { get; set; }
        public int LengthFails { get; set; }
        public long FrameLoss { get; set; }
        public static bool RecordToFile { get; set; }

        private string fileName;
        private FileStream fStream;

        static DemuxManager() {
            RecordToFile = false;
        }

        public DemuxManager() {
            demuxers = new Dictionary<int, Demuxer>();
            CRCFails = 0;
            Bugs = 0;
            Packets = 0;
            LengthFails = 0;
            FrameLoss = 0;
            if (RecordToFile) {
                fileName = string.Format("demuxdump-{0}.bin", LLTools.Timestamp());
                UIConsole.GlobalConsole.Log(string.Format("Demux Dump filename: {0}", fileName));
                fStream = File.OpenWrite(fileName);
            }
        }

        ~DemuxManager() {
            if (RecordToFile) {
                UIConsole.GlobalConsole.Log(string.Format("Closing Demux Dump filename: {0}", fileName));
                fStream.Close();
            }
        }

        public void parseBytes(byte[] data) {
            int vcid = (data[1] & 0x3F);

            if (vcid != FILL_VCID) {
                if (!demuxers.ContainsKey(vcid)) {
                    UIConsole.GlobalConsole.Log(String.Format("I don't have a demuxer for VCID {0}. Creating...", vcid));
                    demuxers.Add(vcid, new Demuxer(this));
                }
                if (RecordToFile) {
                    try {
                        fStream.Write(data, 0, data.Length);
                    } catch (Exception e) {
                        UIConsole.GlobalConsole.Error(String.Format("Error writting demuxdump file: {0}", e));
                    }
                }
                demuxers[vcid].ParseBytes(data);
            }
        }
    }
}


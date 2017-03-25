using System;
using System.Collections.Generic;

namespace OpenSatelliteProject {
    public class DemuxManager {
        private readonly static int FILL_VCID = 63;
        private Dictionary<int, Demuxer> demuxers;

        public int CRCFails { get; set; }
        public int Bugs { get; set; }
        public int Packets { get; set; }
        public int LengthFails { get; set; }
        public long FrameLoss { get; set; }

        public DemuxManager() {
            demuxers = new Dictionary<int, Demuxer>();
            CRCFails = 0;
            Bugs = 0;
            Packets = 0;
            LengthFails = 0;
            FrameLoss = 0;
        }

        public void parseBytes(byte[] data) {
            int vcid = (data[1] & 0x3F);

            if (vcid != FILL_VCID) {
                if (!demuxers.ContainsKey(vcid)) {
                    UIConsole.GlobalConsole.Log(String.Format("I don't have a demuxer for VCID {0}. Creating...", vcid));
                    demuxers.Add(vcid, new Demuxer(this));
                }
                demuxers[vcid].ParseBytes(data);
            }
        }
    }
}


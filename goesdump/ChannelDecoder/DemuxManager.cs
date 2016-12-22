using System;
using System.Collections.Generic;

namespace OpenSatelliteProject {
    public class DemuxManager {
        private readonly static int FILL_VCID = 63;

        private Dictionary<int, Demuxer> demuxers;

        public DemuxManager() {
            demuxers = new Dictionary<int, Demuxer>();
        }

        public void parseBytes(byte[] data) {
            int vcid = (data[1] & 0x3F);
            if (vcid != FILL_VCID) {
                if (vcid == 0 || vcid == 1) {
                    // Skip DCS and EMWIN
                    return;
                }
                if (!demuxers.ContainsKey(vcid)) {
                    UIConsole.GlobalConsole.Log(String.Format("I don't have a demuxer for VCID {0}. Creating...", vcid));
                    demuxers.Add(vcid, new Demuxer());
                }

                demuxers[vcid].ParseBytes(data);
            }
        }
    }
}


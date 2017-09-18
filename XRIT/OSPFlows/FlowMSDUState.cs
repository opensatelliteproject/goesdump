using System;
using System.Collections.Generic;
using OpenSatelliteProject.FlowData;

namespace OpenSatelliteProject.OSPFlows {
    public class FlowMSDUState {
        public Dictionary<int, TransportFileData> apidFiles; // APID, TransportFileData
        public Func<TransportFileData, bool> transportFileFunc;

        public FlowMSDUState (Func<TransportFileData, bool> tff) {
            apidFiles = new Dictionary<int, TransportFileData> ();
            transportFileFunc = tff;
        }
    }
}


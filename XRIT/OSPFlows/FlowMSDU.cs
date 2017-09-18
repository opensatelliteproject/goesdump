using System;
using System.IO;
using System.Linq;
using OpenSatelliteProject.Flow;
using OpenSatelliteProject.PacketData;
using OpenSatelliteProject.Tools;
using OpenSatelliteProject.FlowData;

namespace OpenSatelliteProject.OSPFlows {
    /// <summary>
    /// OpenSatelliteProject MSDU Flow Implementation for LRIT / HRIT
    /// </summary>
    public class FlowMSDU : MSDUFlow {
        #region MSDUFlow implementation
        public object CreateContext (Func<TransportFileData, bool> tff) {
            return new FlowMSDUState (tff);
        }

        public void FinishMSDU (MSDU msdu, object context) {
            FlowMSDUState msduState = (FlowMSDUState)context;
            try {
                if (msdu.APID == 2047) {
                    // Skip fill packet
                    return;
                }

                if (!msduState.apidFiles.ContainsKey(msdu.APID)) {
                    msduState.apidFiles.Add(msdu.APID, new TransportFileData());
                }

                var tfd = msduState.apidFiles[msdu.APID];
                tfd.PutMSDU(msdu);

                GlobalStatManager.IncPacketsReceived();
                if (!msdu.Valid) {
                    GlobalStatManager.IncCRCErrors();
                }

                if (!msdu.Full) {
                    GlobalStatManager.IncLengthFails();
                }

                if (!msdu.Valid || !msdu.Full) {
                    if (msdu.FrameLost) {
                        UIConsole.Error($"Lost some frames on MSDU, the file will be corrupted. CRC Match: {msdu.Valid} - Size Match: {msdu.Full}");
                    } else {
                        UIConsole.Error($"Corrupted MSDU. CRC Match: {msdu.Valid} - Size Match: {msdu.Full}");
                    }
                }

                if (tfd.ReadyToSave) {
                    msduState.transportFileFunc(tfd);
                    msduState.apidFiles.Remove(msdu.APID);
                }
            } catch (Exception e) {
                UIConsole.Error($"There was an error processing MSDU: {e}");
            }
        }
        #endregion  
    }
}


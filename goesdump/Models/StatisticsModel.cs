using System;
using Newtonsoft.Json;
using OpenSatelliteProject.Tools;

namespace OpenSatelliteProject {
    public class StatisticsModel: BaseModel {
        
        public int satelliteID { get; set; }
        public int virtualChannelID { get; set; }
        public UInt64 packetNumber { get; set; }
        public int totalBits { get; set; }
        public int viterbiErrors { get; set; }
        public int signalQuality { get; set; }
        public int syncCorrelation { get; set; }
        public int phaseCorrection { get; set; }
        public int[] reedSolomon { get; set; }
        public string syncWord { get; set; }
        public bool frameLock { get; set; }
        public DateTime startTime { get; set; }
        public TimeSpan runningTime { get; set; }

        public StatisticsModel(Statistics_st data) : base("statisticsData") {
            Refresh(data);
        }

        public void Refresh(Statistics_st data) {
            this.satelliteID = data.scid;
            this.virtualChannelID = data.vcid;
            this.packetNumber = data.packetNumber;
            this.totalBits = data.frameBits;
            this.viterbiErrors = data.vitErrors;
            this.signalQuality = data.signalQuality;
            this.syncCorrelation = data.syncCorrelation;
            if (data.syncWord != null) {
                this.syncWord = string.Format("{0:X02}{1:X02}{2:X02}{3:X02}", data.syncWord[0], data.syncWord[1], data.syncWord[2], data.syncWord[3]);
            } else {
                this.syncWord = "00000000";
            }
            this.reedSolomon = data.rsErrors;
            this.frameLock = data.frameLock > 0;
            this.startTime = LLTools.UnixTimeStampToDateTime(data.startTime);
            this.runningTime = DateTime.Now.Subtract(startTime);
        }
    }
}


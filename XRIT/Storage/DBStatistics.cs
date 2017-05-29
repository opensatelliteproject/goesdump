using System;
using SQLite;

namespace OpenSatelliteProject {
    public class DBStatistics {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public long Timestamp { get; set; }
        public int SCID { get; set; }
        public int VCID { get; set; }
        public long PacketNumber { get; set; }
        public int VitErrors { get; set; }
        public int FrameBits { get; set; }
        public int RSErrors0 { get; set; }
        public int RSErrors1 { get; set; }
        public int RSErrors2 { get; set; }
        public int RSErrors3 { get; set; }
        public byte SignalQuality { get; set; }
        public byte SyncCorrelation { get; set; }
        public byte PhaseCorrection { get; set; }
        public long LostPackets { get; set; }
        public int AverageVitCorrections { get; set; }
        public byte AverageRSCorrections { get; set; }
        public long DroppedPackets { get; set; }
        [MaxLength(16)]
        public string SyncWord { get; set; }
        public bool FrameLock { get; set; }
    }
}


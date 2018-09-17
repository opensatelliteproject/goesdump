using System;
using System.Collections.Generic;
using OpenSatelliteProject.PacketData;

namespace OpenSatelliteProject {
    public class OrganizerData {
        public Dictionary<int, string> Segments { get; set; }
        public int Lines { get; set; }
        public int Columns { get; set; }
        public float PixelAspect { get; set; }
        public int ColumnOffset { get; set; }
        public int MaxSegments;
        public bool OK { get; set; }
        public int Timestamp { get; set; }
        public int FirstSegment { get; set; }
        public int LineOffset { get; set; }
        public float ColumnScalingFactor { get; set; }
        public float LineScalingFactor { get; set; }

        /// <summary>
        /// Code used for desambiguation
        /// </summary>
        /// <value>The code.</value>
        public string Code { get; set; }

        public XRITHeader FileHeader { get; set; }

        public OrganizerData() {
            Segments = new Dictionary<int, string>();
            Lines = -1;
            Columns = -1;
            PixelAspect = -1;
            ColumnOffset = 0;
            MaxSegments = 1;
            OK = false;
            Timestamp = 0;
            FirstSegment = 999999;
            Code = DateTime.UtcNow.ToShortTimeString ();
            FileHeader = null;
            ColumnOffset = -1;
            LineOffset = -1;
            ColumnScalingFactor = 0f;
            LineScalingFactor = 0f;
        }

        public bool IsComplete { get { return Segments.Count == MaxSegments; }}
    }
}


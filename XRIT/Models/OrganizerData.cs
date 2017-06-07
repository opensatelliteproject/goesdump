using System;
using System.Collections.Generic;

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

        /// <summary>
        /// Code used for desambiguation
        /// </summary>
        /// <value>The code.</value>
        public string Code { get; set; }

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
        }

        public bool IsComplete { get { return Segments.Count == MaxSegments; }}
    }
}


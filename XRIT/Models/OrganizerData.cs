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

        public OrganizerData() {
            Segments = new Dictionary<int, string>();
            Lines = -1;
            Columns = -1;
            PixelAspect = -1;
            ColumnOffset = 0;
            MaxSegments = 1;
        }

        public bool IsComplete { get { return Segments.Count == MaxSegments; }}
    }
}


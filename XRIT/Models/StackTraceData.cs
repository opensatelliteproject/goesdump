using System;

namespace OpenSatelliteProject {
    public class StackTraceData {
        public int Line { get; set; }
        public int Column { get; set; }
        public string Filename { get; set; }
        public string ClassName { get; set; }
        public string Method { get; set; }
    }
}


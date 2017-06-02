using System;
using System.Linq;
using System.Collections.Generic;

namespace OpenSatelliteProject {
    public class NewFileReceivedEventData {
        public string Name { get; set; }
        public string Path { get; set; }
        public Dictionary<string, string> Metadata { get; set; }

        public NewFileReceivedEventData() {
            Name = "";
            Path = "";
            Metadata = new Dictionary<string, string> ();
        }

        public override string ToString() {
            string ret = $"Name: {Name} | Path: {Path}: \nMetadata:\n";
            Metadata.Keys.ToList().ForEach (k => ret += $"\t{k}: {Metadata[k]}\n");
            return ret;
        }
    }
}


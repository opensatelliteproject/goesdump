using System;
using System.Linq;
using System.Text;

namespace OpenSatelliteProject.DCS {
    public class DCSHeader {

        public string Address { get; set; }
        public DateTime DateTime { get; set; }
        public string Status { get; set; }
        public int Signal { get; set; }
        public int FrequencyOffset { get; set; }
        public string ModIndexNormal { get; set; }
        public string DataQualNominal { get; set; }
        public string Channel { get; set; }
        public string SourceCode { get; set; }

        public DCSHeader(string header) {
            try { 
                Address = header.Substring(0, 8);
                ModIndexNormal =  "" + header[25];
                DataQualNominal =  "" + header[26];
                Channel = header.Substring(27, 4);
                SourceCode = header.Substring(31, 2);
                Status = "" + header[20];

                int year = int.Parse(header.Substring(9, 2));
                int dayOfYear = int.Parse(header.Substring(11, 3));
                int hour = int.Parse(header.Substring(14, 2));
                int min = int.Parse(header.Substring(16, 2));
                int second = int.Parse(header.Substring(18, 2));
                DateTime = new DateTime(2000 + year, 1, 1, hour, min, second);
                DateTime = DateTime.AddDays(dayOfYear);

                Signal = int.Parse(header.Substring(21, 2));
                FrequencyOffset = int.Parse(header.Substring(23, 2));
            } catch(Exception e) {
                Console.WriteLine("Invalid header: {0}", e);
            }
        }

        public DCSHeader(byte[] header) : this(Encoding.ASCII.GetString(header.Take(33).ToArray())) {
            
        }
    }
}


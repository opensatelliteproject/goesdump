using System;
using System.Globalization;

namespace OpenSatelliteProject.PacketData {
    public class EMWINHeader {

        public string Filename { get; }
        public int PartNumber { get; }
        public int PartTotal { get; }
        public int CS { get; }
        public string DateTime { get; }

        public EMWINHeader(string header) {
            if (header[0] != '/') {
                throw new InvalidOperationException(string.Format("Invalid header: {0}", header));
            }
            //UIConsole.GlobalConsole.Debug("EMWIN Header: " + header);
            ///PFMISDCPNI.TXT/PN1     /PT1     /CS11024  /FD02/12/2017 07:27:29 PM  
            try {
                Filename = header.Substring(1, 14);
                PartNumber = int.Parse(header.Substring(18, 6).Trim());
                PartTotal = int.Parse(header.Substring(27, 6).Trim());
                CS = int.Parse(header.Substring(36, 7).Trim());
                this.DateTime = header.Substring(46, 22).Trim();
            } catch (Exception e) {
                Console.WriteLine("ERROR Parsing header \"{0}\": {1}", header, e);
                Filename = "PFFILLFILE.TXT"; // To EMWIN Ingestor skip corrupted header
            }
        }
    }
}

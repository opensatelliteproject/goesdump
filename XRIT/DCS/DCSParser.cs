using System;
using System.Collections.Generic;
using OpenSatelliteProject.Tools;
using System.IO;
using System.Linq;

namespace OpenSatelliteProject.DCS {
    public static class DCSParser {


        public static List<DCSHeader> parseDCS(string filename) {
            List<DCSHeader> headers = new List<DCSHeader>();
            var header = FileParser.GetHeaderFromFile(filename);
            var dataOffset = header.PrimaryHeader.HeaderLength;
            int dataSize;
            byte[] fileData;

            using (var fs = File.OpenRead(filename)) {
                fs.Seek(dataOffset, SeekOrigin.Begin);
                dataSize = (int)(fs.Length - dataOffset);
                fileData = new byte[dataSize];
                fs.Read(fileData, 0, dataSize);
            }

            //byte[] baseHeader = fileData.Take(64).ToArray();
            fileData = fileData.Skip(64).ToArray();

            List<byte[]> dcs = new List<byte[]>();

            int lastPos = 0;
            int pos = 0;
            while (pos < fileData.Length - 3) {
                if (fileData[pos] == 0x02 && fileData[pos + 1] == 0x02 && fileData[pos + 2] == 0x18) {
                    Console.WriteLine("Found segment at {0}", pos);
                    byte[] segment = fileData.Skip(lastPos).Take(pos - lastPos - 3).ToArray();
                    dcs.Add(segment);
                    pos += 3;
                    lastPos = pos;
                } else {
                    pos++;
                }
            }

            dcs.ForEach(a => {
                if (a.Length > 33) {
                    DCSHeader h = new DCSHeader(a);
                    headers.Add(h);
                }
            });

            return headers;
        }
    }
}


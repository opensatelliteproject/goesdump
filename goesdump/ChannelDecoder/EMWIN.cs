using System;
using System.Linq;
using OpenSatelliteProject.PacketData;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace OpenSatelliteProject {
    public class EMWIN {

        public static EMWIN Ingestor;

        static EMWIN() {
            Ingestor = new EMWIN();
        }

        private static readonly int MAX_FRAME_SIZE = 1116;
        private static readonly string FILLFILENAME = "PFFILLFILE.TXT";
        private byte[] buffer;
        private Dictionary<string, EmwinFile> files;

        private EMWIN() {
            buffer = new byte[0];
            files = new Dictionary<string, EmwinFile>();
        }

        public void Process(byte[] inData) {
            buffer = buffer.Concat(inData).ToArray();
            var pos = FindSyncMarker(buffer);
            if (pos == -1) {
                if (buffer.Length > MAX_FRAME_SIZE * 3) {
                    UIConsole.GlobalConsole.Warn(string.Format("EMWIN Buffer grown beyond {0}! This should happen. Clearing buffer.", MAX_FRAME_SIZE * 3));
                    buffer = new byte[0];
                }
                return;
            }

            var data = buffer.Skip(pos + 12).Take(MAX_FRAME_SIZE - 12).ToArray(); // 12 for the syncMark

            if (data.Length != MAX_FRAME_SIZE - 12) {
                // Not Enough Data
                return;
            }

            buffer = buffer.Skip(pos + MAX_FRAME_SIZE).ToArray();
            if (data.Length > 0) {
                var headerData = Encoding.GetEncoding("ISO-8859-1").GetString(data.Take(80).ToArray());
                data = data.Skip(80).ToArray();

                try {
                    var header = new EMWINHeader(headerData);
                    if (header.Filename.Equals(FILLFILENAME)) {
                        return;
                    }
                    //UIConsole.GlobalConsole.Log(string.Format("Received {0}/{1} of {2}", header.PartNumber, header.PartTotal, header.Filename));
                    if (header.PartNumber == 1) {
                        if (files.ContainsKey(header.Filename)) {
                            UIConsole.GlobalConsole.Warn(string.Format("Files already has a key for file {0}", header.Filename));
                        } else {
                            string newfilename = DateTime.Now.ToString("yyyyMMddHHmmssffff") + header.Filename;
                            files.Add(header.Filename, new EmwinFile());
                            files[header.Filename].Parts = header.PartTotal;
                            files[header.Filename].Received = 0;
                            files[header.Filename].Output = Path.Combine("channels", Path.Combine("tmp", newfilename));
                        }
                    }

                    if (!files.ContainsKey(header.Filename)) {
                        //UIConsole.GlobalConsole.Warn(string.Format("(EMWIN) Received incomplete part for {0}", header.Filename));
                        return;
                    } else if (files[header.Filename].Received + 1 != header.PartNumber) {
                        //UIConsole.GlobalConsole.Error(string.Format("(EMWIN) Missed {0} frames for file {1}", header.PartNumber - files[header.Filename].Received - 1, header.Filename));
                        files.Remove(header.Filename);
                        return;
                    } else {
                        string dir = Path.GetDirectoryName(files[header.Filename].Output);
                        if (!Directory.Exists(dir)) {
                            Directory.CreateDirectory(dir);
                        }

                        var f = File.Open(files[header.Filename].Output, header.PartNumber == 1 ? FileMode.Create : FileMode.Append);
                        f.Write(data, 0, data.Length);
                        f.Close();
                        files[header.Filename].Received += 1;
                    }

                    if (header.PartNumber == header.PartTotal && files.ContainsKey(header.Filename)) {
                        string output = files[header.Filename].Output;
                        string basedir = new DirectoryInfo(Path.GetDirectoryName(output)).Parent.FullName;
                        string newdir = Path.Combine(basedir, "EMWIN");

                        if (!Directory.Exists(newdir)) {
                            Directory.CreateDirectory(newdir);
                        }

                        string fname = Path.Combine(newdir, header.Filename);
                        if (File.Exists(fname)) {
                            fname = DateTime.Now.ToString("yyyyMMddHHmmssffff") + "-" + header.Filename;
                            fname = Path.Combine(newdir, fname);
                        }
                        File.Move(files[header.Filename].Output, fname);
                        UIConsole.GlobalConsole.Log(string.Format("New EMWIN ({0})", header.Filename));
                        files.Remove(header.Filename);
                    }
                } catch (Exception e) {
                    UIConsole.GlobalConsole.Error(string.Format("(EMWIN) Error: {0}", e.Message));
                }
            }
        }

        private static int FindSyncMarker(byte[] data) {
            // The sync marker is 12 null bytes. We check for 12 null and a '/'
            // Because sometimes there is null bytes in the middle of a packet.
            int searchSize = data.Length - 13;
            int pos = -1;

            for (int i = 0; i < searchSize; i++) {
                bool t = true;
                for (int z = 0; z < 13; z++) {
                    if (z == 12) {
                        if (data[i + z] != '/') {
                            t = false;
                            break;
                        }
                    } else {
                        if (data[i + z] != '\x00') {
                            t = false;
                            break;
                        }
                    }
                }

                if (t) {
                    pos = i;
                    break;
                }
            }

            return pos;
        } 
    }
}


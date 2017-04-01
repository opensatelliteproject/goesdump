using System;
using WebSocketSharp;
using WebSocketSharp.Server;
using System.IO;
using System.Text;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using OpenSatelliteProject.Tools;
using OpenSatelliteProject.DCS;

namespace OpenSatelliteProject {
    public class DirectoryHandler {
        private string dataFolder;
        private static readonly int bufferSize = 4096;

        public string BasePath { get; set; }

        public DirectoryHandler(string dataFolder, string basePath) {
            this.dataFolder = dataFolder;
            this.BasePath = basePath;
        }

        public void HandleAccess(HttpServer server, HttpRequestEventArgs e) {
            var req = e.Request;
            var res = e.Response;

            var upperPath = "";
            var path = req.RawUrl.Split(new char[] {'?'}, 2)[0];
            var relPath = path.Replace(BasePath, "").UrlDecode();
            var download = req.QueryString["download"] != null;

            if (relPath.StartsWith("/")) {
                relPath = relPath.Substring(1);
            }

            if (relPath.Length > 0) {
                var tmp = !relPath.EndsWith("/") ? relPath + "/" : relPath;
                upperPath = Path.Combine(BasePath, Path.GetDirectoryName(Path.GetDirectoryName(tmp)));
            }

            var folder = Path.Combine(dataFolder, relPath);

            if (folder.Contains("../")) {
                folder.Replace("../", "");
            }
            if (File.Exists(folder)) {
                if (folder.Contains(".dcs") && !download) {
                    // Assemble DCS Header table
                    List<DCSHeader> headers = DCSParser.parseDCS(folder);

                    string dcsList = "";

                    headers.ForEach(a => {
                        dcsList += String.Format(
                            "\t<tr>\n" +
                            "\t\t<th>{0}</th>\n" +
                            "\t\t<th>{1}</th>\n" +
                            "\t\t<th>{2}</th>\n" +
                            "\t\t<th>{3}</th>\n" +
                            "\t\t<th>{4}</th>\n" +
                            "\t\t<th>{5}</th>\n" +
                            "\t\t<th>{6}</th>\n" +
                            "\t\t<th>{7}</th>\n" +
                            "\t\t<th>{8}</th>\n" +
                            "\t</tr>\n", 
                            a.Address, 
                            a.DateTime.ToString(), 
                            a.Status, 
                            a.Signal, 
                            a.FrequencyOffset, 
                            a.ModIndexNormal, 
                            a.DataQualNominal, 
                            a.Channel, 
                            a.SourceCode);
                    });

                    string listTable = "<table cellpadding=\"5\"cellspacing=\"0\">\n" +
                        "\t<tr>\n" +
                        "\t\t<th>Address</th>\n" +
                        "\t\t<th>Date / Time</th>\n" +
                        "\t\t<th>Status</th>\n" +
                        "\t\t<th>Signal</th>\n" +
                        "\t\t<th>Frequency Offset</th>\n" +
                        "\t\t<th>MIN</th>\n" +
                        "\t\t<th>DQN</th>\n" +
                        "\t\t<th>Channel</th>\n" +
                        "\t\t<th>Source Code</th>\n" +
                        "\t</tr>\n" +
                        "\t<tr><th colspan=\"9\"><hr></th></tr>\n" +
                        "{0}\n" +
                        "</table>\n";

                    listTable = string.Format(listTable, dcsList);

                    string output = string.Format(
                        "<html>\n" +
                        "\t<head>\n" +
                        "\t\t<title>OpenSatelliteProject - {0}</title>\n" +
                        "\t</head>\n" +
                        "\t<body>\n" +
                        "\t<h2>OpenSatelliteProject {0}</h2>\n" +
                        "\tDownload File: <a href=\"{2}\">{0}</a></BR>\n" +
                        "\t</BR>\n" +
                        "\t{1}\n" +
                        "\t</body>\n" +
                        "</html>", "/" + relPath.UrlDecode(), listTable, path + "?download=1");

                    res.StatusCode = (int)HttpStatusCode.OK;
                    res.WriteContent(Encoding.UTF8.GetBytes(output));
                } else {
                    // Handle File load
                    try {
                        res.ContentType = Presets.GetMimeType(Path.GetExtension(folder));
                        res.StatusCode = (int)HttpStatusCode.OK;
                        res.SendChunked = true;
                        using (FileStream f = File.OpenRead(folder)) {
                            Stream o = res.OutputStream;
                            byte[] buffer = new byte[bufferSize];
                            long totalBytes = f.Length;
                            long readBytes = 0;

                            while (readBytes < totalBytes) {
                                int rb = f.Read(buffer, 0, bufferSize);
                                readBytes += rb;
                                o.Write(buffer, 0, rb);
                            }

                            o.Close();
                        }
                    } catch (Exception ex) {
                        string output = string.Format("Error reading file: {0}", ex);
                        res.StatusCode = (int)HttpStatusCode.InternalServerError;
                        res.WriteContent(Encoding.UTF8.GetBytes(output));
                    }
                }
            } else if (Directory.Exists(folder)) {
                // Handle Directory Listing.
                List<string> files = Directory.GetFiles(folder).Where(x => !x.EndsWith(".lrit")).OrderBy(a => a).ToList();
                List<string> dirs = Directory.GetDirectories(folder).OrderBy(a => a).ToList();
                string dirList = "";
                string fileList = "";

                if (upperPath.Length > 0) {
                    dirList += string.Format("\t<tr>\n" +
                    "\t\t<td><img src=\"/static/folder.gif\"></td>\n" +
                    "\t\t<td><a href=\"{0}\">{1}</a></td>\n" +
                    "\t\t<td>{2}</td>\n" +
                    "\t\t<td>{3}</td>\n" +
                    "\t</tr>\n", upperPath, "..", "", "-");
                }

                foreach (var file in dirs) {
                    var name = Path.GetFileName(file);
                    var time = Directory.GetLastWriteTime(file);
                    var url = Path.Combine(req.RawUrl, name.UrlEncode());
                    dirList += string.Format("\t<tr>\n" +
                        "\t\t<td><img src=\"/static/folder.gif\"></td>\n" +
                        "\t\t<td><a href=\"{0}\">{1}</a></td>\n" +
                        "\t\t<td>{2}</td>\n" +
                        "\t\t<td>{3}</td>\n" +
                        "\t</tr>\n", url, name, time, "-");
                }

                foreach (var file in files) {
                    var name = Path.GetFileName(file);
                    var time = File.GetLastWriteTime(file);
                    var size = new System.IO.FileInfo(file).Length;
                    var url = Path.Combine(req.RawUrl, name.UrlEncode());
                    fileList += string.Format("\t<tr>\n" +
                        "\t\t<td><img src=\"/static/file.gif\"></td>\n" +
                        "\t\t<td><a href=\"{0}\">{1}</a></td>\n" +
                        "\t\t<td>{2}</td>\n" +
                        "\t\t<td>{3}</td>\n" +
                        "\t</tr>\n", url, name, time, LLTools.BytesToString(size));
                }

                string listTable = "<table cellpadding=\"5\"cellspacing=\"0\">\n" +
                                   "\t<tr><th valign=\"top\"><img src=\"/static/blank.gif\"></th><th>Name</th><th>Last modified</th><th>Size</th></tr>\n" +
                                   "\t<tr><th colspan=\"4\"><hr></th></tr>\n" +
                                   "{0}\n" +
                                    "{1}" +
                                   "</table>\n";

                listTable = string.Format(listTable, dirList, fileList);
                string output = string.Format(
                    "<html>\n" +
                    "\t<head>\n" +
                    "\t\t<title>OpenSatelliteProject - {0}</title>\n" +
                    "\t</head>\n" +
                    "\t<body>\n" +
                    "\t<h2>OpenSatelliteProject {0}</h2>\n" +
                    "\t</BR>\n" +
                    "\t{1}\n" +
                    "\t</body>\n" +
                    "</html>", "/" + relPath.UrlDecode(), listTable);

                res.StatusCode = (int)HttpStatusCode.OK;
                res.WriteContent(Encoding.UTF8.GetBytes(output));
            } else {
                res.StatusCode = (int)HttpStatusCode.NotFound;
                string res404 = "File not found";
                res.WriteContent(Encoding.UTF8.GetBytes(res404));
            }
        }
    }
}


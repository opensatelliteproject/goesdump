using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using OpenSatelliteProject.Tools;

namespace OpenSatelliteProject {
    public class Organizer {
        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        private Dictionary<int, GroupData> groupData;
        private string folder;

        public Dictionary<int, GroupData> GroupData { get { return groupData; } }

        public Organizer(string folder) {
            this.folder = folder;    
            this.groupData = new Dictionary<int, GroupData>();
        }

        public void Update() {
            List<string> files = Directory.GetFiles(folder).Where(f => f.EndsWith(".lrit")).ToList();
            foreach (string file in files) {
                try {
                    var header = FileParser.GetHeaderFromFile(file);
                    var anciliary = header.AncillaryHeader.Values;
                    var satellite = "Unknown";
                    var region = "Unknown";
                    var datetime = header.TimestampHeader.DateTime; // Defaults to capture time
                    var channel = 99;
                    var segmentId = header.SegmentIdentificationHeader.Sequence;

                    if (anciliary.ContainsKey("Satellite")) {
                        satellite = anciliary["Satellite"];
                    }

                    if (anciliary.ContainsKey("Region")) {
                        region = anciliary["Region"];
                    }

                    if (anciliary.ContainsKey("Channel")) {
                        channel = int.Parse(anciliary["Channel"]);
                    }

                    if (anciliary.ContainsKey("Time of frame start")) {
                        var dtstring = anciliary["Time of frame start"];
                        //2017/055/05:45:18
                        var year = dtstring.Substring(0, 4);
                        var dayOfYear = dtstring.Substring(5, 3);
                        var hours = dtstring.Substring(9, 2);
                        var minutes = dtstring.Substring(12, 2);
                        var seconds = dtstring.Substring(15, 2);
                        //Console.WriteLine("Year: {0}\nDay Of Year: {1}\nHours: {2}\nMinutes: {3}\nSeconds: {4}", year, dayOfYear, hours, minutes, seconds);
                        datetime = new DateTime(int.Parse(year), 1, 1, int.Parse(hours), int.Parse(minutes), int.Parse(seconds));
                        datetime = datetime.AddDays(int.Parse(dayOfYear));
                    } else {
                        Console.WriteLine("No Frame Time of Start found! Using capture time.");
                    }
                    /*
                    foreach(var x in header.AncillaryHeader.Values) {
                        Console.WriteLine("{0}: {1}", x.Key, x.Value);
                    }*/

                    var timestamp = (int)Math.Floor((datetime - UnixEpoch).TotalSeconds);

                    if (!groupData.ContainsKey(timestamp)) {
                        groupData[timestamp] = new GroupData();
                    }
                    var grp = groupData[timestamp];
                    grp.SatelliteName = satellite;
                    grp.RegionName = region;
                    grp.FrameTime = datetime;

                    var od = new OrganizerData();
                    switch (channel) {
                        case 1: // Visible
                            od = grp.Visible;
                            break;
                        case 3: // Water Vapour
                            od = grp.WaterVapour;
                            break;
                        case 4: // Infrared
                            od = grp.Infrared;
                            break;
                        default:
                            Console.WriteLine("Unknown Channel {0}", channel);
                            continue;
                    } 


                    od.Segments[segmentId] = file;
                    if (od.Columns == -1) {
                        od.Columns = header.ImageStructureHeader.Columns;
                        od.Lines = header.ImageStructureHeader.Lines;
                        od.PixelAspect = header.ImageNavigationHeader.ColumnScalingFactor / (float)header.ImageNavigationHeader.LineScalingFactor;
                        od.StartColumn = header.ImageNavigationHeader.ColumnOffset;
                        od.MaxSegments = header.SegmentIdentificationHeader.MaxSegments;
                    } else {
                        od.Lines += header.ImageStructureHeader.Lines;
                    }
                } catch (Exception e) {
                    Console.WriteLine("Error reading file {0}: {1}", file, e);
                }
            }
            /*
            foreach (var i in groupData) {
                var data = i.Value;
                Console.WriteLine("Showing group({0}): ", i.Key);
                Console.WriteLine(data.ToString());
            }
            */
        }
    }
}


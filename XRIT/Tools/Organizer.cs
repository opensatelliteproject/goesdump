using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using OpenSatelliteProject.Tools;
using System.Globalization;
using OpenSatelliteProject.PacketData.Enums;
using System.Text.RegularExpressions;

namespace OpenSatelliteProject {
    public class Organizer {
        static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        readonly Dictionary<int, GroupData> groupData;
        readonly List<string> alreadyProcessed;

        readonly string folder;

        public Dictionary<int, GroupData> GroupData { get { return groupData; } }

        public Organizer(string folder) {
            this.folder = folder;    
            this.groupData = new Dictionary<int, GroupData>();
            this.alreadyProcessed = new List<string>();
        }

        public void RemoveGroupData(int idx) {
            groupData.Remove(idx);
        }

        public void Update() {
            try {
                List<string> files = Directory.GetFiles(folder).Where(f => f.EndsWith(".lrit")).ToList();
                foreach (string file in files) {
                    if (alreadyProcessed.Contains(file)) {
                        continue;
                    }
                    try {
                        var header = FileParser.GetHeaderFromFile(file);
                        var anciliary = header.AncillaryHeader != null ? header.AncillaryHeader.Values : null;
                        var satellite = "Unknown";
                        var region = "Unknown";
                        var satLon = 0f;
                        var datetime = header.TimestampHeader.DateTime; // Defaults to capture time
                        var channel = 99;
                        var segmentId = header.SegmentIdentificationHeader != null ? header.SegmentIdentificationHeader.Sequence : 0;
                        var imageKey = header.SegmentIdentificationHeader != null ? header.SegmentIdentificationHeader.ImageID : -1;

                        if (header.Product.ID == (int)NOAAProductID.HIMAWARI8_ABI) {
                            channel = header.SubProduct.ID;
                            satellite = "HIMAWARI8";
                            region = "Full Disk";
                        }

                        var rgx = new Regex(@".*\((.*)\)", RegexOptions.IgnoreCase);
                        var regMatch = rgx.Match(header.ImageNavigationHeader.ProjectionName);
                        satLon = float.Parse(regMatch.Groups[1].Captures[0].Value, CultureInfo.InvariantCulture);

                        if (anciliary != null) {
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
                                // 2017/055/05:45:18
                                // or
                                // 2017-03-27T15:45:38.2Z
                                if (dtstring[4] == '/') {
                                    var year = dtstring.Substring(0, 4);
                                    var dayOfYear = dtstring.Substring(5, 3);
                                    var hours = dtstring.Substring(9, 2);
                                    var minutes = dtstring.Substring(12, 2);
                                    var seconds = dtstring.Substring(15, 2);
                                    //Console.WriteLine("Year: {0}\nDay Of Year: {1}\nHours: {2}\nMinutes: {3}\nSeconds: {4}", year, dayOfYear, hours, minutes, seconds);
                                    datetime = new DateTime(int.Parse(year), 1, 1, int.Parse(hours), int.Parse(minutes), int.Parse(seconds));
                                    datetime = datetime.AddDays(int.Parse(dayOfYear));
                                } else {
                                    datetime = DateTime.Parse(dtstring, null, DateTimeStyles.RoundtripKind);
                                }
                            } else {
                                UIConsole.Warn("No Frame Time of Start found! Using capture time.");
                            }
                        }

                        var cropSection = region.ToLower().Contains("full disk") || header.IsFullDisk;
                        int timestamp = 0;
                        if (datetime.Year < 2005 && file.Contains("OR_ABI")) {
                            // Timestamp bug on G16
                            imageKey = header.SegmentIdentificationHeader != null ? 
                                header.SegmentIdentificationHeader.ImageID : 
                                (int)Math.Floor((datetime - UnixEpoch).TotalSeconds);
                            timestamp = (int)Math.Floor((datetime - UnixEpoch).TotalSeconds);
                        } else if (datetime.Year < 2005 && file.Contains("IMG_DK")) {
                            // Himawari-8 relay BUG
                            //IMG_DK01VIS_201704161550
                            string bfile = Path.GetFileName(file);
                            string hdt = bfile.Substring(12, 12);
                            var year = hdt.Substring(0, 4);
                            var month = hdt.Substring(4, 2);
                            var day = hdt.Substring(6, 2);
                            var hour = hdt.Substring(8, 2);
                            var minute = hdt.Substring(10, 2);
                            datetime = new DateTime(int.Parse(year), int.Parse(month), int.Parse(day), int.Parse(hour), int.Parse(minute), 0);
                            imageKey = timestamp = (int)Math.Floor((datetime - UnixEpoch).TotalSeconds);
                        } else {
                            imageKey = timestamp = (int)Math.Floor((datetime - UnixEpoch).TotalSeconds);
                        }

                        if (!groupData.ContainsKey(imageKey)) {
                            groupData[imageKey] = new GroupData();
                        }

                        var grp = groupData[imageKey];
                        grp.SatelliteName = satellite;
                        grp.RegionName = region;
                        grp.FrameTime = datetime;
                        if (segmentId == 0) {
                            grp.CropImage = cropSection;
                            grp.SatelliteLongitude = satLon;
                            if (
                                header.ImageNavigationHeader != null && 
                                header.ImageNavigationHeader.ColumnScalingFactor != 0 &&
                                header.ImageNavigationHeader.LineScalingFactor != 0
                            ) {
                                grp.HasNavigationData = true;
                                grp.ColumnScalingFactor = header.ImageNavigationHeader.ColumnScalingFactor;
                                grp.LineScalingFactor = header.ImageNavigationHeader.LineScalingFactor;
                                grp.ColumnOffset = grp.ColumnOffset == -1 ? header.ImageNavigationHeader.ColumnOffset : grp.ColumnOffset;
                                grp.LineOffset = grp.LineOffset == -1 ? header.ImageNavigationHeader.LineOffset : grp.LineOffset;
                            }
                        }
                        grp.Code = header.SegmentIdentificationHeader != null ? 
                            header.SegmentIdentificationHeader.ImageID + "_" + header.SubProduct.Name :
                            header.Product.Name + "_" + header.SubProduct.Name;

                        var od = new OrganizerData();
                        string z;
                        switch (channel) {
                            case 1: // Visible
                                od = grp.Visible;
                                break;
                            case 2: // Visible for G16
                                if (satellite == "G16") {
                                    od = grp.Visible;
                                } else {
                                    string p = $"{timestamp%1000}-{((NOAAProductID)header.Product.ID).ToString()}-{header.SubProduct.Name}";
                                    if (!grp.OtherData.ContainsKey(p)) {
                                        grp.OtherData.Add(p, new OrganizerData());
                                    }
                                    od = grp.OtherData[p];
                                }
                                break;
                            case 3: // Water Vapour
                                od = satellite == "HIMAWARI8" ? grp.Infrared : grp.WaterVapour;
                                break;
                            case 4: // Infrared
                                od = grp.Infrared;
                                break;
                            case 7: 
                                if (satellite == "HIMAWARI8") {
                                    od = grp.WaterVapour;
                                    break;
                                }
                                z = $"{timestamp%1000}-{((NOAAProductID)header.Product.ID).ToString()}-{header.SubProduct.Name}";
                                if (!grp.OtherData.ContainsKey(z)) {
                                    grp.OtherData.Add(z, new OrganizerData());
                                }
                                od = grp.OtherData[z];
                                break;
                            case 8:
                                if (satellite == "G16") {
                                    od = grp.WaterVapour;
                                    break;
                                }
                                z = $"{timestamp%1000}-{((NOAAProductID)header.Product.ID).ToString()}-{header.SubProduct.Name}";
                                if (!grp.OtherData.ContainsKey(z)) {
                                    grp.OtherData.Add(z, new OrganizerData());
                                }
                                od = grp.OtherData[z];
                                break;
                            case 13: // Infrared for G16
                                if (satellite == "G16") {
                                    od = grp.Infrared;
                                    break;
                                }
                                z = $"{timestamp%1000}-{((NOAAProductID)header.Product.ID).ToString()}-{header.SubProduct.Name}";
                                if (!grp.OtherData.ContainsKey(z)) {
                                    grp.OtherData.Add(z, new OrganizerData());
                                }
                                od = grp.OtherData[z];
                                break;
                            default:
                                z = $"{timestamp%1000}-{((NOAAProductID)header.Product.ID).ToString()}-{header.SubProduct.Name}";
                                if (!grp.OtherData.ContainsKey(z)) {
                                    grp.OtherData.Add(z, new OrganizerData());
                                }
                                od = grp.OtherData[z];
                                break;
                        } 
                        od.Code = grp.Code;
                        od.Timestamp = timestamp;
                        od.Segments[segmentId] = file;
                        od.FirstSegment = Math.Min(od.FirstSegment, segmentId);
                        od.FileHeader = header;
                        if (od.Columns == -1) {
                            od.Columns = header.ImageStructureHeader.Columns;
                            od.Lines = header.ImageStructureHeader.Lines;
                            od.PixelAspect = header.ImageNavigationHeader.ColumnScalingFactor / (float)header.ImageNavigationHeader.LineScalingFactor;
                            od.ColumnOffset = header.ImageNavigationHeader.ColumnOffset;
                            od.MaxSegments = header.SegmentIdentificationHeader != null ? header.SegmentIdentificationHeader.MaxSegments : 1;
                        } else {
                            od.Lines += header.ImageStructureHeader.Lines;
                        }
                        alreadyProcessed.Add(file);
                    } catch (Exception e) {
                        UIConsole.Error($"Error reading file {file}: {e}");
                        alreadyProcessed.Add(file);
                    }
                }
            } catch (Exception e) {
                UIConsole.Error ($"Error checking folders: {e}");
            }
        }
    }
}


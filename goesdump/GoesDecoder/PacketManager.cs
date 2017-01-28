using System;
using System.Linq;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using OpenSatelliteProject.PacketData;
using OpenSatelliteProject.PacketData.Enums;
using OpenSatelliteProject.PacketData.Structs;

namespace OpenSatelliteProject {
    public static class PacketManager {

        private static readonly int MAX_RUNS = 20;

        private static readonly string DCSRgex = @"DCSdat(.*)";
        private static readonly string XXRgex = @"gos(.*)XX(.*).lrit";
        private static readonly string FDRgex = @"(.*)FD(.*).lrit";
        private static readonly string GOSRgex = @"gos(.*).lrit";
        private static readonly string ChartRgex = @"NWSchrt(.*).lrit";
        private static readonly string TextRgex = @"(.*)TEXTdat(.*).lrit";

        private static readonly string DCSFolder = "DCS";
        private static readonly string FullDiskFolder = "FullDisk";
        private static readonly string XXFolder = "XX";
        private static readonly string ImagesFolder = "Images";
        private static readonly string TextFolder = "Text";
        private static readonly string ChartFolder = "Charts";
        private static readonly string EMWINFolder = "EMWIN";
        private static readonly string WeatherDataFolder = "Weather Data";
        private static readonly string OtherSatellitesFolder = "Other Satellites";
        private static readonly string UnknownDataFolder = "Unknown";

        private static Regex dcsRegex = new Regex(DCSRgex, RegexOptions.IgnoreCase);
        private static Regex xxRegex = new Regex(XXRgex, RegexOptions.IgnoreCase);
        private static Regex fdRegex = new Regex(FDRgex, RegexOptions.IgnoreCase);
        private static Regex chartRegex = new Regex(ChartRgex, RegexOptions.IgnoreCase);
        private static Regex gosRegex = new Regex(GOSRgex, RegexOptions.IgnoreCase);
        private static Regex textRegex = new Regex(TextRgex, RegexOptions.IgnoreCase);

        private static string FixFileFolder(string dir, string filename, NOAAProduct product, NOAASubproduct subProduct) {
            string filef = filename;
            string basedir = new DirectoryInfo(dir).Parent.FullName;

            if (product != null && product.ID != -1) {
                // New way
                string folderName = UnknownDataFolder;
                if (product.ID == (int)NOAAProductID.SCANNER_DATA_1 || product.ID == (int)NOAAProductID.SCANNER_DATA_2) {
                    switch (subProduct.ID) {
                        case (int)ScannerSubProduct.INFRARED_AREA_OF_INTEREST:
                        case (int)ScannerSubProduct.VISIBLE_AREA_OF_INTEREST:
                        case (int)ScannerSubProduct.WATERVAPOUR_AREA_OF_INTEREST:
                            folderName = Path.Combine(ImagesFolder, "Area of Interest");
                            break;
                        case (int)ScannerSubProduct.INFRARED_FULLDISK:
                        case (int)ScannerSubProduct.VISIBLE_FULLDISK:
                        case (int)ScannerSubProduct.WATERVAPOUR_FULLDISK:
                            folderName = Path.Combine(ImagesFolder, "Full Disk");
                            break;
                        case (int)ScannerSubProduct.INFRARED_NORTHERN:
                        case (int)ScannerSubProduct.VISIBLE_NORTHERN:
                        case (int)ScannerSubProduct.WATERVAPOUR_NORTHERN:
                            folderName = Path.Combine(ImagesFolder, "Northern Hemisphere");
                            break;
                        case (int)ScannerSubProduct.INFRARED_SOUTHERN:
                        case (int)ScannerSubProduct.VISIBLE_SOUTHERN:
                        case (int)ScannerSubProduct.WATERVAPOUR_SOUTHERN:
                            folderName = Path.Combine(ImagesFolder, "Southern Hemisphere");
                            break;
                        case (int)ScannerSubProduct.INFRARED_UNITEDSTATES:
                        case (int)ScannerSubProduct.VISIBLE_UNITEDSTATES:
                        case (int)ScannerSubProduct.WATERVAPOUR_UNITEDSTATES:
                            folderName = Path.Combine(ImagesFolder, "United States");
                            break;
                        default:
                            folderName = Path.Combine(ImagesFolder, UnknownDataFolder);
                            break;
                    }
                } else {
                    switch (product.ID) {
                        case (int)NOAAProductID.DCS:
                            folderName = DCSFolder;
                            break;
                        case (int)NOAAProductID.EMWIN:
                            folderName = EMWINFolder;
                            break;
                        case (int)NOAAProductID.NOAA_TEXT:
                            folderName = TextFolder;
                            break;
                        case (int)NOAAProductID.OTHER_SATELLITES_1:
                        case (int)NOAAProductID.OTHER_SATELLITES_2:
                            folderName = OtherSatellitesFolder;
                            break;
                        case (int)NOAAProductID.WEATHER_DATA:
                            folderName = WeatherDataFolder;
                            break;
                        default:
                            folderName = UnknownDataFolder;
                            break;
                    }
                }

                dir = Path.Combine(basedir, DCSFolder);

                if (!Directory.Exists(dir)) {
                    Directory.CreateDirectory(dir);
                }

                filef = Path.Combine(dir, filename);

            } else {
                // Old way
                string folderName = UnknownDataFolder;
                if (dcsRegex.IsMatch(filename)) {
                    folderName = DCSFolder;
                } else if (xxRegex.IsMatch(filename)) {
                    folderName = Path.Combine(ImagesFolder, "Area of Interest");
                } else if (fdRegex.IsMatch(filename)) { 
                    folderName = Path.Combine(ImagesFolder, "Full Disk");
                } else if (chartRegex.IsMatch(filename)) {
                    folderName = WeatherDataFolder;
                } else if (gosRegex.IsMatch(filename)) {
                    folderName = Path.Combine(ImagesFolder, UnknownDataFolder);
                } else if (textRegex.IsMatch(filename)) {
                    folderName = TextFolder;
                } else {
                    folderName = UnknownDataFolder;
                }

                dir = Path.Combine(basedir, folderName);

                if (!Directory.Exists(dir)) {
                    Directory.CreateDirectory(dir);
                }

                filef = Path.Combine(dir, filename);
            }

            return filef;
        }

        public static void ManageJPGFile(string filename, XRITHeader fileHeader) {
            string dir = Path.GetDirectoryName(filename);
            string f = FixFileFolder(dir, fileHeader.Filename, fileHeader.Product, fileHeader.SubProduct);
            f = f.Replace(".lrit", ".jpg");
            UIConsole.GlobalConsole.Log(String.Format("New JPEG file {0}", fileHeader.Filename));
            Console.WriteLine("Renaming {0} to {1}", filename, f);
            FileStream fs = File.OpenRead(filename);
            fs.Seek(fileHeader.PrimaryHeader.HeaderLength, SeekOrigin.Begin);
            FileStream os = File.OpenWrite(f);

            byte[] buffer = new Byte[1024];
            int bytesRead;

            while ((bytesRead = fs.Read(buffer, 0, 1024)) > 0) {
                os.Write(buffer, 0, bytesRead);
            }

            fs.Close();
            os.Close();

            // Keep the original lrit file
            File.Move(filename, f.Replace(".jpg", ".lrit"));
        }

        public static void ManageGIFFile(string filename, XRITHeader fileHeader) {
            string dir = Path.GetDirectoryName(filename);
            string f = FixFileFolder(dir, fileHeader.Filename, fileHeader.Product, fileHeader.SubProduct);
            f = f.Replace(".lrit", ".gif");
            UIConsole.GlobalConsole.Log(String.Format("New GIF file {0}", fileHeader.Filename));
            Console.WriteLine("Renaming {0} to {1}", filename, f);
            FileStream fs = File.OpenRead(filename);
            fs.Seek(fileHeader.PrimaryHeader.HeaderLength, SeekOrigin.Begin);
            FileStream os = File.OpenWrite(f);

            byte[] buffer = new Byte[1024];
            int bytesRead;

            while ((bytesRead = fs.Read(buffer, 0, 1024)) > 0) {
                os.Write(buffer, 0, bytesRead);
            }

            fs.Close();
            os.Close();

            // Keep the original lrit file
            File.Move(filename, f.Replace(".gif", ".lrit"));
        }

        public static void ManageFile(string filename, XRITHeader fileHeader) {
            if (fileHeader.Filename == null) {
                //Console.WriteLine("Cannot find filename for {0}", filename);
            } else {
                switch (fileHeader.Compression) {
                    case CompressionType.JPEG:
                        ManageJPGFile(filename, fileHeader);
                        break;
                    case CompressionType.GIF:
                        ManageGIFFile(filename, fileHeader);
                        break;
                    default:
                        string dir = Path.GetDirectoryName(filename);
                        string f = FixFileFolder(dir, fileHeader.Filename, fileHeader.Product, fileHeader.SubProduct);
                        UIConsole.GlobalConsole.Log(String.Format("New file {0}", fileHeader.Filename));
                        //Console.WriteLine("Renaming {0} to {1}", filename, Path.Combine(dir, fname));
                        File.Move(filename, f);
                        break;
                }
            }
        }

        public static string Decompressor(string filename, int pixels) {
            try {
                Process decompressor = new Process();
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                startInfo.FileName = "wine";
                startInfo.Arguments = String.Format("Decompress.exe {0} {1} a", pixels, filename);
                startInfo.RedirectStandardError = true;
                startInfo.RedirectStandardOutput = true;
                startInfo.CreateNoWindow = true;
                startInfo.UseShellExecute = false;
                startInfo.EnvironmentVariables.Add("WINEDEBUG", "fixme-all,err-winediag");

                decompressor.StartInfo = startInfo;

                UIConsole.GlobalConsole.Debug(String.Format("Calling {0}", startInfo.Arguments));
                decompressor.Start();
                decompressor.WaitForExit();

                if (decompressor.ExitCode != 0) {
                    string stderr = decompressor.StandardError.ReadToEnd();
                    UIConsole.GlobalConsole.Error(String.Format("Error Decompressing: {0}", stderr));
                } else {
                    UIConsole.GlobalConsole.Debug(String.Format("Decompress sucessful to {0}", String.Format("{0}_decomp.lrit", filename)));
                }

            } catch (Exception e) {
                UIConsole.GlobalConsole.Error(String.Format("Error running decompressor: {0}", e));
            }


            return String.Format("{0}_decomp.lrit", filename);
        }


        public static string Decompressor(string prefix, int pixels, int startnum, int endnum) {
            try {
                Process decompressor = new Process();
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                startInfo.FileName = "wine";
                startInfo.Arguments = String.Format("Decompress.exe {0} {1} {2} {3} a", prefix, pixels, startnum + 1, endnum);
                startInfo.RedirectStandardError = true;
                startInfo.RedirectStandardOutput = true;
                startInfo.CreateNoWindow = true;
                startInfo.UseShellExecute = false;
                startInfo.EnvironmentVariables.Add("WINEDEBUG", "fixme-all,err-winediag");

                decompressor.StartInfo = startInfo;

                UIConsole.GlobalConsole.Debug(String.Format("Calling {0}", startInfo.Arguments));
                decompressor.Start();
                decompressor.WaitForExit();

                if (decompressor.ExitCode != 0) {
                    string stderr = decompressor.StandardError.ReadToEnd();
                    UIConsole.GlobalConsole.Error(String.Format("Error Decompressing: {0}", stderr));
                } else {
                    UIConsole.GlobalConsole.Debug(String.Format("Decompress sucessful to {0}", String.Format("{0}_decomp{1}.lrit", prefix, startnum)));
                }

            } catch (Exception e) {
                UIConsole.GlobalConsole.Error(String.Format("Error running decompressor: {0}", e));
            }


            return String.Format("{0}_decomp{1}.lrit", prefix, startnum);
        }

        public static string GetFilename(byte[] data) {
            string filename = "--";
            int runs = 0;

            if (data.Length == 0) {
                return filename;
            }
            while (true) {
                byte type = data[0];
                byte[] cb = data.Skip(1).Take(2).ToArray();

                if (BitConverter.IsLittleEndian) {
                    Array.Reverse(cb);
                }

                UInt16 size = BitConverter.ToUInt16(cb, 0);
                if (type == 4) {
                    return System.Text.Encoding.UTF8.GetString(data.Skip(3).Take(size - 3).ToArray());
                }

                data = data.Skip(size).ToArray();
                if (data.Length == 0) {
                    break;
                }
                runs++;
                if (runs >= MAX_RUNS) {
                    break;
                }
            }

            return filename;
        }

        public static XRITHeader GetHeader(byte[] data) {
            List<XRITBaseHeader> headers = GetHeaderData(data);
            return new XRITHeader(headers);
        }

        public static List<XRITBaseHeader> GetHeaderData(byte[] data) {
            List<XRITBaseHeader> headers = new List<XRITBaseHeader>();
            int maxLength = data.Length; // Initial Guess
            int c = 0;

            while (c < maxLength) {
                int type = data[0];
                byte[] tmp = data.Skip(1).Take(2).ToArray();

                if (BitConverter.IsLittleEndian) {
                    Array.Reverse(tmp);
                }

                int size = BitConverter.ToUInt16(tmp, 0);
                tmp = data.Take(size).ToArray();

                if (tmp.Length < size) {
                    Console.WriteLine("Not enough data for unpack header.");
                    break;
                }

                XRITBaseHeader h;
                switch (type) {
                    case (int)HeaderType.PrimaryHeader:
                        PrimaryRecord fh = Tools.ByteArrayToStruct<PrimaryRecord>(tmp);
                        fh = Tools.StructToSystemEndian(fh);
                        h = new PrimaryHeader(fh);
                        maxLength = (int)fh.HeaderLength; // Set the correct size
                        break;
                    case (int)HeaderType.ImageStructureRecord:
                        ImageStructureRecord isr = Tools.ByteArrayToStruct<ImageStructureRecord>(tmp);
                        isr = Tools.StructToSystemEndian(isr);
                        h = new ImageStructureHeader(isr);
                        break;
                    case (int)HeaderType.ImageNavigationRecord:
                        ImageNavigationRecord inr = Tools.ByteArrayToStruct<ImageNavigationRecord>(tmp);
                        inr = Tools.StructToSystemEndian(inr);
                        h = new ImageNavigationHeader(inr);
                        break;
                    case (int)HeaderType.ImageDataFunctionRecord:
                        // Cannot marshable due variable length
                        //ImageDataFunctionRecord idfr = Tools.ByteArrayToStruct<ImageDataFunctionRecord>(tmp);
                        //idfr = Tools.StructToSystemEndian(idfr);
                        ImageDataFunctionRecord idfr = new ImageDataFunctionRecord();
                        idfr.Data = System.Text.Encoding.UTF8.GetString(tmp.Skip(3).ToArray());
                        h = new ImageDataFunctionHeader(idfr);
                        break;
                    case (int)HeaderType.AnnotationRecord:
                        // Cannot be marshalled due variable length
                        //AnnotationRecord ar = Tools.ByteArrayToStruct<AnnotationRecord>(tmp);
                        //ar = Tools.StructToSystemEndian(ar);
                        AnnotationRecord ar = new AnnotationRecord();
                        ar.Filename = System.Text.Encoding.UTF8.GetString(tmp.Skip(3).ToArray());
                        h = new AnnotationHeader(ar);
                        break;
                    case (int)HeaderType.TimestampRecord:
                        TimestampRecord tr = Tools.ByteArrayToStruct<TimestampRecord>(tmp);
                        tr = Tools.StructToSystemEndian(tr);
                        h = new TimestampHeader(tr);
                        break;
                    case (int)HeaderType.AncillaryTextRecord:
                        // Cannot be marshalled due variable length.
                        // AncillaryText at = Tools.ByteArrayToStruct<AncillaryText>(tmp);
                        //at = Tools.StructToSystemEndian(at);
                        AncillaryText at = new AncillaryText();
                        at.Data = System.Text.Encoding.UTF8.GetString(tmp.Skip(3).ToArray());
                        h = new AncillaryHeader(at);
                        break;
                    case (int)HeaderType.KeyRecord:
                        h = new XRITBaseHeader(HeaderType.KeyRecord, tmp);
                        break;
                    case (int)HeaderType.SegmentIdentificationRecord:
                        SegmentIdentificationRecord sir = Tools.ByteArrayToStruct<SegmentIdentificationRecord>(tmp);
                        sir = Tools.StructToSystemEndian(sir);
                        h = new SegmentIdentificationHeader(sir);
                        break;
                    case (int)HeaderType.NOAASpecificHeader:
                        NOAASpecificRecord nsr = Tools.ByteArrayToStruct<NOAASpecificRecord>(tmp);
                        nsr = Tools.StructToSystemEndian(nsr);
                        h = new NOAASpecificHeader(nsr);
                        break;
                    case (int)HeaderType.HeaderStructuredRecord:
                        // Cannot be marshalled due variable length
                        //HeaderStructuredRecord hsr = Tools.ByteArrayToStruct<HeaderStructuredRecord>(tmp);
                        //hsr = Tools.StructToSystemEndian(hsr); // Header Structured Record doesn't have endianess dependant fields
                        HeaderStructuredRecord hsr = new HeaderStructuredRecord();
                        hsr.Data = System.Text.Encoding.UTF8.GetString(tmp.Skip(3).ToArray());
                        h = new HeaderStructuredHeader(hsr);
                        break;
                    case (int)HeaderType.RiceCompressionRecord:
                        RiceCompressionRecord rcr = Tools.ByteArrayToStruct<RiceCompressionRecord>(tmp);
                        rcr = Tools.StructToSystemEndian(rcr);
                        h = new RiceCompressionHeader(rcr);
                        break;
                    case (int)HeaderType.DCSFileNameRecord:
                        // Cannot be marshalled due variable length
                        //DCSFilenameRecord dfr = Tools.ByteArrayToStruct<DCSFilenameRecord>(tmp);
                        //dfr = Tools.StructToSystemEndian(dfr); // DCS Filename Record doesn't have endianess dependant fields
                        DCSFilenameRecord dfr = new DCSFilenameRecord();
                        dfr.Filename = System.Text.Encoding.UTF8.GetString(tmp.Skip(3).ToArray());
                        h = new DCSFilenameHeader(dfr);
                        break;
                    default:
                        h = new XRITBaseHeader();
                        h.Type = HeaderType.Unknown;
                        break;
                }

                h.RawData = tmp;
                headers.Add(h);
                c += size;
                data = data.Skip(size).ToArray();
            }

            return headers;
        }

    }
}


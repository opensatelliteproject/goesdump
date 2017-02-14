using System;
using System.Linq;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using OpenSatelliteProject.PacketData;
using OpenSatelliteProject.PacketData.Enums;
using OpenSatelliteProject.PacketData.Structs;
using OpenSatelliteProject.Tools;

namespace OpenSatelliteProject {
    public static class PacketManager {
        private static readonly string DCSRgex = @"DCSdat(.*)";
        private static readonly string XXRgex = @"gos(.*)XX(.*).lrit";
        private static readonly string FDRgex = @"(.*)FD(.*).lrit";
        private static readonly string GOSRgex = @"gos(.*).lrit";
        private static readonly string ChartRgex = @"NWSchrt(.*).lrit";
        private static readonly string TextRgex = @"(.*)TEXTdat(.*).lrit";

        private static readonly string DCSFolder = "DCS";
        private static readonly string ImagesFolder = "Images";
        private static readonly string TextFolder = "Text";
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

        public static string FixFileFolder(string dir, string filename, NOAAProduct product, NOAASubproduct subProduct) {
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

                dir = Path.Combine(basedir, folderName);

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

        static PacketManager() {
            FileHandler.AttachByCompressionHandler((int)CompressionType.JPEG, (filename, fileHeader) => DumpFile(filename, fileHeader, "jpg"));
            FileHandler.AttachByCompressionHandler((int)CompressionType.GIF, (filename, fileHeader) => DumpFile(filename, fileHeader, "gif"));
            FileHandler.AttachByProductIdHandler((int)NOAAProductID.WEATHER_DATA, HandleWeatherData);
        }

        public static void HandleWeatherData(string filename, XRITHeader header) {
            if (header.PrimaryHeader.FileType == FileTypeCode.IMAGE) {
                string basedir = new DirectoryInfo(Path.GetDirectoryName(filename)).Parent.FullName;
                basedir = Path.Combine(basedir, WeatherDataFolder);

                try {
                    ImageHandler.Handler.HandleFile(filename, basedir);
                    File.Delete(filename);
                } catch (Exception e) {
                    UIConsole.GlobalConsole.Warn(string.Format("Failed to parse Weather Data Image at {0}: {1}", filename, e));
                }
            } else {
                FileHandler.DefaultHandler(filename, header);
            }
        }

        public static void DumpFile(string filename, XRITHeader fileHeader, string newExt) {
            string dir = Path.GetDirectoryName(filename);
            string f = FixFileFolder(dir, fileHeader.Filename, fileHeader.Product, fileHeader.SubProduct);
            f = f.Replace(".lrit", "." + newExt);

            if (File.Exists(f)) {
                string timestamp = DateTime.Now.ToString("yyyyMMddHHmmssffff");
                string ext = Path.GetExtension(f);
                string append = String.Format("--dup-{0}{1}", timestamp, ext);
                f = f.Replace(String.Format("{0}", ext), append);
            }

            if (!String.Equals(Path.GetFileName(f), fileHeader.Filename)) {
                if (fileHeader.SubProduct.Name != "Unknown") {
                    UIConsole.GlobalConsole.Log(String.Format("New {0} - {1} ({2}) saved as {3}", fileHeader.Product.Name, fileHeader.SubProduct.Name, fileHeader.Filename, Path.GetFileName(f)));
                } else {
                    UIConsole.GlobalConsole.Log(String.Format("New {0} ({1}) saved as {2}", fileHeader.Product.Name, fileHeader.Filename, Path.GetFileName(f)));
                }
            } else {
                if (fileHeader.SubProduct.Name != "Unknown") {
                    UIConsole.GlobalConsole.Log(String.Format("New {0} - {1} ({2})", fileHeader.Product.Name, fileHeader.SubProduct.Name, fileHeader.Filename));
                } else {
                    UIConsole.GlobalConsole.Log(String.Format("New {0} ({1})", fileHeader.Product.Name, fileHeader.Filename));
                }
            }

            //UIConsole.GlobalConsole.Log(String.Format("New JPEG file {0}", fileHeader.Filename));
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
            File.Move(filename, f.Replace("." + newExt, ".lrit"));
        }

        public static string Decompressor(string filename, int pixels) {
            try {
                Process decompressor = new Process();
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;

                if (LLTools.IsLinux) {
                    startInfo.FileName = "wine";
                    startInfo.Arguments = String.Format("Decompress.exe {0} {1} a", pixels, filename);
                    startInfo.EnvironmentVariables.Add("WINEDEBUG", "fixme-all,err-winediag");
                } else {
                    startInfo.FileName = "Decompress.exe";
                    startInfo.Arguments = String.Format("{0} {1} a", pixels, filename);
                }

                startInfo.RedirectStandardError = true;
                startInfo.RedirectStandardOutput = true;
                startInfo.CreateNoWindow = true;
                startInfo.UseShellExecute = false;

                decompressor.StartInfo = startInfo;

                UIConsole.GlobalConsole.Debug(String.Format("Calling {0}", startInfo.Arguments));
                decompressor.Start();
                decompressor.WaitForExit();

                if (decompressor.ExitCode != 0) {
                    string stderr = decompressor.StandardError.ReadToEnd();
                    UIConsole.GlobalConsole.Error(String.Format("Error Decompressing: {0}", stderr));
                } else {
                    UIConsole.GlobalConsole.Debug(String.Format("Decompress sucessful to {0}", String.Format("{0}_decomp.lrit", filename)));
                    try {
                        File.Delete(filename);
                    } catch (Exception e) {
                        Console.WriteLine("Cannot delete file {0}: {1}", filename, e);
                    }
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
                if (LLTools.IsLinux) {
                    startInfo.FileName = "wine";
                    startInfo.Arguments = String.Format("Decompress.exe {0} {1} {2} {3} a", prefix, pixels, startnum + 1, endnum);
                    startInfo.EnvironmentVariables.Add("WINEDEBUG", "fixme-all,err-winediag");
                } else {
                    startInfo.FileName = "Decompress.exe";
                    startInfo.Arguments =  String.Format("{0} {1} {2} {3} a", prefix, pixels, startnum + 1, endnum);
                }

                startInfo.RedirectStandardError = true;
                startInfo.RedirectStandardOutput = true;
                startInfo.CreateNoWindow = true;
                startInfo.UseShellExecute = false;

                decompressor.StartInfo = startInfo;

                UIConsole.GlobalConsole.Debug(String.Format("Calling {0}", startInfo.Arguments));
                decompressor.Start();
                decompressor.WaitForExit();

                if (decompressor.ExitCode != 0) {
                    string stderr = decompressor.StandardError.ReadToEnd();
                    UIConsole.GlobalConsole.Error(String.Format("Error Decompressing: {0}", stderr));
                } else {
                    UIConsole.GlobalConsole.Debug(String.Format("Decompress sucessful to {0}", String.Format("{0}_decomp{1}.lrit", prefix, startnum)));
                    for (int i=startnum; i<endnum+1; i++) {
                        string f = string.Format("{0}{1}.lrit", prefix, i);
                        try {
                            File.Delete(f);
                        } catch (Exception e) {
                            Console.WriteLine("Error deleting file {0}: {1}", f, e);
                        }
                    }
                }

            } catch (Exception e) {
                UIConsole.GlobalConsole.Error(String.Format("Error running decompressor: {0}", e));
            }


            return String.Format("{0}_decomp{1}.lrit", prefix, startnum);
        }
    }
}


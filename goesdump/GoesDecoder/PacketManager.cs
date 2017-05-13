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
using ICSharpCode.SharpZipLib.Zip;

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

        public static string GetFolderByProduct(NOAAProductID product, int subProduct) {
            // TODO: Unify with other functions that use the same thing
            string folderName = UnknownDataFolder;
            if (product == NOAAProductID.GOES16_ABI) {
                folderName = Path.Combine(ImagesFolder, "FM1");
            } else  if (product == NOAAProductID.GOES13_ABI || product == NOAAProductID.GOES15_ABI) {
                switch (subProduct) {
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
                switch (product) {
                    case NOAAProductID.DCS:
                        folderName = DCSFolder;
                        break;
                    case NOAAProductID.EMWIN:
                        folderName = EMWINFolder;
                        break;
                    case NOAAProductID.NOAA_TEXT:
                        folderName = TextFolder;
                        break;
                    case NOAAProductID.OTHER_SATELLITES_1:
                    case NOAAProductID.OTHER_SATELLITES_2:
                        folderName = OtherSatellitesFolder;
                        break;
                    case NOAAProductID.WEATHER_DATA:
                        folderName = WeatherDataFolder;
                        break;
                    default:
                        folderName = UnknownDataFolder;
                        break;
                }
            }
            return Path.Combine(FileHandler.FinalFileFolder, folderName);
        }

        public static string FixFileFolder(string dir, string filename, NOAAProduct product, NOAASubproduct subProduct) {
            string filef = LLTools.FixPathString(filename);
            string basedir = FileHandler.FinalFileFolder;

            if (product != null && product.ID != -1) {
                // New way
                string folderName = UnknownDataFolder;
                if (product.ID == (int)NOAAProductID.GOES16_ABI) {
                    folderName = Path.Combine(ImagesFolder, "FM1");
                } else if (product.ID == (int)NOAAProductID.HIMAWARI8_ABI) {
                    folderName = Path.Combine(ImagesFolder, "Full Disk");
                } else if (product.ID == (int)NOAAProductID.GOES13_ABI || product.ID == (int)NOAAProductID.GOES15_ABI) {
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
                            if (filename.Contains("KWIN")) {
                                folderName = EMWINFolder; // HRIT EMWIN
                            } else {
                                folderName = WeatherDataFolder;
                            }
                            break;
                        default:
                            if (filename.Contains("KWIN")) {
                                folderName = EMWINFolder; // HRIT EMWIN
                            } else {
                                folderName = UnknownDataFolder;
                            }
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
            FileHandler.AttachByCompressionHandler((int)CompressionType.JPEG, (filename, fileHeader) => DumpFile(filename, fileHeader, "jpg", true));
            FileHandler.AttachByCompressionHandler((int)CompressionType.GIF, (filename, fileHeader) => DumpFile(filename, fileHeader, "gif", true));
            FileHandler.AttachByProductIdHandler((int)NOAAProductID.WEATHER_DATA, HandleWeatherData);
            FileHandler.AttachByProductIdHandler((int)NOAAProductID.OTHER_SATELLITES_1, HandleWeatherData);
            FileHandler.AttachByProductIdHandler((int)NOAAProductID.OTHER_SATELLITES_2, HandleWeatherData);
            FileHandler.AttachByProductIdHandler((int)NOAAProductID.NOAA_TEXT, HandleTextData);
            FileHandler.AttachByProductIdHandler((int)NOAAProductID.HRIT_EMWIN, (filename, fileHeader) => DumpFile(filename, fileHeader, "txt", true));
        }

        public static void ExtractZipFile(string zipfile) {
            try {
                string basepath = Path.GetDirectoryName(zipfile);

                using (ZipInputStream s = new ZipInputStream(File.OpenRead(zipfile))) {
                    ZipEntry theEntry;
                    while ((theEntry = s.GetNextEntry()) != null) {
                        string directoryName = Path.GetDirectoryName(theEntry.Name);
                        string fileName      = Path.GetFileName(theEntry.Name);
                        string baseFileName  = Path.GetFileName(theEntry.Name);

                        directoryName = Path.Combine(basepath, directoryName);

                        if ( directoryName.Length > 0 ) {
                            Directory.CreateDirectory(directoryName);
                        }

                        fileName = Path.Combine(directoryName, fileName);

                        if (fileName != String.Empty) {
                            int count = 1;
                            while (File.Exists(fileName)) {
                                fileName = String.Format("{0}__{1}{2}", Path.GetFileNameWithoutExtension(baseFileName), count, Path.GetExtension(baseFileName));
                                fileName = Path.Combine(directoryName, fileName);
                                if (count == 1000) {
                                    count = 0;
                                    break;
                                }
                                count++;
                            }
                            UIConsole.GlobalConsole.Debug(String.Format("Saving file {0}", fileName));

                            using (FileStream streamWriter = File.Create(fileName)) {

                                const int MAXSIZE = 40 * 1024 * 1024;
                                int size = 2048;
                                int readBytes = 0;
                                byte[] data = new byte[2048];
                                while (readBytes < MAXSIZE) {
                                    size = s.Read(data, 0, data.Length);
                                    readBytes+= size;
                                    if (size > 0) {
                                        streamWriter.Write(data, 0, size);
                                    } else {
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
            } catch (Exception e) {
                UIConsole.GlobalConsole.Error(String.Format("Error extracting file {0}: {1}", zipfile, e));
            }

            try {
                File.Delete(zipfile);
            } catch (Exception) {
            
            }
        }

        public static void HandleWeatherData(string filename, XRITHeader header) {
            if (header.Filename.Contains("KWIN")) {
                // HRIT EMWIN wrongly has product id of weather data
                if (FileHandler.SkipEMWIN) {
                    try {
                        File.Delete(filename);
                    } catch (Exception) {
                        // Do nothing, file doesn't exists
                    }
                    return;
                }
                string fz = null;
                switch (header.Compression) {
                    case CompressionType.ZIP:
                        fz = DumpFile(filename, header, "zip");
                        break;
                    case CompressionType.GIF:
                        fz = DumpFile(filename, header, "gif");
                        break;
                    case CompressionType.JPEG:
                        fz = DumpFile(filename, header, "jpg");
                        break;
                    case CompressionType.NO_COMPRESSION:
                        fz = DumpFile(filename, header, "txt");
                        break;
                    default:
                        fz = DumpFile(filename, header, "bin");
                        break;
                }
                if (fz != null) {
                    try {
                        File.Delete(fz);
                    } catch (Exception) {
                        // Do nothing, file doesn't exists
                    }
                }
            } else if (header.PrimaryHeader.FileType == FileTypeCode.IMAGE) {
                string basedir = FileHandler.FinalFileFolder;
                if (header.Product.ID == (int)NOAAProductID.OTHER_SATELLITES_1 || header.Product.ID == (int)NOAAProductID.OTHER_SATELLITES_2) {
                    basedir = Path.Combine(basedir, OtherSatellitesFolder);
                } else {
                    basedir = Path.Combine(basedir, WeatherDataFolder);
                }

                try {
                    UIConsole.GlobalConsole.Log(string.Format("New Weather Data - {0} - {1}", header.SubProduct.Name, header.Filename));
                    if (!Directory.Exists(basedir)) {
                        Directory.CreateDirectory(basedir);
                    }
                    ImageHandler.Handler.HandleFile(filename, basedir);
                    File.Delete(filename);
                } catch (Exception e) {
                    UIConsole.GlobalConsole.Warn(string.Format("Failed to parse Weather Data Image at {0}: {1}", filename, e));
                }
            } else if (header.PrimaryHeader.FileType == FileTypeCode.TEXT) {
                string fz = DumpFile(filename, header, "txt");

                if (fz != null) {
                    try {
                        File.Delete(fz);
                    } catch (Exception) {
                        // Do nothing, file doesn't exists
                    }
                }
            } else {
                FileHandler.DefaultHandler(filename, header);
            }
        }

        public static void HandleTextData(string filename, XRITHeader header) {
            if (header.PrimaryHeader.FileType == FileTypeCode.TEXT) {
                string basedir = FileHandler.FinalFileFolder;
                basedir = Path.Combine(basedir, TextFolder);

                try {
                    UIConsole.GlobalConsole.Log(string.Format("New NOAA Text ({0})", header.Filename));
                    if (!Directory.Exists(basedir)) {
                        Directory.CreateDirectory(basedir);
                    }
                    TextHandler.Handler.HandleFile(filename, basedir);
                    File.Delete(filename);
                } catch (Exception e) {
                    UIConsole.GlobalConsole.Warn(string.Format("Failed to parse Weather Data Image at {0}: {1}", filename, e));
                }
            } else {
                FileHandler.DefaultHandler(filename, header);
            }
        }

        public static string DumpFile(string filename, XRITHeader fileHeader, string newExt, bool forceErase = false) {
            string dir = Path.GetDirectoryName(filename);
            string f = FixFileFolder(dir, fileHeader.Filename, fileHeader.Product, fileHeader.SubProduct);
            f = f.Replace(".lrit", "." + newExt);

            if (
                FileHandler.SkipEMWIN && fileHeader.Product.ID == (int)NOAAProductID.HRIT_EMWIN ||
                FileHandler.SkipWeatherData && f.Contains(WeatherDataFolder)
            )  {
                try {
                    File.Delete(filename);
                } catch (Exception) {
                    // Do nothing, file doesn't exists
                }
                return null;
            }

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

            if (f.Contains(".zip")) {
                UIConsole.GlobalConsole.Log(String.Format("Extracting Zip File {0}", f));
                ExtractZipFile(f);
            }
            if (!forceErase) {
                // Keep the original lrit file
                File.Move(filename, f.Replace("." + newExt, ".lrit"));
                return f.Replace("." + newExt, ".lrit");
            } else {
                try {
                    File.Delete(filename);
                } catch (Exception) {
                    // Do nothing, file doesn't exists
                }
                return null;
            }
        }

        public static byte[] GenerateFillData(int pixels) {
            byte[] outputData = new byte[pixels];

            for (int i = 0; i < pixels; i++) {
                outputData[i] = 0x00;
            }

            return outputData;
        }

        public static byte[] InMemoryDecompress(byte[] compressedData, int pixels, int pixelsPerBlock, int mask) {
            byte[] outputData = GenerateFillData(pixels);

            try {
                AEC.LritRiceDecompress(ref outputData, compressedData, 8, pixelsPerBlock, pixels, mask);
            } catch (Exception e) {
                if (e is AECException) {
                    AECException aece = (AECException)e;
                    UIConsole.GlobalConsole.Error(string.Format("AEC Decompress Error: {0}", aece.status.ToString()));
                } else {
                    UIConsole.GlobalConsole.Error(string.Format("Decompress error: {0}", e.ToString()));
                }
            }

            return outputData;
        }

        public static string Decompressor(string filename, int pixels, int pixelsPerBlock, int mask) {
            /**
             *  Temporary Workarround. Needs to change directly on Demuxer
             */
            string outputFile = String.Format("{0}_decomp.lrit", filename);
            byte[] outputData = new byte[pixels];

            for (int i = 0; i < pixels; i++) {
                outputData[i] = 0x00;
            }

            try {
                byte[] inputData = File.ReadAllBytes(filename);
                AEC.LritRiceDecompress(ref outputData, inputData, 8, pixelsPerBlock, pixels, mask); //  AEC.ALLOW_K13_OPTION_MASK | AEC.MSB_OPTION_MASK | AEC.NN_OPTION_MASK
                File.Delete(filename);
            } catch (Exception e) {
                if (e is AECException) {
                    AECException aece = (AECException)e;
                    UIConsole.GlobalConsole.Error(string.Format("AEC Decompress Error: {0}", aece.status.ToString()));
                } else {
                    UIConsole.GlobalConsole.Error(string.Format("Decompress error: {0}", e.ToString()));
                }
            }

            File.WriteAllBytes(outputFile, outputData);
            return outputFile;
        }


        public static string Decompressor(string prefix, int pixels, int startnum, int endnum, int pixelsPerBlock, int mask) {
            /**
             *  Temporary Workarround. Needs to change directly on Demuxer
             */

            string outputFile = String.Format("{0}_decomp{1}.lrit", prefix, startnum);
            string ifile;
            FileStream f = null;
            try {
                ifile = string.Format("{0}{1}.lrit", prefix, startnum);
                byte[] input = File.ReadAllBytes(ifile);
                byte[] outputData = new byte[pixels];

                try {
                    File.Delete(ifile);
                } catch (IOException e) {
                    UIConsole.GlobalConsole.Warn(String.Format("Cannot delete file {0}: {1}", ifile, e));
                }

                f = File.OpenWrite(outputFile);
                startnum++;
                // First file only contains header
                f.Write(input, 0, input.Length);

                int overflowCaseLast = -1;

                // Check for overflow in file number
                if (endnum < startnum) {
                    overflowCaseLast = endnum;
                    endnum = 16383;
                }

                for (int i = startnum; i <= endnum; i++) {
                    ifile = string.Format("{0}{1}.lrit", prefix, i);
                    for (int z = 0; z < outputData.Length; z++) {
                        outputData[z] = 0x00;
                    }

                    try {
                        input = File.ReadAllBytes(ifile);
                        File.Delete(ifile);
                        AEC.LritRiceDecompress(ref outputData, input, 8, pixelsPerBlock, pixels, mask);
                    } catch (FileNotFoundException) {
                        UIConsole.GlobalConsole.Error(String.Format("Decompressor cannot find file {0}", ifile));
                    } catch (AECException e) {
                        Console.WriteLine("AEC Decompress problem decompressing file {0}: {1}", ifile, e.status.ToString());
                        Console.WriteLine("AEC Params: {0} - {1} - {2} - {3}", 8, pixelsPerBlock, pixels, mask);
                    } catch (IOException e) {
                        Console.WriteLine(e);
                    }

                    f.Write(outputData, 0, outputData.Length);
                }

                if (overflowCaseLast != -1) {
                    for (int i = 0; i < overflowCaseLast; i++) {
                        ifile = string.Format("{0}{1}.lrit", prefix, i);
                        for (int z = 0; z < outputData.Length; z++) {
                            outputData[z] = 0x00;
                        }
                        try {
                            input = File.ReadAllBytes(ifile);
                            File.Delete(ifile);
                            AEC.LritRiceDecompress(ref outputData, input, 8, pixelsPerBlock, pixels, mask);
                        } catch (FileNotFoundException) {
                            UIConsole.GlobalConsole.Error(String.Format("Decompressor cannot find file {0}", ifile));
                        } catch (AECException) {
                            Console.WriteLine("AEC Decompress problem decompressing file {0}", ifile);
                        } catch (IOException e) {
                            Console.WriteLine("Error deleting file {0}: {1}", ifile, e);
                        }

                        f.Write(outputData, 0, outputData.Length);
                    }
                }

            } catch (Exception e) {
                UIConsole.GlobalConsole.Error(string.Format("There was an error decompressing data: {0}", e));
            }

            try {
                if (f != null) {
                    f.Close();
                }
            } catch (Exception) {

            }

            return outputFile;
        }
    }
}


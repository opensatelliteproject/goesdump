using System;
using OpenSatelliteProject.PacketData;
using System.Collections.Generic;
using System.IO;
using OpenSatelliteProject.PacketData.Enums;
using System.Linq;

namespace OpenSatelliteProject {
    public delegate void FileHandlerFunction(string filename, XRITHeader fileHeader);

    public static class FileHandler {

        static Dictionary<int, FileHandlerFunction> byCompressionTypeHandler;
        static Dictionary<int, FileHandlerFunction> byProductIdHandler;

        public static bool SkipDCS { get; set; }
        public static bool SkipEMWIN { get; set; }
        public static bool SkipWeatherData { get; set; }
        public static string TemporaryFileFolder { get; set; }
        public static string FinalFileFolder { get; set; }
        public static string ArchiveFolder { get; set; }
        public static int DaysToArchive { get; set; }

        static FileHandler() {
            byProductIdHandler = new Dictionary<int, FileHandlerFunction>();
            byCompressionTypeHandler = new Dictionary<int, FileHandlerFunction>();
            SkipDCS = false;
            SkipEMWIN = false;
            SkipWeatherData = false;

            string baseFolder = Directory.GetCurrentDirectory();

            TemporaryFileFolder = Path.Combine(baseFolder, "tmp");
            FinalFileFolder = Path.Combine(baseFolder, "output");
            ArchiveFolder = Path.Combine (baseFolder, "archive");
            DaysToArchive = 1;
        }

        /// <summary>
        /// Checks if any files in folder has reached the DaysToArchive days and archive them.
        /// </summary>
        /// <param name="folder">Folder.</param>
        /// <param name="groupName">Group name.</param>
        public static void ArchieveHandler(string folder, string groupName) {
            List<string> files = Directory.GetFiles (folder).ToList ();
            List<string> filesToArchive = new List<string> ();
            Dictionary<string, List<string>> dateMap = new Dictionary<string, List<string>> ();
            files.ForEach (f => {
                DateTime dt = File.GetCreationTime(f);
                var delta = DateTime.Now.Date - dt.Date;
                if (delta.Days >= DaysToArchive) {
                    UIConsole.Debug($"{f} is to archive: {delta.Days}");
                    filesToArchive.Add(f);
                    string dir = Path.Combine(ArchiveFolder);
                    dir = Path.Combine(dir, dt.Year.ToString());
                    dir = Path.Combine(dir, dt.Month.ToString());
                    dir = Path.Combine(dir, dt.Day.ToString());
                    dir = Path.Combine(dir, groupName);
                    if (!dateMap.ContainsKey(dir)) {
                        dateMap.Add(dir, new List<string>());
                    }
                    dateMap[dir].Add(f);
                }
            });

            if (filesToArchive.Count > 0) {
                UIConsole.Debug ($"{filesToArchive.Count} files to archive");
                dateMap.Keys.ToList ().ForEach ((d) => {
                    var fs = dateMap[d];
                    try {
                        if (!Directory.Exists(d)) {
                            Directory.CreateDirectory(d);
                        }
                    } catch (IOException e) {
                        UIConsole.Error($"Error creating Archive Directory ({d}): {e.Message}");
                        return;
                    }
                    fs.ForEach((f) => {
                        try {
                            File.Move(f, Path.Combine(d, Path.GetFileName(f)));
                        } catch (IOException e) {
                            UIConsole.Error($"Error archiving file {f}: {e.Message}");
                        }
                    });
                });
            }
        }

        public static void AttachByCompressionHandler(int compressionType, FileHandlerFunction handler) {
            byCompressionTypeHandler[compressionType] = handler;
        }

        public static void AttachByProductIdHandler(int productId, FileHandlerFunction handler) {
            byProductIdHandler[productId] = handler;
        }

        public static void HandleFile(string filename, XRITHeader fileHeader, DemuxManager dm = null) {
            if (dm != null) {
                dm.incProductCount(fileHeader.Product.ID);
            }

            if (byCompressionTypeHandler.ContainsKey((int)fileHeader.Compression)) {
                byCompressionTypeHandler[(int)fileHeader.Compression](filename, fileHeader);
            } else if (byProductIdHandler.ContainsKey(fileHeader.Product.ID)) {
                byProductIdHandler[fileHeader.Product.ID](filename, fileHeader);
            } else {
                DefaultHandler(filename, fileHeader);
            }
        }

        public static void DefaultHandler(string filename, XRITHeader fileHeader) {
            string dir = Path.GetDirectoryName(filename);
            string ofilename = fileHeader.Filename ?? Path.GetFileName (filename);

            // Workarround for multi-segment HRIT
            if (fileHeader.Product.ID == (int)NOAAProductID.GOES16_ABI) {
                if (fileHeader.SegmentIdentificationHeader != null && fileHeader.SegmentIdentificationHeader.MaxSegments > 1) {
                    string baseName = Path.GetFileNameWithoutExtension (ofilename);
                    string ext = Path.GetExtension (ofilename);
                    string fileH = fileHeader.SegmentIdentificationHeader.Sequence.ToString ("D2");
                    string imageId = fileHeader.SegmentIdentificationHeader.ImageID.ToString();
                    ofilename = $"{baseName}-img{imageId}-seg{fileH}{ext}";
                }
            }

            string f = PacketManager.FixFileFolder(dir, ofilename, fileHeader.Product, fileHeader.SubProduct);

            if (
                (fileHeader.Product.ID == (int)NOAAProductID.DCS && SkipDCS) || 
                (fileHeader.Product.ID == (int)NOAAProductID.EMWIN && SkipEMWIN) || 
                (fileHeader.Product.ID == (int)NOAAProductID.HRIT_EMWIN && SkipEMWIN) ||
                (fileHeader.Product.ID == (int)NOAAProductID.WEATHER_DATA && SkipWeatherData)
            ) {
                try {
                    File.Delete(filename);
                } catch (IOException e) {
                    UIConsole.Error(String.Format("Error deleting file {0}: {1}", Path.GetFileName(filename), e));
                }
                return;
            }

            if (File.Exists(f)) {
                string timestamp = DateTime.Now.ToString("yyyyMMddHHmmssffff");
                string ext = Path.GetExtension(f);
                string append = String.Format("--dup-{0}{1}", timestamp, ext);
                f = f.Replace(String.Format("{0}", ext), append);
            }

            UIConsole.Log ($"New {fileHeader.ToNameString()}");

            /*if (fileHeader.SubProduct.Name != "Unknown") {
                UIConsole.Log($"New {fileHeader.Product.Name} - {fileHeader.SubProduct.Name}");
            } else {
                UIConsole.Log($"New {fileHeader.Product.Name}");
            }*/

            EventMaster.Post (EventTypes.NewFileEvent, new NewFileReceivedEventData {
                Name = Path.GetFileName(ofilename),
                Path = ofilename,
                Metadata = {
                    { "product", fileHeader.Product.Name },
                    { "subProduct", fileHeader.SubProduct.Name },
                    { "productId", fileHeader.Product.ID.ToString() },
                    { "subProductId", fileHeader.SubProduct.ID.ToString() }
                }
            });

            try {
                File.Move(filename, f);
            } catch (IOException e) {
                UIConsole.Error(String.Format("Error moving file {0} to {1}: {2}", filename, f, e));
            }
        }
        
    }
}


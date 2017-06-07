using System;
using System.Threading;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using OpenSatelliteProject.Tools;

namespace OpenSatelliteProject {
    public class ImageManager {

        /// <summary>
        /// Erase files after creating false color images.
        /// </summary>
        /// <value><c>true</c> if erase files; otherwise, <c>false</c>.</value>
        public static bool EraseFiles { get; set; }

        /// <summary>
        /// Max Retry Count for generating false colors. After that it will be considered as done and ignored.
        /// </summary>
        /// <value>The max retry count.</value>
        public static int MaxRetryCount { get; set; }

        public static bool GenerateFalseColor { get; set; }
        public static bool GenerateVisible { get; set; }
        public static bool GenerateInfrared { get; set; }
        public static bool GenerateWaterVapour { get; set; }
        public static bool GenerateOtherImages { get; set; }
        public static bool UseNOAAFileFormat { get; set; }

        private Thread imageThread;
        private bool running;
        private Organizer organizer;
        private string folder;

        static ImageManager() {
            EraseFiles = false;
            MaxRetryCount = 3;
            GenerateFalseColor = true;
            GenerateVisible = true;
            GenerateInfrared = true;
            GenerateWaterVapour = true;
            GenerateOtherImages = true;
            UseNOAAFileFormat = false;
        }

        private static string GenFilename(string satelliteName, string regionName, string imageName, int timestamp, string origName = null) {
            if (UseNOAAFileFormat) {
                //gos15chnIR04rgnFDseg001res04dat130 18 06 19 190.lrit
                if (origName != null) {
                    origName = Path.GetFileName(origName);
                } else {
                    origName = "";
                }

                var dt = LLTools.UnixTimeStampToDateTime(timestamp);
                var year = dt.Year.ToString ("0000");
                var month = dt.Month.ToString ("00");
                var day = dt.Day.ToString ("00");
                var doy = dt.DayOfYear.ToString("000");
                var hour = dt.Hour.ToString("00");
                var minute = dt.Minute.ToString("00");
                var second = dt.Second.ToString("00");

                if (origName.Length == 48) {
                    return $"{origName.Substring(0, 31)}{doy}{hour}{minute}{second}000.png";
                } else if (origName.StartsWith("IMG_DK")) {
                    // Himawari
                    // IMG_DK01IR3_201705190350_002
                    return $"{origName.Substring(0, 12)}{year}{month}{day}{hour}{minute}_000.png";
                } else {
                    // Return default
                    return string.Format("{0}-{1}-{2}-{3}.png", satelliteName, regionName, imageName, timestamp);
                }
            } else {
                return string.Format("{0}-{1}-{2}-{3}.png", satelliteName, regionName, imageName, timestamp);
            }
        }

        public ImageManager(string folder) {
            this.organizer = new Organizer(folder);
            this.folder = folder;
            this.imageThread = null;

            if (!Directory.Exists(folder)) {
                Directory.CreateDirectory(folder);
            }

            UIConsole.GlobalConsole.Debug(string.Format("Creating ImageManager on folder {0}", folder));
        }

        public void Start() {
            if (!running) {
                running = true;
                imageThread = new Thread(new ThreadStart(ThreadLoop));
                imageThread.IsBackground = true;
                imageThread.Start();
            }
        }

        public void Stop() {
            if (running) {
                running = false;
                if (imageThread != null) {
                    imageThread.Join();
                }
                imageThread = null;
            }
        }

        private void TryEraseGroupDataFiles(int idx, GroupData mData) {
            // Water Vapour and Other files can be erased without FalseColor
            // Erase Water Vapour LRIT
            if ((GenerateWaterVapour && mData.IsWaterVapourProcessed || !GenerateWaterVapour)) {
                mData.WaterVapour.Segments.Select(x => x.Value).ToList().ForEach(f => {
                    try {
                        File.Delete(f);
                    } catch (IOException e) {
                        UIConsole.GlobalConsole.Error(string.Format("Error erasing file {0}: {1}", f, e));
                    }
                });
            }
            // Erase Other Images LRIT
            if ((GenerateOtherImages && mData.IsOtherDataProcessed || !GenerateOtherImages)) {
                mData.OtherData.Select(x => x.Value).ToList().ForEach(k => {
                    k.Segments.Select(x => x.Value).ToList().ForEach(f => {
                        try {
                            File.Delete(f);
                        } catch (IOException e) {
                            UIConsole.GlobalConsole.Error(string.Format("Error erasing file {0}: {1}", f, e));
                        }
                    });
                });
            }

            // Do not erase files until false color is processed if required.
            if (GenerateFalseColor && !mData.IsFalseColorProcessed) {
                return;
            }

            // Erase Infrared LRIT
            if ((GenerateInfrared && mData.IsInfraredProcessed || !GenerateInfrared)) {
                mData.Infrared.Segments.Select(x => x.Value).ToList().ForEach(f => {
                    try {
                        File.Delete(f);
                    } catch (IOException e) {
                        UIConsole.GlobalConsole.Error(string.Format("Error erasing file {0}: {1}", f, e));
                    }
                });
            }
            // Erase Visible LRIT
            if ((GenerateVisible && mData.IsVisibleProcessed || !GenerateVisible)) {
                mData.Visible.Segments.Select(x => x.Value).ToList().ForEach(f => {
                    try {
                        File.Delete(f);
                    } catch (IOException e) {
                        UIConsole.GlobalConsole.Error(string.Format("Error erasing file {0}: {1}", f, e));
                    }
                });
            }

            if (
                (GenerateFalseColor && mData.IsFalseColorProcessed || !GenerateFalseColor) && 
                (GenerateVisible && mData.IsVisibleProcessed || !GenerateVisible) && 
                (GenerateInfrared && mData.IsInfraredProcessed || !GenerateInfrared) && 
                (GenerateWaterVapour && mData.IsWaterVapourProcessed || !GenerateWaterVapour) && 
                (GenerateOtherImages && mData.IsOtherDataProcessed || !GenerateOtherImages)
            ) {
                UIConsole.GlobalConsole.Debug($"Group Data {idx} is done. Removing it from Organizer.");
                organizer.RemoveGroupData(idx);   
            }
        }

        private void ThreadLoop() {
            while (running) {
                organizer.Update();
                var data = organizer.GroupData;
                var clist = data.ToList();
                foreach (var z in clist) {
                    var mData = z.Value;
                    if (!running) {
                        break;
                    }
                    string ImageName = string.Format("{0}-{1}-{2}", z.Key, mData.SatelliteName, mData.RegionName);
                    if (!mData.IsProcessed) {
                        try {
                            if (ImageManager.GenerateVisible && mData.Visible.IsComplete && mData.Visible.MaxSegments != 0 && !mData.IsVisibleProcessed) {
                                string ofilename = Path.Combine(folder, GenFilename(mData.SatelliteName, mData.RegionName, "VIS", z.Key, mData.Visible.Segments[mData.Visible.FirstSegment]));
                                if (File.Exists(ofilename)) {
                                    UIConsole.GlobalConsole.Debug(string.Format("Skipping generating Visible for {0}. Image already exists.", Path.GetFileName(ofilename)));
                                    mData.IsVisibleProcessed = true;
                                } else {
                                    UIConsole.GlobalConsole.Debug(string.Format("Starting Generation of Visible for {0}.", Path.GetFileName(ofilename)));
                                    var bmp = ImageTools.GenerateFullImage(mData.Visible, mData.CropImage);
                                    bmp.Save(ofilename, ImageFormat.Png);
                                    bmp.Dispose();
                                    UIConsole.GlobalConsole.Log(string.Format("New Visible Image: {0}", Path.GetFileName(ofilename)));
                                }
                                mData.IsVisibleProcessed = true;
                                mData.Visible.OK = true;
                            }

                            if (ImageManager.GenerateInfrared && mData.Infrared.IsComplete && mData.Infrared.MaxSegments != 0 && !mData.IsInfraredProcessed) {
                                string ofilename = Path.Combine(folder, GenFilename(mData.SatelliteName, mData.RegionName, "IR", z.Key, mData.Infrared.Segments[mData.Infrared.FirstSegment]));
                                if (File.Exists(ofilename)) {
                                    UIConsole.GlobalConsole.Debug(string.Format("Skipping generating Infrared for {0}. Image already exists.", Path.GetFileName(ofilename)));
                                } else {
                                    UIConsole.GlobalConsole.Debug(string.Format("Starting Generation of Infrared for {0}.", Path.GetFileName(ofilename)));
                                    var bmp = ImageTools.GenerateFullImage(mData.Infrared, mData.CropImage);
                                    bmp.Save(ofilename, ImageFormat.Png);
                                    bmp.Dispose();
                                    UIConsole.GlobalConsole.Log(string.Format("New Infrared Image: {0}", Path.GetFileName(ofilename)));
                                }
                                mData.IsInfraredProcessed = true;
                                mData.Infrared.OK = true;
                            }

                            if (ImageManager.GenerateWaterVapour && mData.WaterVapour.IsComplete && mData.WaterVapour.MaxSegments != 0 && !mData.IsWaterVapourProcessed) {
                                string ofilename = Path.Combine(folder, GenFilename(mData.SatelliteName, mData.RegionName, "WV", z.Key, mData.WaterVapour.Segments[mData.WaterVapour.FirstSegment]));
                                if (File.Exists(ofilename)) {
                                    UIConsole.GlobalConsole.Debug(string.Format("Skipping generating Water Vapour for {0}. Image already exists.", Path.GetFileName(ofilename)));
                                } else {
                                    UIConsole.GlobalConsole.Debug(string.Format("Starting Generation of Water Vapour for {0}.", Path.GetFileName(ofilename)));
                                    var bmp = ImageTools.GenerateFullImage(mData.WaterVapour, mData.CropImage);
                                    bmp.Save(ofilename, ImageFormat.Png);
                                    bmp.Dispose();
                                    UIConsole.GlobalConsole.Log(string.Format("New Water Vapour Image: {0}", Path.GetFileName(ofilename)));
                                }
                                mData.IsWaterVapourProcessed = true;
                                mData.WaterVapour.OK = true;
                            }
                            if (GenerateFalseColor && !mData.IsFalseColorProcessed  && ImageTools.CanGenerateFalseColor(mData)) {
                                string filename = GenFilename(
                                    mData.SatelliteName, 
                                    mData.RegionName, 
                                    "FSCLR", 
                                    z.Key, 
                                    mData.Visible.Segments[mData.Visible.FirstSegment]
                                    .Replace("VS", "FC")
                                    .Replace("VIS", "FC")
                                );
                                filename = Path.Combine(folder, filename);

                                if (File.Exists(filename)) {
                                    UIConsole.GlobalConsole.Debug(string.Format("Skipping generating FLSCLR for {0}. Image already exists.", Path.GetFileName(filename)));
                                } else {
                                    UIConsole.GlobalConsole.Debug(string.Format("Starting Generation of FSLCR for {0}.", Path.GetFileName(filename)));
                                    var bmp = ImageTools.GenerateFalseColor(mData);

                                    bmp.Save(filename, ImageFormat.Png);
                                    bmp.Dispose();
                                    UIConsole.GlobalConsole.Log(string.Format("New False Colour Image: {0}", Path.GetFileName(filename)));
                                }
                                mData.IsFalseColorProcessed = true;
                            }
                            if (GenerateOtherImages && !mData.IsOtherDataProcessed && mData.OtherData.Count > 0) {
                                bool Processed = true;
                                mData.OtherData.Keys.ToList().ForEach(k => {
                                    var gd = mData.OtherData[k];
                                    if (gd.IsComplete && gd.MaxSegments != 0 && !gd.OK) {
                                        string ofilename = GenFilename(mData.SatelliteName, mData.RegionName, gd.Code, gd.Timestamp, gd.Segments[0]);
                                        ofilename = Path.Combine(folder, ofilename);

                                        if (File.Exists(ofilename)) {
                                            UIConsole.GlobalConsole.Debug(string.Format("Skipping generating {0}. Image already exists.", Path.GetFileName(ofilename)));
                                        } else {
                                            UIConsole.GlobalConsole.Debug(string.Format("Starting Generation of {0}.", Path.GetFileName(ofilename)));
                                            var bmp = ImageTools.GenerateFullImage(gd, false);
                                            bmp.Save(ofilename, ImageFormat.Png);
                                            bmp.Dispose();
                                            UIConsole.GlobalConsole.Log(string.Format("New Image: {0}", Path.GetFileName(ofilename)));
                                        }
                                        gd.OK = true;
                                    } else {
                                        Processed = false;
                                    }
                                });
                                mData.IsOtherDataProcessed = Processed;
                            }

                            mData.IsProcessed = 
                                (!GenerateFalseColor    || ( GenerateFalseColor && mData.IsFalseColorProcessed) ) &&
                                (!GenerateVisible       || ( GenerateVisible && mData.IsVisibleProcessed) ) &&
                                (!GenerateInfrared      || ( GenerateInfrared && mData.IsInfraredProcessed) ) &&
                                (!GenerateWaterVapour   || ( GenerateWaterVapour && mData.IsWaterVapourProcessed) ) &&
                                (!GenerateOtherImages   || ( GenerateOtherImages && mData.IsOtherDataProcessed) );

                            if (mData.Timeout) {
                                // Timeout completing, so let's erase the files.
                                mData.ForceComplete();
                            }

                            if (EraseFiles) {
                                TryEraseGroupDataFiles(z.Key, mData);
                            }
                        } catch (SystemException e) {
                            UIConsole.GlobalConsole.Error(string.Format("Error processing image (SysExcpt) {0}: {1}", ImageName, e));                            
                            mData.RetryCount++;
                            if (mData.RetryCount == ImageManager.MaxRetryCount) {
                                mData.IsProcessed = true;
                            }
                        } catch (Exception e) {
                            UIConsole.GlobalConsole.Error(string.Format("Error processing image {0}: {1}", ImageName, e));
                            mData.RetryCount++;
                            if (mData.RetryCount == ImageManager.MaxRetryCount) {
                                mData.IsProcessed = true;
                            }
                        } 
                    }
                }

                Thread.Sleep(200);
            }
        }
    }
}


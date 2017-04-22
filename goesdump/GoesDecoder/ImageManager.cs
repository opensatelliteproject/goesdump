using System;
using System.Threading;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

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
            running = true;
            imageThread = new Thread(new ThreadStart(ThreadLoop));
            imageThread.IsBackground = true;
            imageThread.Start();
        }

        public void Stop() {
            running = false;
            if (imageThread != null) {
                imageThread.Join();
            }
        }

        private void TryEraseGroupDataFiles(GroupData mData) {
            // Water Vapour and Other files can be erased without FalseColor
            // Erase Water Vapour LRIT
            if (mData.IsWaterVapourProcessed) {
                mData.WaterVapour.Segments.Select(x => x.Value).ToList().ForEach(f => {
                    try {
                        File.Delete(f);
                    } catch (IOException e) {
                        UIConsole.GlobalConsole.Error(string.Format("Error erasing file {0}: {1}", f, e));
                    }
                });
            }
            // Erase Other Images LRIT
            if (mData.IsOtherDataProcessed) {
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
            if (mData.IsInfraredProcessed) {
                mData.Infrared.Segments.Select(x => x.Value).ToList().ForEach(f => {
                    try {
                        File.Delete(f);
                    } catch (IOException e) {
                        UIConsole.GlobalConsole.Error(string.Format("Error erasing file {0}: {1}", f, e));
                    }
                });
            }
            // Erase Visible LRIT
            if (mData.IsVisibleProcessed) {
                mData.Visible.Segments.Select(x => x.Value).ToList().ForEach(f => {
                    try {
                        File.Delete(f);
                    } catch (IOException e) {
                        UIConsole.GlobalConsole.Error(string.Format("Error erasing file {0}: {1}", f, e));
                    }
                });
            }
        }

        private void ThreadLoop() {
            while (running) {
                organizer.Update();
                var data = organizer.GroupData;

                foreach (var z in data) {
                    var mData = z.Value;
                    if (!running) {
                        break;
                    }
                    string ImageName = string.Format("{0}-{1}-{2}", z.Key, mData.SatelliteName, mData.RegionName);
                    if (!mData.IsProcessed) {
                        try {
                            if (ImageManager.GenerateVisible && mData.Visible.IsComplete && mData.Visible.MaxSegments != 0 && !mData.IsVisibleProcessed) {
                                string ofilename = Path.Combine(folder, string.Format("{0}-{1}-{2}-{3}.png", mData.SatelliteName, mData.RegionName, "VIS", z.Key));
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
                                string ofilename = Path.Combine(folder, string.Format("{0}-{1}-{2}-{3}.png", mData.SatelliteName, mData.RegionName, "IR", z.Key));
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
                                string ofilename = Path.Combine(folder, string.Format("{0}-{1}-{2}-{3}.png", mData.SatelliteName, mData.RegionName, "WV", z.Key));
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
                                string filename = string.Format("{0}-{1}-{2}-{3}.png", z.Key, mData.SatelliteName, mData.RegionName, "FLSCLR");
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
                                        string ofilename = string.Format("{0}-{1}-{2}.png", z.Key, mData.SatelliteName, k);
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
                                TryEraseGroupDataFiles(mData);
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


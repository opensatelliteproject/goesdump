using System;
using System.Threading;
using System.Drawing.Imaging;
using System.IO;

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

        private void EraseGroupDataFiles(GroupData mData) {
            // Erase Infrared LRIT
            foreach (var f in mData.Infrared.Segments) {
                try {
                    File.Delete(f.Value);
                } catch (IOException e) {
                    UIConsole.GlobalConsole.Error(string.Format("Error erasing file {0}: {1}", f.Value, e));
                }
            }
            // Erase Visible LRIT
            foreach (var f in mData.Visible.Segments) {
                try {
                    File.Delete(f.Value);
                } catch (IOException e) {
                    UIConsole.GlobalConsole.Error(string.Format("Error erasing file {0}: {1}", f.Value, e));
                }
            }
            // Erase Water Vapour LRIT
            foreach (var f in mData.WaterVapour.Segments) {
                try {
                    File.Delete(f.Value);
                } catch (IOException e) {
                    UIConsole.GlobalConsole.Error(string.Format("Error erasing file {0}: {1}", f.Value, e));
                }
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

                    try {
                        if (ImageManager.GenerateVisible && mData.Visible.IsComplete && mData.Visible.MaxSegments != 0 && !mData.IsVisibleProcessed) {
                            string ofilename = Path.Combine(folder, string.Format("{0}-{1}-{2}-{3}.png", mData.SatelliteName, mData.RegionName, "VIS", z.Key));
                            if (File.Exists(ofilename)) {
                                UIConsole.GlobalConsole.Debug(string.Format("Skipping generating Visible for {0}. Image already exists.", Path.GetFileName(ofilename)));
                                mData.IsVisibleProcessed = true;
                            } else {
                                UIConsole.GlobalConsole.Debug(string.Format("Starting Generation of Visible for {0}.", Path.GetFileName(ofilename)));
                                var bmp = ImageTools.GenerateFullImage(mData.Visible);
                                bmp.Save(ofilename, ImageFormat.Png);
                                bmp.Dispose();
                                UIConsole.GlobalConsole.Log(string.Format("New Visible Image: {0}", Path.GetFileName(ofilename)));
                            }
                            mData.IsVisibleProcessed = true;
                        }

                        if (ImageManager.GenerateInfrared && mData.Infrared.IsComplete && mData.Infrared.MaxSegments != 0 && !mData.IsInfraredProcessed) {
                            string ofilename = Path.Combine(folder, string.Format("{0}-{1}-{2}-{3}.png", mData.SatelliteName, mData.RegionName, "IR", z.Key));
                            if (File.Exists(ofilename)) {
                                UIConsole.GlobalConsole.Debug(string.Format("Skipping generating Visible for {0}. Image already exists.", Path.GetFileName(ofilename)));
                            } else {
                                UIConsole.GlobalConsole.Debug(string.Format("Starting Generation of Infrared for {0}.", Path.GetFileName(ofilename)));
                                var bmp = ImageTools.GenerateFullImage(mData.Infrared);
                                bmp.Save(ofilename, ImageFormat.Png);
                                bmp.Dispose();
                                UIConsole.GlobalConsole.Log(string.Format("New Infrared Image: {0}", Path.GetFileName(ofilename)));
                            }
                            mData.IsInfraredProcessed = true;
                        }

                        if (ImageManager.GenerateWaterVapour && mData.WaterVapour.IsComplete && mData.WaterVapour.MaxSegments != 0 && !mData.IsWaterVapourProcessed) {
                            string ofilename = Path.Combine(folder, string.Format("{0}-{1}-{2}-{3}.png", mData.SatelliteName, mData.RegionName, "WV", z.Key));
                            if (File.Exists(ofilename)) {
                                UIConsole.GlobalConsole.Debug(string.Format("Skipping generating Visible for {0}. Image already exists.", Path.GetFileName(ofilename)));
                            } else {
                                UIConsole.GlobalConsole.Debug(string.Format("Starting Generation of Water Vapour for {0}.", Path.GetFileName(ofilename)));
                                var bmp = ImageTools.GenerateFullImage(mData.WaterVapour);
                                bmp.Save(ofilename, ImageFormat.Png);
                                bmp.Dispose();
                                UIConsole.GlobalConsole.Log(string.Format("New Water Vapour Image: {0}", Path.GetFileName(ofilename)));
                            }
                            mData.IsWaterVapourProcessed = true;
                        }
                        if (GenerateFalseColor && ImageTools.CanGenerateFalseColor(mData)) {
                            string filename = string.Format("{0}-{1}-{2}-{3}.png", z.Key, mData.SatelliteName, mData.RegionName, "FLSCLR");
                            filename = Path.Combine(folder, filename);

                            if (File.Exists(filename)) {
                                UIConsole.GlobalConsole.Debug(string.Format("Skipping generating FLSCLR for {0}. Image already exists.", Path.GetFileName(filename)));
                                if (EraseFiles) {
                                    EraseGroupDataFiles(mData);
                                }
                            } else {
                                UIConsole.GlobalConsole.Debug(string.Format("Starting Generation of FSLCR for {0}.", Path.GetFileName(filename)));
                                var bmp = ImageTools.GenerateFalseColor(mData);

                                bmp.Save(filename, ImageFormat.Png);
                                bmp.Dispose();
                                UIConsole.GlobalConsole.Log(string.Format("New False Colour Image: {0}", Path.GetFileName(filename)));
                            }
                            mData.IsFalseColorProcessed = true;
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

                    if (EraseFiles) {
                        EraseGroupDataFiles(mData);
                    }
                }

                Thread.Sleep(200);
            }
        }
    }
}


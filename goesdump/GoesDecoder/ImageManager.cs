using System;
using System.Threading;
using System.Drawing.Imaging;
using System.IO;

namespace OpenSatelliteProject {
    public class ImageManager {
        private Thread imageThread;
        private bool running;
        private Organizer organizer;
        private string folder;

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

        private void ThreadLoop() {
            while (running) {
                organizer.Update();
                var data = organizer.GroupData;

                foreach (var z in data) {
                    var mData = z.Value;
                    if (!mData.IsProcessed) {
                        string filename = string.Format("{0}-{1}-{2}-{3}.jpg", z.Key, mData.SatelliteName, mData.RegionName, "FLSCLR");
                        filename = Path.Combine(folder, filename);

                        if (File.Exists(filename)) {
                            UIConsole.GlobalConsole.Debug(string.Format("Skipping generating FLSCLR for {0}. Image already exists.", Path.GetFileName(filename)));
                            mData.IsProcessed = true;
                            continue;
                        }

                        if (ImageTools.CanGenerateFalseColor(mData)) {
                            UIConsole.GlobalConsole.Debug(string.Format("Starting Generation of FSLCR for {0}.", Path.GetFileName(filename)));
                            var bmp = ImageTools.GenerateFalseColor(mData);

                            bmp.Save(filename, ImageFormat.Jpeg);
                            bmp.Dispose();
                            UIConsole.GlobalConsole.Log(string.Format("New False Colour Image: {0}", Path.GetFileName(filename)));
                            mData.IsProcessed = true;
                        } else {
                            /*
                            if (mData.Visible.IsComplete && mData.Visible.MaxSegments != 0 && !mData.IsVisibleProcessed) {
                                bmp = ImageTools.GenerateFullImage(mData.Visible);
                                bmp.Save(string.Format("{0}-{1}-{2}-{3}.jpg", mData.SatelliteName, mData.RegionName, "VIS", z.Key), ImageFormat.Jpeg);
                                bmp.Dispose();
                                mData.IsVisibleProcessed = true;
                            }
                            if (mData.Infrared.IsComplete && mData.Infrared.MaxSegments != 0 && !mData.IsInfraredProcessed) {
                                bmp = ImageTools.GenerateFullImage(mData.Infrared);
                                bmp.Save(string.Format("{0}-{1}-{2}-{3}.jpg", mData.SatelliteName, mData.RegionName, "IR", z.Key), ImageFormat.Jpeg);
                                bmp.Dispose();
                                mData.IsInfraredProcessed = true;
                            }
                            if (mData.WaterVapour.IsComplete && mData.WaterVapour.MaxSegments != 0 && !mData.IsWaterVapourProcessed) {
                                bmp = ImageTools.GenerateFullImage(mData.WaterVapour);
                                bmp.Save(string.Format("{0}-{1}-{2}-{3}.jpg", mData.SatelliteName, mData.RegionName, "WV", z.Key), ImageFormat.Jpeg);
                                bmp.Dispose();
                                mData.IsWaterVapourProcessed = true;
                            }
                            Console.WriteLine("Not all segments available!");
                            */
                        }
                    }
                }

                Thread.Sleep(200);
            }
        }
    }
}


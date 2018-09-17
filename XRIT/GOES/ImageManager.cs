using System;
using System.Threading;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using OpenSatelliteProject.Tools;
using System.Collections.Generic;
using OpenSatelliteProject.Geo;
using System.Drawing;

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

        /// <summary>
        /// Generate PNG files for False Color
        /// </summary>
        /// <value><c>true</c> if generate false color; otherwise, <c>false</c>.</value>
        public static bool GenerateFalseColor { get; set; }

        /// <summary>
        /// Generate PNG files for Visible Channel
        /// </summary>
        /// <value><c>true</c> if generate visible; otherwise, <c>false</c>.</value>
        public static bool GenerateVisible { get; set; }

        /// <summary>
        /// Generate PNG files for Infrared Channel
        /// </summary>
        /// <value><c>true</c> if generate infrared; otherwise, <c>false</c>.</value>
        public static bool GenerateInfrared { get; set; }

        /// <summary>
        /// Generate PNG files for Water Vapour Channel
        /// </summary>
        /// <value><c>true</c> if generate water vapour; otherwise, <c>false</c>.</value>
        public static bool GenerateWaterVapour { get; set; }

        /// <summary>
        /// Generate PNG files for any image that is not a False Color, Visible, Infrared or Water Vapour
        /// </summary>
        /// <value><c>true</c> if generate other images; otherwise, <c>false</c>.</value>
        public static bool GenerateOtherImages { get; set; }

        /// <summary>
        /// Generate Lat / Lon Overlays
        /// </summary>
        /// <value><c>true</c> if generate lat lon overlays; otherwise, <c>false</c>.</value>
        public static bool GenerateLatLonOverlays { get; set; }

        /// <summary>
        /// Generate Map Overlays 
        /// </summary>
        /// <value><c>true</c> if generate map overlays; otherwise, <c>false</c>.</value>
        public static bool GenerateMapOverlays { get; set; }

        /// <summary>
        /// Generate Informative Labels on Image
        /// </summary>
        /// <value><c>true</c> if generate labels; otherwise, <c>false</c>.</value>
        public static bool GenerateLabels { get; set; }

        /// <summary>
        /// Generate the label with center lat/lon
        /// </summary>
        /// <value><c>true</c> if generate lat lon label; otherwise, <c>false</c>.</value>
        public static bool GenerateLatLonLabel { get; set; }

        /// <summary>
        /// If a PNG without the overlays should also be saved
        /// </summary>
        /// <value><c>true</c> if should be saved; otherwise, <c>false</c>.</value>
        public static bool SaveNonOverlay { get; set; }

        /// <summary>
        /// Use default noaa file name format.
        /// </summary>
        /// <value><c>true</c> if use NOAA file format; otherwise, <c>false</c>.</value>
        public static bool UseNOAAFileFormat { get; set; }

        /// <summary>
        /// The LatLon Overlay Pen Thickness
        /// </summary>
        public static int LatLonOverlayPenThickness;

        /// <summary>
        /// The color of the lat lon overlay pen.
        /// </summary>
        public static Color LatLonOverlayPenColor;

        /// <summary>
        /// The Map Overlay Pen Thickness
        /// </summary>
        public static int MapOverlayPenThickness;

        /// <summary>
        /// The Map of the lat lon overlay pen.
        /// </summary>
        public static Color MapOverlayPenColor;

        /// <summary>
        /// If file archiving is enabled. Set the interval in FileHandler.DaysToArchive
        /// </summary>
        /// <value><c>true</c> if enable archive; otherwise, <c>false</c>.</value>
        /// <see cref="FileHandler.DaysToArchive"/>
        public static bool EnableArchive { get; set; }

        Thread imageThread;
        bool running;
        readonly Organizer organizer;
        readonly string folder;
        readonly static string defaultShapeFile;
        readonly string archiveGroup;
        MapDrawer mapDrawer;

        static ImageManager() {
            EraseFiles = false;
            MaxRetryCount = 3;
            GenerateFalseColor = true;
            GenerateVisible = true;
            GenerateInfrared = true;
            GenerateWaterVapour = true;
            GenerateOtherImages = true;
            UseNOAAFileFormat = false;
            GenerateLatLonOverlays = false;
            GenerateMapOverlays = false;
            LatLonOverlayPenThickness = 5;
            LatLonOverlayPenColor = Color.Brown;
            MapOverlayPenThickness = 5;
            MapOverlayPenColor = Color.Aqua;
            GenerateLabels = false;
            GenerateLatLonLabel = true;
            SaveNonOverlay = false;
            defaultShapeFile = ShapeFiles.InitShapeFiles ();
            EnableArchive = true;
        }
        #region ImageManager Base Methods
        public ImageManager(string folder) {
            this.organizer = new Organizer(folder);
            this.folder = folder;
            this.imageThread = null;

            if (!Directory.Exists(folder)) {
                Directory.CreateDirectory(folder);
            }

            string aFolder = folder [folder.Count() - 1] == '/' ? folder.Substring (0, folder.Count() - 1) : folder;

            this.archiveGroup = Path.GetFileName (aFolder);
            UIConsole.Debug($"Creating ImageManager on folder {folder}");
        }

        public ImageManager(string folder, string archiveGroup) : this(folder) {
            this.archiveGroup = archiveGroup;
        }

        /// <summary>
        /// Loads default map drawer
        /// </summary>
        public void InitMapDrawer() {
            try {
                UIConsole.Debug($"ImageManager -- Initializing MapDrawer with {defaultShapeFile}");
                mapDrawer = new MapDrawer(defaultShapeFile);
                if (mapDrawer.ShapeFile == null) {
                    mapDrawer = null;
                    throw new ArgumentException("Error loading ShapeFile");
                }
            } catch (Exception e) {
                UIConsole.Error ($"ImageManager -- There was an error initializing MapDrawer: {e}");
            }
        }

        /// <summary>
        /// Loads a mapDrawer with given SHP file
        /// </summary>
        /// <param name="filename">Filename.</param>
        public void InitMapDrawer(string filename) {
            try {
                UIConsole.Debug($"ImageManager -- Initializing MapDrawer with {filename}");
                mapDrawer = new MapDrawer(filename);
                if (mapDrawer.ShapeFile == null) {
                    mapDrawer = null;
                    throw new ArgumentException("Error loading ShapeFile");
                }
            } catch (Exception e) {
                UIConsole.Error ($"ImageManager -- There was an error initializing MapDrawer: {e}");
            }
        }

        public void Start() {
            if (!running) {
                running = true;
                imageThread = new Thread(new ThreadStart(ThreadLoop));
                imageThread.IsBackground = true;
                imageThread.Priority = ThreadPriority.BelowNormal;
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
        #endregion
        #region File Erase
        private void TryEraseGroupDataFiles(int idx, GroupData mData) {
            // Water Vapour and Other files can be erased without FalseColor
            // Erase Water Vapour LRIT
            if ((GenerateWaterVapour && mData.IsWaterVapourProcessed || !GenerateWaterVapour)) {
                mData.WaterVapour.Segments.Select(x => x.Value).ToList().ForEach(f => {
                    try {
                        UIConsole.Debug($"Erasing file {f}");
                        File.Delete(f);
                    } catch (IOException e) {
                        UIConsole.Error($"Error erasing file {Path.GetFileName(f)}: {e}");
                    }
                });
                mData.WaterVapour.Segments.Clear ();
            }
            // Erase Other Images LRIT
            mData.OtherData.Select(x => x.Value).ToList().ForEach(k => {
                if (k.OK) {
                    k.Segments.Select(x => x.Value).ToList().ForEach(f => {
                        try {
                            UIConsole.Debug($"Erasing file {f}");
                            File.Delete(f);
                        } catch (IOException e) {
                            UIConsole.Error($"Error erasing file {Path.GetFileName(f)}: {e}");
                        }
                    });
                    k.Segments.Clear();
                }
            });

            // Do not erase files until false color is processed if required.
            if (GenerateFalseColor && !mData.IsFalseColorProcessed) {
                return;
            }

            // Erase Infrared LRIT
            if ((GenerateInfrared && mData.IsInfraredProcessed || !GenerateInfrared)) {
                mData.Infrared.Segments.Select(x => x.Value).ToList().ForEach(f => {
                    try {
                        UIConsole.Debug($"Erasing file {f}");
                        File.Delete(f);
                    } catch (IOException e) {
                        UIConsole.Error($"Error erasing file {Path.GetFileName(f)}: {e}");
                    }
                });
                mData.Infrared.Segments.Clear ();
            }
            // Erase Visible LRIT
            if ((GenerateVisible && mData.IsVisibleProcessed || !GenerateVisible)) {
                mData.Visible.Segments.Select(x => x.Value).ToList().ForEach(f => {
                    try {
                        UIConsole.Debug($"Erasing file {f}");
                        File.Delete(f);
                    } catch (IOException e) {
                        UIConsole.Error($"Error erasing file {Path.GetFileName(f)}: {e}");
                    }
                });
                mData.Visible.Segments.Clear ();
            }

            if (
                (GenerateFalseColor && mData.IsFalseColorProcessed || !GenerateFalseColor) && 
                (GenerateVisible && mData.IsVisibleProcessed || !GenerateVisible) && 
                (GenerateInfrared && mData.IsInfraredProcessed || !GenerateInfrared) && 
                (GenerateWaterVapour && mData.IsWaterVapourProcessed || !GenerateWaterVapour) && 
                (GenerateOtherImages && mData.IsOtherDataProcessed || !GenerateOtherImages) &&
                mData.GroupTimeout
            ) {
                UIConsole.Debug($"Group Data {idx} is done. Removing it from Organizer.");
                organizer.RemoveGroupData(idx);   
            }
        }
        #endregion
        #region Image Functions and Tools
        private void GenerateImageOverlay(ref Bitmap bmp, GroupData gd, OrganizerData od) {
            if (gd.HasNavigationData) {
                var gc = new GeoConverter (gd.SatelliteLongitude, od.ColumnOffset, od.LineOffset, od.ColumnScalingFactor, od.LineScalingFactor, true, od.Columns);
                if (mapDrawer != null && GenerateMapOverlays) {
                    mapDrawer.DrawMap (ref bmp, gc, MapOverlayPenColor, MapOverlayPenThickness, gd.CropImage);
                }
                if (GenerateLatLonOverlays) {
                    ImageTools.DrawLatLonLines (ref bmp, gc, LatLonOverlayPenColor, LatLonOverlayPenThickness, gd.CropImage);
                }
                if (GenerateLabels) {
                    ImageTools.ImageLabel (ref bmp, gd, od, gc, GenerateLatLonLabel);
                }
            } else if (GenerateLabels) {
                ImageTools.ImageLabel (ref bmp, gd, od, null, false);
            }
        }
        private static string GenFilename(string satelliteName, string regionName, string imageName, int timestamp, string origName = null) {
            if (UseNOAAFileFormat) {
                //gos15chnIR04rgnFDseg001res04dat130 18 06 19 190.lrit
                origName = origName != null ? Path.GetFileName (origName) : "";

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
        #endregion
        #region Thread Main Function
        void ThreadLoop() {
            try {
                while (running) {
                    organizer.Update ();
                    var data = organizer.GroupData;
                    var clist = data.ToList ();
                    foreach (var z in clist) {
                        var mData = z.Value;
                        if (!running) {
                            break;
                        }
                        string ImageName = string.Format ("{0}-{1}-{2}", z.Key, mData.SatelliteName, mData.RegionName);
                        if (!mData.IsProcessed) {
                            try {

                                if (!GenerateVisible) {
                                    mData.Visible.OK = true;
                                    mData.IsVisibleProcessed = true;
                                }

                                if (!GenerateInfrared) {
                                    mData.Infrared.OK = true;
                                    mData.IsInfraredProcessed = true;
                                }

                                if (!GenerateWaterVapour) {
                                    mData.WaterVapour.OK = true;
                                    mData.IsWaterVapourProcessed = true;
                                }

                                if (!GenerateFalseColor) {
                                    mData.IsFalseColorProcessed = true;
                                }

                                if (!GenerateOtherImages) {
                                    mData.OtherData.Select( x => x.Value ).ToList().ForEach(k => { k.OK = true; });
                                }

                                if (ImageManager.GenerateVisible && mData.Visible.IsComplete && mData.Visible.MaxSegments != 0 && !mData.IsVisibleProcessed) {
                                    string ofilename = Path.Combine (folder, GenFilename (mData.SatelliteName, mData.RegionName, "VIS", z.Key, mData.Visible.Segments [mData.Visible.FirstSegment]));
                                    if (File.Exists (ofilename)) {
                                        UIConsole.Debug ($"Skipping generating Visible for {Path.GetFileName(ofilename)}. Image already exists.");
                                        mData.IsVisibleProcessed = true;
                                    } else {
                                        UIConsole.Debug (string.Format ("Starting Generation of Visible for {0}.", Path.GetFileName (ofilename)));
                                        var bmp = ImageTools.GenerateFullImage (mData.Visible, mData.CropImage);
                                        if (GenerateMapOverlays || GenerateLatLonOverlays || GenerateLabels) {
                                            if (SaveNonOverlay) {
                                                string orgFileName = Path.Combine (folder, $"{Path.GetFileNameWithoutExtension(ofilename)}-original.png");
                                                bmp.Save (orgFileName, ImageFormat.Png);
                                            }
                                            UIConsole.Debug (string.Format ("Generating Overlays Visible for {0}.", Path.GetFileName (ofilename)));
                                            GenerateImageOverlay (ref bmp, mData, mData.Visible);
                                        }
                                        bmp.Save (ofilename, ImageFormat.Png);
                                        bmp.Dispose ();
                                        UIConsole.Log ($"New Visible Image: {Path.GetFileName(ofilename)}");
                                        EventMaster.Post ("newFile", new NewFileReceivedEventData () {
                                            Name = Path.GetFileName (ofilename),
                                            Path = ofilename,
                                            Metadata = {
                                                { "channel", "visible" }, {
                                                    "satelliteName",
                                                    mData.SatelliteName
                                                }, {
                                                    "regionName",
                                                    mData.RegionName
                                                }, {
                                                    "timestamp",
                                                    z.Key.ToString ()
                                                }
                                            }
                                        });
                                    }
                                    mData.IsVisibleProcessed = true;
                                    mData.Visible.OK = true;
                                } else if (mData.Visible.MaxSegments == 0) {
                                    mData.IsVisibleProcessed = true;
                                    mData.Visible.OK = true;
                                }

                                if (ImageManager.GenerateInfrared && mData.Infrared.IsComplete && mData.Infrared.MaxSegments != 0 && !mData.IsInfraredProcessed) {
                                    string ofilename = Path.Combine (folder, GenFilename (mData.SatelliteName, mData.RegionName, "IR", z.Key, mData.Infrared.Segments [mData.Infrared.FirstSegment]));
                                    if (File.Exists (ofilename)) {
                                        UIConsole.Debug ($"Skipping generating Infrared for {Path.GetFileName(ofilename)}. Image already exists.");
                                    } else {
                                        UIConsole.Debug ($"Starting Generation of Infrared for {Path.GetFileName(ofilename)}.");
                                        var bmp = ImageTools.GenerateFullImage (mData.Infrared, mData.CropImage);
                                        if (GenerateMapOverlays || GenerateLatLonOverlays || GenerateLabels) {
                                            if (SaveNonOverlay) {
                                                string orgFileName = Path.Combine (folder, $"{Path.GetFileNameWithoutExtension(ofilename)}-original.png");
                                                bmp.Save (orgFileName, ImageFormat.Png);
                                            }
                                            UIConsole.Debug (string.Format ("Generating Overlays Infrared for {0}.", Path.GetFileName (ofilename)));
                                            GenerateImageOverlay (ref bmp, mData, mData.Infrared);
                                        }
                                        bmp.Save (ofilename, ImageFormat.Png);
                                        bmp.Dispose ();
                                        UIConsole.Log ($"New Infrared Image: {Path.GetFileName(ofilename)}");
                                        EventMaster.Post ("newFile", new NewFileReceivedEventData () {
                                            Name = Path.GetFileName (ofilename),
                                            Path = ofilename,
                                            Metadata = {
                                                { "channel", "infrared" }, {
                                                    "satelliteName",
                                                    mData.SatelliteName
                                                }, {
                                                    "regionName",
                                                    mData.RegionName
                                                }, {
                                                    "timestamp",
                                                    z.Key.ToString ()
                                                }
                                            }
                                        });
                                    }
                                    mData.IsInfraredProcessed = true;
                                    mData.Infrared.OK = true;
                                } else if (mData.Infrared.MaxSegments == 0) {
                                    mData.IsInfraredProcessed = true;
                                    mData.Infrared.OK = true;
                                }

                                if (ImageManager.GenerateWaterVapour && mData.WaterVapour.IsComplete && mData.WaterVapour.MaxSegments != 0 && !mData.IsWaterVapourProcessed) {
                                    string ofilename = Path.Combine (folder, GenFilename (mData.SatelliteName, mData.RegionName, "WV", z.Key, mData.WaterVapour.Segments [mData.WaterVapour.FirstSegment]));
                                    if (File.Exists (ofilename)) {
                                        UIConsole.Debug ($"Skipping generating Water Vapour for {Path.GetFileName(ofilename)}. Image already exists.");
                                    } else {
                                        UIConsole.Debug ($"Starting Generation of Water Vapour for {Path.GetFileName(ofilename)}.");
                                        var bmp = ImageTools.GenerateFullImage (mData.WaterVapour, mData.CropImage);
                                        if (GenerateMapOverlays || GenerateLatLonOverlays || GenerateLabels) {
                                            if (SaveNonOverlay) {
                                                string orgFileName = Path.Combine (folder, $"{Path.GetFileNameWithoutExtension(ofilename)}-original.png");
                                                bmp.Save (orgFileName, ImageFormat.Png);
                                            }
                                            UIConsole.Debug (string.Format ("Generating Overlays WaterVapour for {0}.", Path.GetFileName (ofilename)));
                                            GenerateImageOverlay (ref bmp, mData, mData.WaterVapour);
                                        }
                                        bmp.Save (ofilename, ImageFormat.Png);
                                        bmp.Dispose ();
                                        UIConsole.Log ($"New Water Vapour Image: {Path.GetFileName(ofilename)}");
                                        EventMaster.Post ("newFile", new NewFileReceivedEventData () {
                                            Name = Path.GetFileName (ofilename),
                                            Path = ofilename,
                                            Metadata = {
                                                { "channel", "watervapour" }, {
                                                    "satelliteName",
                                                    mData.SatelliteName
                                                }, {
                                                    "regionName",
                                                    mData.RegionName
                                                }, {
                                                    "timestamp",
                                                    z.Key.ToString ()
                                                }
                                            }
                                        });
                                    }
                                    mData.IsWaterVapourProcessed = true;
                                    mData.WaterVapour.OK = true;
                                }
                                if (GenerateFalseColor && !mData.IsFalseColorProcessed && ImageTools.CanGenerateFalseColor (mData)) {
                                    string filename = GenFilename (
                                                          mData.SatelliteName, 
                                                          mData.RegionName, 
                                                          "FSCLR", 
                                                          z.Key, 
                                                          mData.Visible.Segments [mData.Visible.FirstSegment]
                                            .Replace ("VS", "FC")
                                            .Replace ("VIS", "FC")
                                                      );
                                    filename = Path.Combine (folder, filename);

                                    if (File.Exists (filename)) {
                                        UIConsole.Debug ($"Skipping generating FLSCLR for {Path.GetFileName(filename)}. Image already exists.");
                                    } else {
                                        UIConsole.Debug ($"Starting Generation of FSLCR for {Path.GetFileName(filename)}.");
                                        var bmp = ImageTools.GenerateFalseColor (mData);
                                        if (GenerateMapOverlays || GenerateLatLonOverlays || GenerateLabels) {
                                            if (SaveNonOverlay) {
                                                string orgFileName = Path.Combine (folder, $"{Path.GetFileNameWithoutExtension(filename)}-original.png");
                                                bmp.Save (orgFileName, ImageFormat.Png);
                                            }
                                            UIConsole.Debug (string.Format ("Generating Overlays False Colour for {0}.", Path.GetFileName (filename)));
                                            GenerateImageOverlay (ref bmp, mData, mData.Visible); // Using visible coordinates
                                        }
                                        bmp.Save (filename, ImageFormat.Png);
                                        bmp.Dispose ();
                                        UIConsole.Log ($"New False Colour Image: {Path.GetFileName(filename)}");
                                        EventMaster.Post ("newFile", new NewFileReceivedEventData () {
                                            Name = Path.GetFileName (filename),
                                            Path = filename,
                                            Metadata = {
                                                { "channel", "filename" }, {
                                                    "satelliteName",
                                                    mData.SatelliteName
                                                }, {
                                                    "regionName",
                                                    mData.RegionName
                                                }, {
                                                    "timestamp",
                                                    z.Key.ToString ()
                                                }
                                            }
                                        });
                                    }
                                    mData.IsFalseColorProcessed = true;
                                }
                                if (GenerateOtherImages && !mData.IsOtherDataProcessed && mData.OtherData.Count > 0) {
                                    mData.OtherData.Keys.ToList ().ForEach (k => {
                                        var gd = mData.OtherData [k];
                                        if (gd.IsComplete && gd.MaxSegments != 0 && !gd.OK) {
                                            string ofilename = GenFilename (mData.SatelliteName, mData.RegionName, gd.Code, gd.Timestamp, gd.Segments [0]);
                                            ofilename = Path.Combine (folder, ofilename);

                                            if (File.Exists (ofilename)) {
                                                UIConsole.Debug ($"Skipping generating {Path.GetFileName(ofilename)}. Image already exists.");
                                            } else {
                                                UIConsole.Debug ($"Starting Generation of {Path.GetFileName(ofilename)}.");
                                                var bmp = ImageTools.GenerateFullImage (gd, false);
                                                if (GenerateMapOverlays || GenerateLatLonOverlays || GenerateLabels) {
                                                    if (SaveNonOverlay) {
                                                        string orgFileName = Path.Combine (folder, $"{Path.GetFileNameWithoutExtension(ofilename)}-original.png");
                                                        bmp.Save (orgFileName, ImageFormat.Png);
                                                    }
                                                    UIConsole.Debug (string.Format ("Generating Overlays for {0}.", Path.GetFileName (ofilename)));
                                                    GenerateImageOverlay (ref bmp, mData, gd);
                                                }
                                                bmp.Save (ofilename, ImageFormat.Png);
                                                bmp.Dispose ();
                                                UIConsole.Log ($"New Image: {Path.GetFileName(ofilename)}");
                                                EventMaster.Post ("newFile", new NewFileReceivedEventData () {
                                                    Name = Path.GetFileName (ofilename),
                                                    Path = ofilename,
                                                    Metadata = { {
                                                            "channel",
                                                            "otherimages"
                                                        }, {
                                                            "satelliteName",
                                                            mData.SatelliteName
                                                        }, {
                                                            "regionName",
                                                            mData.RegionName
                                                        }, {
                                                            "timestamp",
                                                            z.Key.ToString ()
                                                        }
                                                    }
                                                });
                                            }
                                            gd.OK = true;
                                        }
                                    });
                                } else if (mData.OtherData.Count == 0) {
                                    if (mData.ReadyToMark) {
                                        mData.OtherData.Select (x => x.Value).ToList ().ForEach (k => { k.OK = true; });
                                    }
                                }

                                if (mData.ReadyToMark) {
                                    mData.IsProcessed = 
                                        (!GenerateFalseColor || (GenerateFalseColor && mData.IsFalseColorProcessed)) &&
                                    (!GenerateVisible || (GenerateVisible && mData.IsVisibleProcessed)) &&
                                    (!GenerateInfrared || (GenerateInfrared && mData.IsInfraredProcessed)) &&
                                    (!GenerateWaterVapour || (GenerateWaterVapour && mData.IsWaterVapourProcessed)) &&
                                    (!GenerateOtherImages || (GenerateOtherImages && mData.IsOtherDataProcessed));
                                }

                                if (mData.Timeout) {
                                    // Timeout completing, so let's erase the files.
                                    mData.ForceComplete ();
                                }

                                if (EraseFiles) {
                                    TryEraseGroupDataFiles (z.Key, mData);
                                }
                            } catch (SystemException e) {
                                UIConsole.Error ($"Error processing image (SysExcpt) {ImageName}: {e}");
                                mData.RetryCount++;
                                if (mData.RetryCount == ImageManager.MaxRetryCount) {
                                    mData.IsProcessed = true;
                                }
                            } catch (Exception e) {
                                UIConsole.Error ($"Error processing image {ImageName}: {e}");
                                mData.RetryCount++;
                                if (mData.RetryCount == ImageManager.MaxRetryCount) {
                                    mData.IsProcessed = true;
                                }
                            } 
                        }
                    }

                    if (EnableArchive) {
                        FileHandler.ArchieveHandler(folder, Path.GetFileName(folder));
                    }

                    Thread.Sleep (200);
                }
            } catch (Exception e) {
                CrashReport.Report (e);
                throw e;
            }
        }
        #endregion
    }
}


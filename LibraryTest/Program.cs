using System;
using OpenSatelliteProject;
using OpenSatelliteProject.Geo;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using OpenSatelliteProject.PacketData.Enums;
using System.Threading;
using System.Threading.Tasks;
using DotSpatial.Data;
using System.Collections.Generic;
using OpenSatelliteProject.PacketData;
using OpenSatelliteProject.Tools;
using System.Text.RegularExpressions;
using System.Globalization;

namespace LibraryTest {
    class MainClass {

        public static void Main (string[] args) {
            Console.WriteLine($"XRIT Version: {LibInfo.Version}");
            AppDomain.CurrentDomain.UnhandledException += CrashReport.DefaultExceptionHandler;
            //*
            //Organizer org = new Organizer("./himawari");
            //org.Update();
            //var gd = org.GroupData[1490489400];
            //var od = gd.Infrared;
            /*
			Console.WriteLine("Initializing organizer");
			Organizer org = new Organizer("/home/lucas/Works/OpenSatelliteProject/split/goesdump/XRITLibraryTest/bin/Debug/channels/Images/Full Disk/");
			org.Update();
			int k = 0;
			int c = 0;
			foreach (var z in org.GroupData.Keys) {
				k = z;
				c++;
				if (c == 2) { // 20 For US
					break;
				}
			}
			var gd = org.GroupData[k];
			var od = gd.Visible;

			Console.WriteLine("Initializing GeoConverter");
			var gc = new GeoConverter(gd.SatelliteLongitude, gd.ColumnOffset, gd.LineOffset, gd.ColumnScalingFactor, gd.LineScalingFactor, true, od.Columns);

            Console.WriteLine("Generating BMP");
			//var bmp = ImageTools.GenerateFullImage(od);
			var bmp = ImageTools.GenerateFalseColor(gd);
			var mapDrawer = new MapDrawer("/home/lucas/Works/OpenSatelliteProject/split/borders/ne_10m_admin_1_states_provinces.shp");
			//var mapDrawer = new MapDrawer("/home/lucas/Works/OpenSatelliteProject/split/borders/ne_50m_admin_0_countries.shp");
			Console.WriteLine("Drawing Map");
			mapDrawer.DrawMap(ref bmp, gc, Color.Aqua, 2, true);

			Console.WriteLine("Drawing LatLon Lines");
			ImageTools.DrawLatLonLines(ref bmp, gc, Color.Brown, 1, true);

			bmp.Save("unitedstates.jpg", ImageFormat.Jpeg);
			bmp.Dispose();
			// */
            ///*
            string filename = "./OR_ABI-L2-CMIPF-M3C13_G16_s20170861545382_e20170861556160_c20170861556231.lrit";
            string visFilename = "./OR_ABI-L2-CMIPF-M3C02_G16_s20170861545382_e20170861556149_c20170861556217.lrit";
            XRITHeader header = FileParser.GetHeaderFromFile(filename);
            Console.WriteLine($"Parsing file {header.Filename}");
            Regex x = new Regex(@".*\((.*)\)", RegexOptions.IgnoreCase);
            var regMatch = x.Match(header.ImageNavigationHeader.ProjectionName);
            float satelliteLongitude = float.Parse(regMatch.Groups[1].Captures[0].Value, CultureInfo.InvariantCulture);
            var inh = header.ImageNavigationHeader;
            var gc = new GeoConverter(satelliteLongitude, inh.ColumnOffset, inh.LineOffset, inh.ColumnScalingFactor, inh.LineScalingFactor);

            var od = new OrganizerData();
            od.Segments.Add(0, filename);
            od.FirstSegment = 0;
            od.Columns = header.ImageStructureHeader.Columns;
            od.Lines = header.ImageStructureHeader.Lines;
            od.ColumnOffset = inh.ColumnOffset;
            od.PixelAspect = 1;
            var bmp = ImageTools.GenerateFullImage(od);

            od = new OrganizerData();
            od.Segments.Add(0, visFilename);
            od.FirstSegment = 0;
            od.Columns = header.ImageStructureHeader.Columns;
            od.Lines = header.ImageStructureHeader.Lines;
            od.ColumnOffset = inh.ColumnOffset;
            od.PixelAspect = 1;
            var vbmp = ImageTools.GenerateFullImage(od);

            UIConsole.GlobalEnableDebug = true;
            var shapeFile = ShapeFiles.InitShapeFiles ();

            var mapDrawer = new MapDrawer(shapeFile);
            //ImageTools.DrawLatLonLines(ref bmp, gc, Color.Brown);
            ImageTools.ApplyCurve (OpenSatelliteProject.Presets.NEW_VIS_FALSE_CURVE, ref vbmp);

            vbmp = ImageTools.ToFormat (vbmp, PixelFormat.Format32bppArgb, true);
            bmp = ImageTools.ToFormat (bmp, PixelFormat.Format32bppArgb, true);

            ImageTools.Apply2DLut(OpenSatelliteProject.Presets.FalseColorLUTVal, ref vbmp, bmp);

            var startTime = LLTools.TimestampMS ();
            mapDrawer.DrawMap(ref vbmp, gc, Color.Yellow, 2, false, true);
            var delta = LLTools.TimestampMS () - startTime;

            Console.WriteLine ($"Took {delta} ms to generate map.");

            vbmp.Save(visFilename + ".png", ImageFormat.Png);

            Bitmap landMap = mapDrawer.GenerateLandMap (gc, bmp.Width, bmp.Height);
            landMap.Save(filename + "-landmap.jpg", ImageFormat.Jpeg);
            landMap.Dispose ();
            bmp.Dispose();


            return;
            // */
            /*
			Bitmap test0 = (Bitmap) Bitmap.FromFile("test0.jpg");
			Bitmap test1 = (Bitmap) Bitmap.FromFile("test1.jpg");
			Bitmap overlay = (Bitmap) Bitmap.FromFile("goes13-fulldisk.jpg");

			test0 = test0.ToFormat(PixelFormat.Format24bppRgb, true);

			overlay.Save("hue.jpg", ImageFormat.Jpeg);

			ImageTools.ApplyOverlay(ref test0, overlay);
			test0.Save("test0-ovl.jpg", ImageFormat.Jpeg);

			ImageTools.ApplyOverlay(ref test1, overlay);
			test1.Save("test1-ovl.jpg", ImageFormat.Jpeg);

			test0.Dispose();
			test1.Dispose();
			overlay.Dispose();
			*/
            //*
            //string dcsFile = "/home/lucas/Works/OpenSatelliteProject/split/goesdump/XRITLibraryTest/bin/Debug/channels/DCS/pM-17085003239-A.dcs";
            //List<DCSHeader> d = DCSParser.parseDCS(dcsFile);
            //*
            //string debugFrames = "/media/ELTN/tmp/demuxdump-1490627438.bin";
            //string debugFrames = "/media/ELTN/tmp/debug5/demuxdump-1492732814.bin";
            //string debugFrames = "/home/lucas/Works/OpenSatelliteProject/split/issues/trango/3/debug_frames.bin";
            //string debugFrames = "/media/ELTN/tmp/debug3/raw_data.bin";
            /*
            var mapDrawer = new MapDrawer("/home/lucas/Works/OpenSatelliteProject/split/borders/ne_50m_admin_0_countries.shp");
            var fList = mapDrawer.ShapeFile.Features.ToList ();
            var bmp = new Bitmap (1280, 720, PixelFormat.Format24bppRgb);
            using (var graphics = Graphics.FromImage (bmp)) {
                Brush bgBrush = new SolidBrush (Color.Black);
                Brush polyBrush = new SolidBrush (Color.White);
                graphics.FillRectangle (bgBrush, 0, 0, 1280, 720);
                int o = 0;
                foreach (var fo in fList) {
                    Console.WriteLine ($"Writting Geometry {o}");
                    Pen pen = new Pen(Color.FromArgb((int)((255.0 * o) / fList.Count), 127, 127), 3);
                    o++;
                    for (var n = 0; n < fo.NumGeometries; n++) {
                        //Console.WriteLine ($"Writting Geometry {n}");
                        var fg = fo.GetBasicGeometryN (n);
                        var k = fg.Coordinates;

                        float lastX = float.NaN;
                        float lastY = float.NaN;

                        List<PointF> points = new List<PointF> ();
                        foreach (var z in k) {
                            float lon = (float)z.X;
                            float lat = (float)z.Y;

                            float X = bmp.Width / 2 + bmp.Width * (lon / 360);
                            float Y = bmp.Height / 2 - bmp.Height * (lat / 180);

                            if (!float.IsNaN (lastX) && !float.IsNaN (lastY)) {
                                //graphics.DrawLine (pen, lastX, lastY, X, Y);
                            }
                            lastX = X;
                            lastY = Y;

                            points.Add (new PointF (X, Y));
                        }
                        graphics.FillPolygon(polyBrush, points.ToArray());
                    }
                }
            }
            Console.WriteLine ("Saving");
            bmp.Save ("/home/lucas/test.png", ImageFormat.Png);
            bmp.Dispose ();
            Console.WriteLine ("Done");
            return;
            */

            EventMaster.On ("newFile", d => {
                var ed = (NewFileReceivedEventData) d.Data;
                //Console.WriteLine($"Received event for new file {ed.Name} at {ed.Path}");
                //Console.WriteLine(ed.ToString());
            });
            Console.WriteLine ("Hash: " + LibInfo.CommitID);
            Console.WriteLine ("Log: " + LibInfo.LogLines);
            Console.WriteLine ("Short Hash: " + LibInfo.ShortCommitID);
            Console.WriteLine ("Version: " + LibInfo.Version);
            //string debugFrames = "/media/ELTN/tmp/demuxdump-1490627438.bin";
            //string debugFrames = "/media/ELTN/tmp/G16JuneTest/demuxdump-1496790733.bin";
            //string debugFrames = "/media/ELTN/tmp/G16JuneTest/demuxdump-1500179126.bin";
            //string debugFrames = "/media/ELTN/tmp/debug14/demuxdump-1495166529.bin";
            string debugFrames = "/media/ELTN/tmp/trango/demuxdump-1500736657.bin";
            //var mapDrawer = new MapDrawer("/home/lucas/Works/OpenSatelliteProject/split/borders/ne_10m_admin_1_states_provinces.shp");

            var im0 = new ImageManager ("output/Images/Full Disk/");
            var im1 = new ImageManager ("output/Images/Northern Hemisphere/");
            var im2 = new ImageManager ("output/Images/Southern Hemisphere/");
            var im3 = new ImageManager ("output/Images/Area of Interest/");
            var im4 = new ImageManager ("output/Images/United States/");
            var im5 = new ImageManager ("output/Images/FM1/");
            var im6 = new ImageManager ("output/Images/Unknown/");

            im0.InitMapDrawer ();
            im1.InitMapDrawer ();
            im2.InitMapDrawer ();
            im3.InitMapDrawer ();
            im4.InitMapDrawer ();
            im5.InitMapDrawer ();
            im6.InitMapDrawer ();

            ImageManager.GenerateVisible = true;
            ImageManager.GenerateInfrared = true;
            ImageManager.GenerateFalseColor = true;
            ImageManager.GenerateWaterVapour = true;
            ImageManager.GenerateOtherImages = true;
            ImageManager.EraseFiles = false;
            ImageManager.UseNOAAFileFormat = true;
            ImageManager.GenerateLatLonOverlays = true;
            ImageManager.GenerateMapOverlays = true;
            ImageManager.GenerateLabels = true;
            ImageManager.GenerateLatLonLabel = true;
            ImageManager.SaveNonOverlay = false;
            //im0.Start ();
            //im1.Start ();
            //im2.Start ();
            //im3.Start ();
            //im4.Start ();
            im5.Start ();
            //im6.Start ();
            // */
            //*/
            // /*
            /*
            DemuxManager dm = new DemuxManager ();
            FileHandler.SkipDCS = true;
            FileHandler.SkipEMWIN = true;
            //const int startFrame = 956000;
            const int startFrame = 0;
            FileStream file = File.OpenRead (debugFrames);
            byte[] data = new byte[892];
            long bytesRead = startFrame * 892;
            long bytesToRead = file.Length;
            int frameN = startFrame;
            file.Position = bytesRead;
            while (bytesRead < bytesToRead) {
                if (frameN % 1000 == 0) {
                    //Console.WriteLine("Injecting Frame {0}", frameN);
                }
                bytesRead += file.Read (data, 0, 892);
                dm.parseBytes (data);
                frameN++;
            }

            Console.WriteLine ("CRC Fails: {0}", dm.CRCFails);
            Console.WriteLine ("Bugs: {0}", dm.Bugs);
            Console.WriteLine ("Frame Loss: {0}", dm.FrameLoss);
            Console.WriteLine ("Length Fails: {0}", dm.LengthFails);
            Console.WriteLine ("Packets: {0}", dm.Packets);

            Console.WriteLine ("Received Products: ");
            foreach (int pID in dm.ProductsReceived.Keys) {
                Console.WriteLine ("\t{0}: {1}", ((NOAAProductID)pID).ToString (), dm.ProductsReceived [pID]);
            }
            //*/
            //im.Stop();
            //*/
            //ProcessFile("/home/lucas/Works/OpenSatelliteProject/split/goesdump/goesdump/bin/Debug/channels/Text/NWSTEXTdat043204159214.lrit");
            /*
            Organizer organizer = new Organizer("/home/lucas/Works/OpenSatelliteProject/split/goesdump/goesdump/bin/Debug/channels/Images/Full Disk");
            organizer.Update();

            var data = organizer.GroupData;

            foreach (var z in data) {
                var mData = z.Value;
                var bmp = ImageTools.GenerateFalseColor(mData);

                if (bmp != null) {
                    bmp.Save(string.Format("{0}-{1}-{2}-{3}.jpg", mData.SatelliteName, mData.RegionName, "FLSCLR", z.Key), ImageFormat.Jpeg);
                    bmp.Dispose();
                    mData.IsProcessed = true;
                } else {
                    if (mData.Visible.IsComplete && mData.Visible.MaxSegments != 0) {
                        bmp = ImageTools.GenerateFullImage(mData.Visible);
                        bmp.Save(string.Format("{0}-{1}-{2}-{3}.jpg", mData.SatelliteName, mData.RegionName, "VIS", z.Key), ImageFormat.Jpeg);
                        bmp.Dispose();
                    }
                    if (mData.Infrared.IsComplete && mData.Infrared.MaxSegments != 0) {
                        bmp = ImageTools.GenerateFullImage(mData.Infrared);
                        bmp.Save(string.Format("{0}-{1}-{2}-{3}.jpg", mData.SatelliteName, mData.RegionName, "IR", z.Key), ImageFormat.Jpeg);
                        bmp.Dispose();
                    }
                    if (mData.WaterVapour.IsComplete && mData.WaterVapour.MaxSegments != 0) {
                        bmp = ImageTools.GenerateFullImage(mData.WaterVapour);
                        bmp.Save(string.Format("{0}-{1}-{2}-{3}.jpg", mData.SatelliteName, mData.RegionName, "WV", z.Key), ImageFormat.Jpeg);
                        bmp.Dispose();
                    }
                    Console.WriteLine("Not all segments available!");
                }
            }


            //*/

            /*
			string visFile = "/home/lucas/Works/OpenSatelliteProject/split/samples/FD 26-02-17 2106 G13VI.jpg";
			string irFile = "/home/lucas/Works/OpenSatelliteProject/split/samples/FD 26-02-17 2106 G13IR.jpg";

			Bitmap vis = new Bitmap(visFile);
			ImageTools.ApplyCurve(Presets.VIS_FALSE_CURVE, ref vis);
			vis.Save("test.jpg", ImageFormat.Jpeg);
			//vis = vis.ToFormat(PixelFormat.Format32bppArgb, true);

			Bitmap ir = new Bitmap(irFile);
			ir = ir.ToFormat(PixelFormat.Format32bppArgb, true);
			ImageTools.ApplyLUT(Presets.THERMAL_FALSE_LUT, ref ir, 3);
			ir.Save("test2.jpg", ImageFormat.Jpeg);

			ir = ir.ToFormat(PixelFormat.Format32bppArgb);
			ImageTools.CombineHStoV(ref ir, vis);

			ir.Save("final.jpg", ImageFormat.Jpeg);
			//*/
            do {
                while (! Console.KeyAvailable) {
                    Thread.Sleep(1000);
                }
            } while (Console.ReadKey(true).Key != ConsoleKey.Escape);
        }
    }
}

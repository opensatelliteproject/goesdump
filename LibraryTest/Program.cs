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
using OpenSatelliteProject.GOES;

namespace LibraryTest {
    class MainClass {
        public static void Main (string[] args) {
            Console.WriteLine($"XRIT Version: {LibInfo.Version}");
            AppDomain.CurrentDomain.UnhandledException += CrashReport.DefaultExceptionHandler;
            #region GeoConverter Tests
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
            #endregion
            #region Reprojection / Map Test
            /*
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

            //bmp.Save(filename + "-orig.png", ImageFormat.Png);
            //vbmp.Save(visFilename + "-orig.png", ImageFormat.Png);

            //Bitmap reproj = ImageTools.ReprojectLinear (vbmp, gc);

            //reproj.Save("test.png", ImageFormat.Png);

            // /*
            var mapDrawer = new MapDrawer("/home/lucas/Works/OpenSatelliteProject/split/borders/ne_50m_admin_1_states_provinces_lakes.shp");
            //var mapDrawer = new MapDrawer(shapeFile);
            //ImageTools.DrawLatLonLines(ref bmp, gc, Color.Brown);
            ImageTools.ApplyCurve (OpenSatelliteProject.Presets.NEW_VIS_FALSE_CURVE, ref vbmp);

            vbmp = ImageTools.ToFormat (vbmp, PixelFormat.Format32bppArgb, true);
            bmp = ImageTools.ToFormat (bmp, PixelFormat.Format32bppArgb, true);

            ImageTools.Apply2DLut(OpenSatelliteProject.Presets.FalseColorLUTVal, ref vbmp, bmp);

            var startTime = LLTools.TimestampMS ();
            mapDrawer.DrawMap(ref vbmp, gc, Color.Yellow, 2, false, true);
            var delta = LLTools.TimestampMS () - startTime;

            Console.WriteLine ($"Took {delta} ms to generate map.");

            vbmp.Save(visFilename + ".jpg", ImageFormat.Jpeg);

            Bitmap landMap = mapDrawer.GenerateLandMap (gc, bmp.Width, bmp.Height);
            landMap.Save(filename + "-landmap.jpg", ImageFormat.Jpeg);
            landMap.Dispose ();
            bmp.Dispose();

            //Console.WriteLine ("Starting Reprojection");
            //Bitmap reproj = ImageTools.ReprojectLinear (vbmp, gc);
            //Console.WriteLine ("Reprojection end");
            //reproj.Save("test-falsecolor-reproject.jpg", ImageFormat.Jpeg);


            //return;
            // */
            #endregion
            #region Ingestor Test
            ///*
            EventMaster.On ("newFile", d => {
                var ed = (NewFileReceivedEventData) d.Data;
                //Console.WriteLine($"Received event for new file {ed.Name} at {ed.Path}");
                //Console.WriteLine(ed.ToString());
            });
            Console.WriteLine ("Hash: " + LibInfo.CommitID);
            Console.WriteLine ("Log: " + LibInfo.LogLines);
            Console.WriteLine ("Short Hash: " + LibInfo.ShortCommitID);
            Console.WriteLine ("Version: " + LibInfo.Version);

            OpenSatelliteProject.Presets.LoadFalseColorLUT("falsecolor.png");
            OpenSatelliteProject.Presets.LoadVisibleFalseColorCurve("curve.txt");

            //string debugFrames = "/media/ELTN/tmp/demuxdump-1490627438.bin";
            //string debugFrames = "/media/ELTN/tmp/G16JuneTest/demuxdump-1496790733.bin";
            //string debugFrames = "/media/ELTN/tmp/G16JuneTest/demuxdump-1500179126.bin";
            //string debugFrames = "/media/ELTN/tmp/debug14/demuxdump-1495166529.bin";
            //string debugFrames = "/media/ELTN/tmp/debug16/demuxdump-1504736974.bin";
//	        string debugFrames = "/media/ELTN/tmp/debug17/demuxdump-1505145094.bin";
//	        string debugFrames = "/media/ELTN/tmp/G16Bug/demuxdump-1537197025.bin";
//	        string debugFrames = "/media/ELTN/tmp/G16Bug/demuxdump-1537119165.bin";
	        string debugFrames = "/media/ELTN/tmp/G16Bug/demuxdump-1537365333.bin";
	       
            //var mapDrawer = new MapDrawer("/home/lucas/Works/OpenSatelliteProject/split/borders/ne_10m_admin_1_states_provinces.shp");
			var mim = new MultiImageManager(new []
	        {
		        "output/Images/Full Disk/",
		        "output/Images/Northern Hemisphere/",
		        "output/Images/Southern Hemisphere/",
		        "output/Images/Area of Interest/",
		        "output/Images/United States/",
		        "output/Images/FM1/",
		        "output/Images/Unknown/",
	        }, false);
	        
	        mim.InitMapDrawer();
            
            UIConsole.GlobalEnableDebug = true;

            FileHandler.SkipWeatherData = true;
            FileHandler.SkipEMWIN = true;
            FileHandler.SkipDCS = true;
            ImageManager.GenerateVisible = true;
            ImageManager.GenerateInfrared = true;
            ImageManager.GenerateFalseColor = false;
            ImageManager.GenerateWaterVapour = true;
            ImageManager.GenerateOtherImages = true;
            ImageManager.EraseFiles = true;
            ImageManager.UseNOAAFileFormat = true;
            ImageManager.GenerateLatLonOverlays = false;
            ImageManager.GenerateMapOverlays = true;
            ImageManager.GenerateLabels = true;
            ImageManager.GenerateLatLonLabel = true;
            ImageManager.SaveNonOverlay = false;
            // ImageManager.EnableReproject = true;
            mim.Start();
            // */
            //*/
            // /*
            /*
            DemuxManager dm = new DemuxManager ();
            FileHandler.SkipDCS = true;
            FileHandler.SkipEMWIN = true;
            FileHandler.SkipWeatherData = true;
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
            #endregion
            #region Archive Test
            /*

            FileHandler.ArchiveFolder = "/media/ELTN/tmp/archiveTest/archive";
            FileHandler.ArchieveHandler("/media/ELTN/tmp/archiveTest/output", "Images");
            */
            #endregion
            do {
                while (! Console.KeyAvailable) {
                    Thread.Sleep(1000);
                }
            } while (Console.ReadKey(true).Key != ConsoleKey.Escape);
        }
    }
}

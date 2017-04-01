using System;
using Gtk;
using System.IO;
using OpenSatelliteProject;
using OpenSatelliteProject.Tools;
using System.Drawing;
using System.Drawing.Imaging;
using OpenSatelliteProject.Log;

public partial class MainWindow: Gtk.Window {

    DemuxManager dm;

    public MainWindow() : base(Gtk.WindowType.Toplevel) {
        Build();

        fileChooser.FileSet += (object sender, EventArgs e) => {
            Console.WriteLine(fileChooser.Filename);
            ProcessFile(fileChooser.Filename);
        };

        //string debugFrames = "/media/ELTN/tmp/demuxdump-1490627438.bin";
        //string debugFrames = "/home/lucas/Works/OpenSatelliteProject/split/issues/trango/3/debug_frames.bin";
        string debugFrames = "/media/ELTN/tmp/debug3/raw_data.bin";
        var im = new ImageManager("channels/Images/FM1");
        ImageManager.GenerateVisible = true;
        ImageManager.GenerateInfrared = true;
        ImageManager.GenerateFalseColor = true;
        im.Start();
        dm = new DemuxManager();
        FileHandler.SkipDCS = true;
        FileHandler.SkipEMWIN = true;
        //int startFrame = 83000;
        int startFrame = 0;
        FileStream file = File.OpenRead(debugFrames);
        byte[] data = new byte[892];
        long bytesRead = startFrame * 892;
        long bytesToRead = file.Length;
        int frameN = startFrame;
        file.Position = bytesRead;
        while (bytesRead < bytesToRead) {
            //Console.WriteLine("Injecting Frame {0}", frameN);
            bytesRead += file.Read(data, 0, 892);
            dm.parseBytes(data);
            frameN++;
        }

        Console.WriteLine("CRC Fails: {0}", dm.CRCFails);
        Console.WriteLine("Bugs: {0}", dm.Bugs);
        Console.WriteLine("Frame Loss: {0}", dm.FrameLoss);
        Console.WriteLine("Length Fails: {0}", dm.LengthFails);
        Console.WriteLine("Packets: {0}", dm.Packets);
        im.Stop();

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
    }

    private void ProcessFile(string filename) {
        //string outputFolder = System.IO.Path.GetDirectoryName(filename);
        //ImageHandler.Handler.HandleFile(filename, outputFolder);
        //TextHandler.Handler.HandleFile(filename, outputFolder);
    }

    protected void OnDeleteEvent(object sender, DeleteEventArgs a) {
        Application.Quit();
        a.RetVal = true;
    }
}

using System;
using Gtk;
using System.IO;
using OpenSatelliteProject;
using OpenSatelliteProject.Tools;
using System.Drawing;
using System.Drawing.Imaging;

public partial class MainWindow: Gtk.Window {
    public MainWindow() : base(Gtk.WindowType.Toplevel) {
        Build();

        fileChooser.FileSet += (object sender, EventArgs e) => {
            Console.WriteLine(fileChooser.Filename);
            ProcessFile(fileChooser.Filename);
        };

        //ProcessFile("/home/lucas/Works/OpenSatelliteProject/split/goesdump/goesdump/bin/Debug/channels/Text/NWSTEXTdat043204159214.lrit");

        //Organizer organizer = new Organizer("/home/lucas/Works/OpenSatelliteProject/split/goesdump/goesdump/bin/Debug/channels/Images/Full Disk");
        //organizer.Update();

        string visFile = "/home/lucas/Works/OpenSatelliteProject/split/samples/FD 26-02-17 2106 G13VI.jpg";
        string irFile = "/home/lucas/Works/OpenSatelliteProject/split/samples/FD 26-02-17 2106 G13IR.jpg";

        Bitmap vis = new Bitmap(visFile);
        ImageTools.ApplyCurve(Presets.VIS_FALSE_CURVE, ref vis);
        vis.Save("test.jpg", ImageFormat.Jpeg);

        Bitmap ir = new Bitmap(irFile);
        ImageTools.ApplyLUT(Presets.THERMAL_FALSE_LUT, ref ir);
        ir.Save("test2.jpg", ImageFormat.Jpeg);

        ir = ir.ToFormat(PixelFormat.Format24bppRgb);
        ImageTools.CombineHStoV(ref ir, vis);

        ir.Save("final.jpg", ImageFormat.Jpeg);
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

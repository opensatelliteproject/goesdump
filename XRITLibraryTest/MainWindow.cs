using System;
using Gtk;
using System.IO;
using OpenSatelliteProject;
using OpenSatelliteProject.Tools;

public partial class MainWindow: Gtk.Window {
    public MainWindow() : base(Gtk.WindowType.Toplevel) {
        Build();

        fileChooser.FileSet += (object sender, EventArgs e) => {
            ProcessFile(fileChooser.Filename);
            Console.WriteLine(fileChooser.Filename);
        };

        //ProcessFile("/home/lucas/Works/OpenSatelliteProject/split/goesdump/goesdump/bin/Debug/channels/Weather Data/NWSchrt_DANGER_PAC_LATESTBWdat043203144255.lrit");
    }

    private void ProcessFile(string filename) {
        string outputFolder = System.IO.Path.GetDirectoryName(filename);
        ImageHandler.Handler.HandleFile(filename, outputFolder);
    }

    protected void OnDeleteEvent(object sender, DeleteEventArgs a) {
        Application.Quit();
        a.RetVal = true;
    }
}

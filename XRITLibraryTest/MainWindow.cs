using System;
using Gtk;
using System.IO;
using OpenSatelliteProject;
using OpenSatelliteProject.Tools;

public partial class MainWindow: Gtk.Window {
    public MainWindow() : base(Gtk.WindowType.Toplevel) {
        Build();

        fileChooser.FileSet += (object sender, EventArgs e) => {
            Console.WriteLine(fileChooser.Filename);
            ProcessFile(fileChooser.Filename);
        };

        ProcessFile("/home/lucas/Works/OpenSatelliteProject/split/goesdump/goesdump/bin/Debug/channels/Text/NWSTEXTdat043204159214.lrit");
    }

    private void ProcessFile(string filename) {
        string outputFolder = System.IO.Path.GetDirectoryName(filename);
        //ImageHandler.Handler.HandleFile(filename, outputFolder);
        //TextHandler.Handler.HandleFile(filename, outputFolder);
    }

    protected void OnDeleteEvent(object sender, DeleteEventArgs a) {
        Application.Quit();
        a.RetVal = true;
    }
}

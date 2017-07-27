using System;
using System.IO;
using System.Threading;

namespace OpenSatelliteProject {
    public static class CrashReport {
        public static bool EnableSendCrashDump = true;
        public static bool EnableSaveCrashDump = true;
        public static string CrashLogFolder = "crash";
        public static string Username = "Not defined";

        public static void DefaultExceptionHandler(object sender, ThreadExceptionEventArgs e) {
            Report (e.Exception);
        }

        public static void DefaultExceptionHandler(object sender, UnhandledExceptionEventArgs e) {
            Report ((Exception) e.ExceptionObject);
        }

        public static void Report(Exception e) {
            var cd = new CrashData (e, Username);
            try {
                UIConsole.Error($"Got {cd.ExceptionName} at {cd.Filename}:{cd.Line}");
            } catch (Exception) {
                Console.WriteLine($"Got {cd.ExceptionName} at {cd.Filename}:{cd.Line}");
            }
            SaveCrashDump (cd);
            SendCrashDump (cd);
        }

        static void SaveCrashDump(CrashData cd) {
            if (EnableSaveCrashDump) {
                try {
                    Directory.CreateDirectory(CrashLogFolder);
                    string jsonData = cd.ToJSON();
                    string filename = $"crashdata-{cd.ExceptionName}-{cd.ID}.json";
                    try {
                        UIConsole.Error($"Saving Crash Dump to: {filename}");
                    } catch (Exception) {
                        Console.WriteLine($"Saving Crash Dump to: {filename}");
                    }
                    File.WriteAllText(Path.Combine(CrashLogFolder, filename), jsonData);
                } catch (Exception e) {
                    Console.WriteLine ("FATAL: Cannot write crash dump: {0}", e);
                }
            }
        }

        static void SendCrashDump(CrashData cd) {
            if (EnableSendCrashDump) {
                // TODO
            }
        }
    }
}


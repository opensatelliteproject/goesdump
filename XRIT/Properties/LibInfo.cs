using System.Reflection;
using System.IO;
using System;

namespace OpenSatelliteProject {
    public static class LibInfo {

        private static string _commitId = "development";
        private static string _gitlog = "";

        static LibInfo() {
            var assembly = Assembly.GetExecutingAssembly();

            try {
                using (Stream stream = assembly.GetManifestResourceStream("OpenSatelliteProject.git-hash.txt"))
                using (StreamReader reader = new StreamReader(stream))
                {
                    string result = reader.ReadToEnd();
                    _commitId = result;
                }
            } catch (Exception) {
                UIConsole.Warn ("Cannot load git-hash from library.");
            }

            try {
                using (Stream stream = assembly.GetManifestResourceStream("OpenSatelliteProject.git-log.txt"))
                using (StreamReader reader = new StreamReader(stream))
                {
                    string result = reader.ReadToEnd();
                    _gitlog = result;
                }
            } catch (Exception) {
                UIConsole.Warn ("Cannot load git-log from library.");
            }
        }

        public static string CommitID { get { return _commitId; } }
        public static string ShortCommitID { get { return _commitId.Substring(0, 7); } }
        public static string[] ArrayLogLines { get { return _gitlog.Split ('\n'); } }
        public static string LogLines { get { return _gitlog; } } 

        public static int VersionMajor {
            get {
                return typeof(LibInfo).Assembly.GetName ().Version.Major;
            }
        }

        public static int VersionMinor {
            get {
                return typeof(LibInfo).Assembly.GetName ().Version.Minor;
            }
        }

        public static int VersionBuild {
            get {
                return typeof(LibInfo).Assembly.GetName ().Version.Build;
            }
        }

        public static int VersionRevision {
            get {
                return typeof(LibInfo).Assembly.GetName ().Version.Revision;
            }
        }

        public static string Version {
            get {
                return string.Format("{0}.{1}.{2}.{3}", 
                    typeof(LibInfo).Assembly.GetName().Version.Major,
                    typeof(LibInfo).Assembly.GetName().Version.Minor,
                    typeof(LibInfo).Assembly.GetName().Version.Build,
                    typeof(LibInfo).Assembly.GetName().Version.Revision
                );
            }
        }
    }
}


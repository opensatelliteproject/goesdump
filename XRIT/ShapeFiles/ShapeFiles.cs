using System;
using System.IO;
using System.Reflection;

namespace OpenSatelliteProject {
    public static class ShapeFiles {
        private static byte[] ReadFileFromAssembly(string filename) {
            byte[] data = null;
            var assembly = Assembly.GetExecutingAssembly();
            try {
                Stream stream = assembly.GetManifestResourceStream($"OpenSatelliteProject.ShapeFiles.{filename}");
                if (stream == null) {
                    stream = assembly.GetManifestResourceStream($"OpenSatelliteProject.{filename}");
                }
                using (stream) {
                    data = new byte[stream.Length];
                    int position = 0;
                    while (position < stream.Length) {
                        int chunkSize = stream.Length - position > 4096 ? 4096 : (int) (stream.Length - position);
                        stream.Read(data, position, chunkSize);
                        position += chunkSize;
                    }
                }
            } catch (Exception e) {
                UIConsole.Warn ($"ShapeFiles -- Cannot load {filename} from library.");
                UIConsole.Error($"ShapeFiles -- {e.Message}");
            }

            return data;
        }

        public static byte[] ReadSHX() {
            return ReadFileFromAssembly ("ne_50m_admin_0_countries.shx");
        }

        public static byte[] ReadDBF() {
            return ReadFileFromAssembly ("ne_50m_admin_0_countries.dbf");
        }

        public static byte[] ReadPRJ() {
            return ReadFileFromAssembly ("ne_50m_admin_0_countries.prj");
        }

        public static byte[] ReadSHP() {
            return ReadFileFromAssembly ("ne_50m_admin_0_countries.shp");
        }

        private static string ExtractFile(string filename) {
            string output = Path.Combine (Path.GetTempPath (), filename);
            UIConsole.Debug ($"ShapeFiles -- Extracting {filename} to {output}");
            byte[] data = ReadFileFromAssembly (filename);
            File.WriteAllBytes (output, data);
            return output;
        }

        // Need to do better, but since DotSpatial doesn't support loading from memory, thats what we have for now.

        public static string ExtractSHX() {
            UIConsole.Debug ("ShapeFiles -- Extracting SHX File ne_50m_admin_0_countries.shx");
            return ExtractFile ("ne_50m_admin_0_countries.shx");
        }
        public static string ExtractDBF() {
            UIConsole.Debug ("ShapeFiles -- Extracting DBF File ne_50m_admin_0_countries.dbf");
            return ExtractFile ("ne_50m_admin_0_countries.dbf");
        }
        public static string ExtractPRJ() {
            UIConsole.Debug ("ShapeFiles -- Extracting PRJ File ne_50m_admin_0_countries.prj");
            return ExtractFile ("ne_50m_admin_0_countries.prj");
        }
        public static string ExtractSHP() {
            UIConsole.Debug ("ShapeFiles -- Extracting SHP File ne_50m_admin_0_countries.shp");
            return ExtractFile ("ne_50m_admin_0_countries.shp");
        }

        public static string InitShapeFiles() {
            try {
                UIConsole.Debug("ShapeFiles -- Initializing ShapeFiles");
                ExtractDBF ();
                ExtractPRJ ();
                ExtractSHX ();
                return ExtractSHP ();
            } catch (Exception e) {
                UIConsole.Error ($"ShapeFiles -- There was an error extracting ShapeFiles: {e}");
            }
            return null;
        }
    }
}


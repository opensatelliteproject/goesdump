using System;
using System.IO;
using System.Reflection;

namespace OpenSatelliteProject {
    public static class ShapeFiles {
        private static byte[] ReadFileFromAssembly(string filename) {
            byte[] data = null;
            var assembly = Assembly.GetExecutingAssembly();
            try {
                using (Stream stream = assembly.GetManifestResourceStream($"OpenSatelliteProject.ShapeFiles.{filename}")) {
                    data = new byte[stream.Length];
                    int position = 0;
                    while (position < stream.Length) {
                        int chunkSize = stream.Length - position > 4096 ? 4096 : (int) (stream.Length - position);
                        stream.Read(data, position, chunkSize);
                        position += chunkSize;
                    }
                }
            } catch (Exception) {
                UIConsole.Warn ($"Cannot load {filename} from library.");
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
            UIConsole.Debug ($"Extracting {filename} to {output}");
            byte[] data = ReadFileFromAssembly (filename);
            File.WriteAllBytes (output, data);
            return output;
        }

        // Need to do better, but since DotSpatial doesn't support loading from memory, thats what we have for now.

        public static string ExtractSHX() {
            return ExtractFile ("ne_50m_admin_0_countries.shx");
        }
        public static string ExtractDBF() {
            return ExtractFile ("ne_50m_admin_0_countries.dbf");
        }
        public static string ExtractPRJ() {
            return ExtractFile ("ne_50m_admin_0_countries.prj");
        }
        public static string ExtractSHP() {
            return ExtractFile ("ne_50m_admin_0_countries.shp");
        }

        public static string InitShapeFiles() {
            try {
                ExtractDBF ();
                ExtractPRJ ();
                ExtractSHX ();
                return ExtractSHP ();
            } catch (Exception e) {
                UIConsole.Error ($"There was an error extracting ShapeFiles: {e}");
            }
            return null;
        }
    }
}


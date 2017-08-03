using System;
using System.IO;
using OpenSatelliteProject.Tools;

namespace OpenSatelliteProject {
    public static class Decompress {

        public static byte[] GenerateFillData(int pixels) {
            byte[] outputData = new byte[pixels];

            for (int i = 0; i < pixels; i++) {
                outputData[i] = 0x00;
            }

            return outputData;
        }

        public static byte[] InMemoryDecompress(byte[] compressedData, int pixels, int pixelsPerBlock, int mask) {
            byte[] outputData = GenerateFillData(pixels);

            try {
                AEC.LritRiceDecompress(ref outputData, compressedData, 8, pixelsPerBlock, pixels, mask);
            } catch (Exception e) {
                if (e is AECException) {
                    AECException aece = (AECException)e;
                    UIConsole.Error(string.Format("AEC Decompress Error: {0}", aece.status.ToString()));
                } else {
                    UIConsole.Error(string.Format("Decompress error: {0}", e.ToString()));
                }
            }

            return outputData;
        }

        public static string Decompressor(string filename, int pixels, int pixelsPerBlock, int mask) {
            /**
             *  Temporary Workarround. Needs to change directly on Demuxer
             */
            string outputFile = String.Format("{0}_decomp.lrit", filename);
            byte[] outputData = new byte[pixels];

            for (int i = 0; i < pixels; i++) {
                outputData[i] = 0x00;
            }

            try {
                byte[] inputData = File.ReadAllBytes(filename);
                AEC.LritRiceDecompress(ref outputData, inputData, 8, pixelsPerBlock, pixels, mask); //  AEC.ALLOW_K13_OPTION_MASK | AEC.MSB_OPTION_MASK | AEC.NN_OPTION_MASK
                File.Delete(filename);
            } catch (Exception e) {
                if (e is AECException) {
                    AECException aece = (AECException)e;
                    UIConsole.Error(string.Format("AEC Decompress Error: {0}", aece.status.ToString()));
                } else {
                    UIConsole.Error(string.Format("Decompress error: {0}", e.ToString()));
                }
            }

            File.WriteAllBytes(outputFile, outputData);
            return outputFile;
        }

        public static string Decompressor(string prefix, int pixels, int startnum, int endnum, int pixelsPerBlock, int mask) {
            /**
             *  Temporary Workarround. Needs to change directly on Demuxer
             */

            string outputFile = String.Format("{0}_decomp{1}.lrit", prefix, startnum);
            string ifile;
            FileStream f = null;
            try {
                ifile = string.Format("{0}{1}.lrit", prefix, startnum);
                byte[] input = File.ReadAllBytes(ifile);
                byte[] outputData = new byte[pixels];

                try {
                    File.Delete(ifile);
                } catch (IOException e) {
                    UIConsole.Warn(String.Format("Cannot delete file {0}: {1}", Path.GetFileName(ifile), e));
                }

                f = File.OpenWrite(outputFile);
                startnum++;
                // First file only contains header
                f.Write(input, 0, input.Length);

                int overflowCaseLast = -1;

                // Check for overflow in file number
                if (endnum < startnum) {
                    overflowCaseLast = endnum;
                    endnum = 16383;
                }

                for (int i = startnum; i <= endnum; i++) {
                    ifile = string.Format("{0}{1}.lrit", prefix, i);
                    for (int z = 0; z < outputData.Length; z++) {
                        outputData[z] = 0x00;
                    }

                    try {
                        input = File.ReadAllBytes(ifile);
                        File.Delete(ifile);
                        AEC.LritRiceDecompress(ref outputData, input, 8, pixelsPerBlock, pixels, mask);
                    } catch (FileNotFoundException) {
                        UIConsole.Error(String.Format("Decompressor cannot find file {0}", Path.GetFileName(ifile)));
                    } catch (AECException e) {
                        UIConsole.Error($"AEC Decompress problem decompressing file {Path.GetFileName(ifile)}: {e.status.ToString()}");
                        UIConsole.Debug($"AEC Params: 8 - {pixelsPerBlock} - {pixels} - {mask}");
                    } catch (IOException e) {
                        UIConsole.Error($"AEC Decompress problem decompressing file {Path.GetFileName(ifile)}: {e}");
                    }

                    f.Write(outputData, 0, outputData.Length);
                }

                if (overflowCaseLast != -1) {
                    for (int i = 0; i < overflowCaseLast; i++) {
                        ifile = string.Format("{0}{1}.lrit", prefix, i);
                        for (int z = 0; z < outputData.Length; z++) {
                            outputData[z] = 0x00;
                        }
                        try {
                            input = File.ReadAllBytes(ifile);
                            File.Delete(ifile);
                            AEC.LritRiceDecompress(ref outputData, input, 8, pixelsPerBlock, pixels, mask);
                        } catch (FileNotFoundException) {
                            UIConsole.Error(String.Format("Decompressor cannot find file {0}", Path.GetFileName(ifile)));
                        } catch (AECException e) {
                            UIConsole.Error($"AEC Decompress problem decompressing file {Path.GetFileName(ifile)}: {e.status.ToString()}");
                            UIConsole.Debug($"AEC Params: 8 - {pixelsPerBlock} - {pixels} - {mask}");
                        } catch (IOException e) {
                            Console.WriteLine("Error deleting file {0}: {1}", Path.GetFileName(ifile), e);
                        }

                        f.Write(outputData, 0, outputData.Length);
                    }
                }

            } catch (Exception e) {
                UIConsole.Error(string.Format("There was an error decompressing data: {0}", e));
            }

            try {
                if (f != null) {
                    f.Close();
                }
            } catch (Exception) {
                // Do nothing
            }

            return outputFile;
        }
    }
}


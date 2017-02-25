using System;
using System.IO;
using System.Linq;
using OpenSatelliteProject.PacketData.Structs;
using OpenSatelliteProject.PacketData;
using System.Drawing;
using System.Drawing.Imaging;
using OpenSatelliteProject.Tools;
using System.Runtime.InteropServices;

namespace OpenSatelliteProject.Tools {
    public class ImageHandler {

        public static ImageHandler Handler { get; private set; }

        static ImageHandler() {
            ImageHandler.Handler = new ImageHandler();
        }

        private ImageHandler() {
            
        }

        public void HandleFile(string filename, string outputFolder) {
            var f = File.Open(filename, FileMode.Open);
            var firstHeader = new byte[3];
            f.Read(firstHeader, 0, 3);
            if (firstHeader[0] == 0) {
                var tmp = firstHeader.Skip(1).Take(2).ToArray();
                if (BitConverter.IsLittleEndian) {
                    Array.Reverse(tmp);
                }

                int size = BitConverter.ToUInt16(tmp, 0);
                firstHeader = new byte[size - 3];
                f.Seek(0, SeekOrigin.Begin);
                f.Read(firstHeader, 0, size - 3);

                PrimaryRecord fh = LLTools.ByteArrayToStruct<PrimaryRecord>(firstHeader);
                fh = LLTools.StructToSystemEndian(fh);

                f.Seek(0, SeekOrigin.Begin);
                tmp = new byte[fh.HeaderLength];
                f.Read(tmp, 0, (int)fh.HeaderLength);
                var header = FileParser.GetHeader(tmp);
                ProcessFile(f, header, outputFolder);
                f.Close();
            } else {
                Console.WriteLine("Expected header type 0 for first header. Got {0}.", (int)firstHeader[0]);
            }
        }

        private void ProcessCompressedFile(FileStream file, XRITHeader header, string outputFolder) {
            if (header.NOAASpecificHeader.Compression == CompressionType.GIF) {
                string outName = header.Filename.Replace(".lrit", ".gif");
                outName = Path.Combine(outputFolder, outName);
                var file2 = File.OpenWrite(outName);

                byte[] buffer = new Byte[1024];
                int bytesRead;

                while ((bytesRead = file.Read(buffer, 0, 1024)) > 0) {
                    file2.Write(buffer, 0, bytesRead);
                }

                file2.Close();
            } else if (header.NOAASpecificHeader.Compression == CompressionType.JPEG) {
                string outName = header.Filename.Replace(".lrit", ".jpg");
                outName = Path.Combine(outputFolder, outName);
                var file2 = File.OpenWrite(outName);

                byte[] buffer = new Byte[1024];
                int bytesRead;

                while ((bytesRead = file.Read(buffer, 0, 1024)) > 0) {
                    file2.Write(buffer, 0, bytesRead);
                }

                file2.Close();
            } else {
                throw new Exception(string.Format("Unknown Compression type: {0}", header.NOAASpecificHeader.Compression.ToString()));
            }
        }

        private void ProcessFile(FileStream file, XRITHeader header, string outputFolder) {
            var width = header.ImageStructureHeader.Columns;
            var height = header.ImageStructureHeader.Lines;
            var bitsPerPixel = header.ImageStructureHeader.BitsPerPixel;

            if (header.NOAASpecificHeader.Compression != CompressionType.NO_COMPRESSION && header.NOAASpecificHeader.Compression != CompressionType.LRIT_RICE) {
                ProcessCompressedFile(file, header, outputFolder);
                return;
            }

            if (bitsPerPixel != 8 && bitsPerPixel != 1) {
                throw new Exception(string.Format("Unsupported bits per pixel {0}", bitsPerPixel));
            }

            var format = bitsPerPixel == 8 ? PixelFormat.Format8bppIndexed : PixelFormat.Format1bppIndexed;
            var b = new Bitmap(width, height, format);
            var bytesToRead = (width * height);

            if (bitsPerPixel == 1) {
                bytesToRead = (8 * (bytesToRead + 7)) / 8;
                bytesToRead /= 8;
            } else {
                // Create grayscale palette
                ColorPalette pal = b.Palette;
                for(int i=0;i<=255;i++) {
                    pal.Entries[i] = Color.FromArgb(i, i, i);
                }
                b.Palette = pal;
            }

            var buffer = new byte[bytesToRead];
            file.Read(buffer, 0, bytesToRead);

            if (width % 8 == 0 || bitsPerPixel != 1) {
                var data = b.LockBits(new Rectangle(0, 0, b.Width, b.Height), ImageLockMode.WriteOnly, format);
                if (data.Stride == width * bitsPerPixel / 8) {
                    Marshal.Copy(buffer, 0, data.Scan0, buffer.Length);
                } else {
                    // So our stride is bigger than our width (alignment issues). So let's copy line by line.
                    var strideBuffer = new byte[data.Stride * height];
                    int nwidth = width * bitsPerPixel / 8;
                    for (int i = 0; i < height; i++) {
                        Buffer.BlockCopy(buffer, nwidth * i, strideBuffer, data.Stride * i, nwidth);
                    }
                    Marshal.Copy(strideBuffer, 0, data.Scan0, strideBuffer.Length);
                }
                b.UnlockBits(data);
            } else {
                // Hard mode, let's optimize this in the future.
                b = new Bitmap(width, height, PixelFormat.Format24bppRgb);
                var z = 0;
                for (int i = 0; i < bytesToRead; i++) {
                    for (int k = 7; k >= 0; k--) {
                        var x = z % width;
                        var y = z / width;
                        bool bitset = ((buffer[i] >> k) & 1) == 1;
                        if (x < width && y < height) {
                            b.SetPixel(x, y, Color.FromArgb((int)(bitset ? 0xFFFFFFFF : 0x0)));
                        }
                        z++;
                    }
                }
            }

            string outName = header.Filename.Replace(".lrit", ".jpg");
            outName = Path.Combine(outputFolder, outName);
            b.Save(outName, ImageFormat.Jpeg);
        }
    }
}


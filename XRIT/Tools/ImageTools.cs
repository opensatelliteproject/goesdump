using System;
using System.Drawing;
using System.Drawing.Imaging;
using OpenSatelliteProject.Tools;
using System.Runtime.InteropServices;
using System.IO;
using System.Drawing.Drawing2D;
using OpenSatelliteProject.Geo;


#if DEBUG
using System.Diagnostics;
#endif

namespace OpenSatelliteProject {
    public static class ImageTools {

        private const int OVERLAY_THRESHOLD = 10;
        private const float OVERLAY_ALPHA_HOLD = 100f;

        /// <summary>
        /// Determines if can generate false color for the specified data.
        /// </summary>
        /// <returns><c>true</c> if can generate false color the specified data; otherwise, <c>false</c>.</returns>
        /// <param name="data">Data.</param>
        public static bool CanGenerateFalseColor(GroupData data) {
            return data.Visible.IsComplete && data.Visible.MaxSegments != 0 && data.Infrared.IsComplete && data.Infrared.MaxSegments != 0;
        }

        /// <summary>
        /// Generates False Color Image from Group Data if Visible and Infrared channels are complete.
        /// Returns null otherwise.
        /// </summary>
        /// <returns>The false color image</returns>
        /// <param name="data">Group Data</param>
        public static Bitmap GenerateFalseColor(GroupData data) {
            if (data.Visible.IsComplete && data.Visible.MaxSegments != 0 && data.Infrared.IsComplete && data.Infrared.MaxSegments != 0) {
                #if DEBUG
                Stopwatch watch = Stopwatch.StartNew();
                #endif
                var visible = GenerateFullImage(data.Visible, data.CropImage);
                #if DEBUG
                watch.Stop();
                Console.WriteLine("Took {0} milisseconds to generate Visible Image", watch.ElapsedMilliseconds);
                watch = Stopwatch.StartNew();
                #endif
                var infrared = GenerateFullImage(data.Infrared, data.CropImage);
                #if DEBUG
                watch.Stop();
                Console.WriteLine("Took {0} milisseconds to generate Infrared Image", watch.ElapsedMilliseconds);
                watch = Stopwatch.StartNew();
                #endif
                ImageTools.ApplyCurve(Presets.VIS_FALSE_CURVE, ref visible);
                #if DEBUG
                watch.Stop();
                Console.WriteLine("Took {0} milisseconds to apply visible curve", watch.ElapsedMilliseconds);
                watch = Stopwatch.StartNew();
                #endif
                ImageTools.ApplyLUT(Presets.THERMAL_FALSE_LUT, ref infrared, 3);
                #if DEBUG
                watch.Stop();
                Console.WriteLine("Took {0} milisseconds to apply infrared LUT", watch.ElapsedMilliseconds);
                watch = Stopwatch.StartNew();
                #endif
                ImageTools.CombineHStoV(ref infrared, visible);
                #if DEBUG
                watch.Stop();
                Console.WriteLine("Took {0} milisseconds to combine HS to V", watch.ElapsedMilliseconds);
                #endif
                visible.Dispose();
                return infrared;
            } else {
                return null;
            }
        }

        /// <summary>
        /// Generates the full image represented on OrganizedData
        /// </summary>
        /// <returns>The full image</returns>
        /// <param name="data">The Organizer Data</param>
        public static Bitmap GenerateFullImage(OrganizerData data, bool crop = false) {
            Bitmap bmp = new Bitmap(data.Columns, data.Lines, PixelFormat.Format8bppIndexed);

            ColorPalette pal = bmp.Palette;
            // Standard grayscale palette
            for(int i=0;i<=255;i++) {
                pal.Entries[i] = Color.FromArgb(i, i, i);
            }
            bmp.Palette = pal;

            int lineOffset = 0;

            var pdata = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, PixelFormat.Format8bppIndexed);
            // Dump files to bitmap
            for (int i = 0; i < data.Segments.Count; i++) {
                var filename = data.Segments[i];
                var header = FileParser.GetHeaderFromFile(filename);
                var width = header.ImageStructureHeader.Columns;
                var height = header.ImageStructureHeader.Lines;
                var bytesToRead = (width * height);
                var buffer = new byte[bytesToRead];

                // Read data
                var file = File.OpenRead(filename);
                file.Seek(header.PrimaryHeader.HeaderLength, SeekOrigin.Begin);
                file.Read(buffer, 0, bytesToRead);
                file.Close();

                // Write data to image
                if (pdata.Stride == width) {
                    Marshal.Copy(buffer, 0, IntPtr.Add(pdata.Scan0, lineOffset * pdata.Stride), buffer.Length);
                    lineOffset += height;
                } else {
                    // So our stride is bigger than our width (alignment issues). So let's copy line by line.
                    for (int z = 0; z < height; z++) {
                        Marshal.Copy(buffer, width * z, IntPtr.Add(pdata.Scan0, lineOffset * pdata.Stride), width);
                        lineOffset += 1;
                    }
                }
            }
            bmp.UnlockBits(pdata);

            // Crop
            if (crop) {
                int sc = (int)data.ColumnOffset;
                int hw = (int)Math.Min(data.Columns - sc, sc);
                int cl = (int)data.ColumnOffset - hw;
                int cf = cl + 2 * hw;

                bmp = bmp.Crop(cl, 0, cf - cl, bmp.Height, true);
            }
            // Resize to match pixel aspect
            int newHeight = (int) Math.Round(bmp.Height * data.PixelAspect);
            bmp = ResizeImage(bmp, bmp.Width, newHeight, true);

            return bmp;
        }

        /// <summary>
        /// Resize the image to the specified width and height.
        /// </summary>
        /// <param name="image">The image to resize.</param>
        /// <param name="width">The width to resize to.</param>
        /// <param name="height">The height to resize to.</param>
        /// <returns>The resized image.</returns>
        public static Bitmap ResizeImage(Image image, int width, int height, bool disposeOld) {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            using (var graphics = Graphics.FromImage(destImage)) {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes()) {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width,image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            if (disposeOld) {
                image.Dispose();
            }

            return destImage;
        }

        /// <summary>
        /// Crop the specified bitmap, x, y, w, h and disposeOld.
        /// </summary>
        /// <param name="bitmap">Bitmap.</param>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        /// <param name="w">The width.</param>
        /// <param name="h">The height.</param>
        /// <param name="disposeOld">If set to <c>true</c> dispose old.</param>
        public static Bitmap Crop(this Bitmap bitmap, int x, int y, int w, int h, bool disposeOld) {
            Bitmap cropped = bitmap.Clone(new Rectangle(x, y, w, h), bitmap.PixelFormat);

            if (disposeOld) {
                bitmap.Dispose();
            }
            return cropped;
        }

        /// <summary>
        /// Converts the bitmap from one format to another.
        /// </summary>
        /// <returns>The converted bitmap</returns>
        /// <param name="orig">Original bitmap</param>
        /// <param name="newFormat">New format</param>
        public static Bitmap ToFormat(this Bitmap orig, PixelFormat newFormat) {
            return ToFormat(orig, newFormat, false);
        }

        /// <summary>
        /// Converts the bitmap from one format to another.
        /// </summary>
        /// <returns>The converted bitmap</returns>
        /// <param name="orig">Original bitmap</param>
        /// <param name="newFormat">New format</param>
        public static Bitmap ToFormat(this Bitmap orig, PixelFormat newFormat, bool disposeOld) {
            Bitmap newBmp = new Bitmap(orig.Width, orig.Height, newFormat);
            using (Graphics gr = Graphics.FromImage(newBmp)) {
                gr.DrawImage(orig, new Rectangle(0, 0, newBmp.Width, newBmp.Height));
            }

            if (disposeOld) {
                orig.Dispose();
            }
            return newBmp;
        }

        /// <summary>
        /// Applies a LUT to the indexed bitmap using the Index Pallete
        /// </summary>
        /// <param name="lut">Raw Lookup Table</param>
        /// <param name="bitmap">Bitmap to be applied</param>
        public static void ApplyLUT(byte[] lut, ref Bitmap bitmap, int lutPointSize) {
            if (bitmap.PixelFormat == PixelFormat.Format8bppIndexed) {
                ColorPalette pal = bitmap.Palette;
                for (int i = 0; i <= 255; i++) {
                    pal.Entries[i] = Color.FromArgb(lut[i * lutPointSize], lut[i * lutPointSize + 1 % lutPointSize], lut[i * lutPointSize + 2 % lutPointSize]);
                }

                bitmap.Palette = pal;
            } else if (bitmap.PixelFormat == PixelFormat.Format24bppRgb) {
                var data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, bitmap.PixelFormat);
                int totalPoints = data.Stride * bitmap.Height;
                unsafe {
                    byte* dPtr = (byte*)data.Scan0.ToPointer();
                    for (int c = 0; c < totalPoints; c++) {
                        int subPixel = c % 3;
                        int lutPoint = (lutPointSize - subPixel - 1) % lutPointSize;
                        byte d = dPtr[c];
                        dPtr[c] = lut[d*lutPointSize + lutPoint];
                    }
                }
                bitmap.UnlockBits(data);
            } else if (bitmap.PixelFormat == PixelFormat.Format32bppArgb) {
                var data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, bitmap.PixelFormat);
                int totalPoints = data.Stride * bitmap.Height;
                unsafe {
                    byte* dPtr = (byte*)data.Scan0.ToPointer();
                    for (int c = 0; c < totalPoints; c++) {
                        int subPixel = c % 4;
                        int lutPoint = (lutPointSize - subPixel - 1) % lutPointSize;
                        if (subPixel != 3) { // Ignore Alpha
                            byte d = dPtr[c];
                            dPtr[c] = lut[d * lutPointSize + lutPoint];
                        } else {
                            dPtr[c] = 0xFF;
                        }
                    }
                }
                bitmap.UnlockBits(data); 
            } else {
                throw new Exception("Pixel format not supported!");
            }
        }

        /// <summary>
        /// Applies a Correction Curve to Image
        /// </summary>
        /// <param name="curve">Curve points (exactly 256 points)</param>
        /// <param name="bitmap">Bitmap to be applied</param>
        public static void ApplyCurve(float[] curve, ref Bitmap bitmap) {
            if (curve.Length != 256) {
                throw new Exception("Curve should contain exactly 256 floats.");
            }

            byte[] cLut = new byte[256];

            for (int i = 0; i < 256; i++) {
                float v = 255 * curve[i];
                cLut[i] = (byte) (((int)Math.Floor(v)) & 0xFF);
            }

            ApplyLUT(cLut, ref bitmap, 1);
        }

        /// <summary>
        /// Converts RGB to HSV
        /// </summary>
        /// <param name="rgb">Rgb.</param>
        public static float[] rgb2hsv(byte[] rgb) {
            float h = 0, s = 0, v = 0;
            float r, g, b;

            r = rgb[0] / 255f;
            g = rgb[1] / 255f;
            b = rgb[2] / 255f;

            float mx = Math.Max(Math.Max(r, g), b);
            float mn = Math.Min(Math.Min(r, g), b);
            float df = mx - mn;
            if (mx == mn) {
                h = 0;
            } else if (mx == r) {
                h = (60 * ((g - b) / df) + 360) % 360;
            } else if (mx == g) {
                h = (60 * ((g - b) / df) + 360) % 360;
            } else if (mx == b) {
                h = (60 * ((r-g)/df) + 240) % 360;
            }

            if (mx == 0) {
                s = 0;
            } else {
                s = df / mx;
            }

            v = mx;

            return new float[] { h, s, v };
        }

        /// <summary>
        /// Converts HSV to RGB
        /// </summary>
        /// <param name="hsv">Hsv.</param>
        public static byte[] hsv2rgb (float[] hsv) {
            float h = hsv[0], s = hsv[1], v = hsv[2];
            float h60 = h / 60f;
            float h60f = (float) Math.Floor(h60);
            int hi = ((int)h60f) % 6;
            float f = h60 - h60f;
            float p = v * (1 - s);
            float q = v * (1 - f * s);
            float t = v * (1 - (1 - f) * s);
            float r = 0, g = 0, b = 0;

            switch (hi) {
                case 0:
                    r = v;
                    g = t;
                    b = p;
                    break;
                case 1:
                    r = q;
                    g = v;
                    b = p;
                    break;
                case 2:
                    r = p;
                    g = v;
                    b = t;
                    break;
                case 3:
                    r = p;
                    g = q;
                    b = v;
                    break;
                case 4:
                    r = t;
                    g = p;
                    b = v;
                    break;
                case 5:
                    r = v;
                    g = p;
                    b = q;
                    break;
            } 
                    
            return new byte[] {
                (byte) (r * 255),
                (byte) (g * 255),
                (byte) (b * 255)
            };
        }

        /// <summary>
        /// Applies an overlay on the image.
        /// </summary>
        /// <param name="bmp">Bmp.</param>
        /// <param name="overlay">Overlay.</param>
        public static void ApplyOverlay(ref Bitmap bmp, Bitmap overlay) {
            if (bmp.PixelFormat != PixelFormat.Format24bppRgb && bmp.PixelFormat != PixelFormat.Format32bppArgb) {
                throw new Exception("bmp format needs to be either 24bpp or 32bpp RGB");
            }

            if (overlay.PixelFormat != bmp.PixelFormat) {
                overlay = overlay.ToFormat(bmp.PixelFormat);
            }


            var overlaydata = overlay.LockBits(new Rectangle(0, 0, overlay.Width, overlay.Height), ImageLockMode.ReadOnly, overlay.PixelFormat);
            var bmpdata = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, bmp.PixelFormat);
            int readWidth = Math.Min(overlay.Width, bmp.Width);
            int readHeight = Math.Min(overlay.Height, bmp.Height);

            if (bmp.PixelFormat == PixelFormat.Format24bppRgb) {
                unsafe {
                    byte* ovPtr = (byte*)overlaydata.Scan0.ToPointer();
                    byte* bPtr = (byte*)bmpdata.Scan0.ToPointer();

                    for (int y = 0; y < readHeight; y++) {
                        for (int x = 0; x < readWidth; x++) {
                            int c = ((bmpdata.Stride / 3) * y + x) * 3;
                            if (ovPtr[c] > OVERLAY_THRESHOLD || ovPtr[c + 1] > OVERLAY_THRESHOLD || ovPtr[c + 2] > OVERLAY_THRESHOLD) {
                                float alpha = ovPtr[c] / OVERLAY_ALPHA_HOLD; 
                                alpha = alpha > 1 ? 1 : alpha < 0 ? 0 : alpha;
                                byte v = (byte) ((ovPtr[c] * 2 > 255) ? 255 : ovPtr[c] * 2);
                                bPtr[c] =  (byte) ((int)(v * alpha + bPtr[c+0] * (1-alpha)));
                                bPtr[c+1] =(byte) ((int)(v * alpha + bPtr[c+1] * (1-alpha)));
                                bPtr[c+2] =(byte) ((int)(v * alpha + bPtr[c+2] * (1-alpha)));
                            }
                        }
                    }
                }
            } else { // 32 ARGB
                unsafe {
                    byte* ovPtr = (byte*)overlaydata.Scan0.ToPointer();
                    byte* bPtr = (byte*)bmpdata.Scan0.ToPointer();

                    for (int y = 0; y < readHeight; y++) {
                        for (int x = 0; x < readWidth; x++) {
                            // TODO: Improve this
                            int c = ((bmpdata.Stride / 4) * y + x) * 4;
                            if (ovPtr[c] > OVERLAY_THRESHOLD || ovPtr[c + 1] > OVERLAY_THRESHOLD || ovPtr[c + 2] > OVERLAY_THRESHOLD) {
                                float alpha = ovPtr[c] / OVERLAY_ALPHA_HOLD; 
                                alpha = alpha > 1 ? 1 : alpha < 0 ? 0 : alpha;
                                byte v = (byte) ((ovPtr[c] * 2 > 255) ? 255 : ovPtr[c] * 2);
                                bPtr[c] =  (byte) ((int)(v * alpha + bPtr[c+0] * (1-alpha)));
                                bPtr[c+1] =(byte) ((int)(v * alpha + bPtr[c+1] * (1-alpha)));
                                bPtr[c+2] =(byte) ((int)(v * alpha + bPtr[c+2] * (1-alpha)));
                            }
                        }
                    }
                }
            }

            overlay.UnlockBits(overlaydata);
            bmp.UnlockBits(bmpdata);
        }

        /// <summary>
        /// Combines Value of VBmp to Hue/Saturation of HSBmp.
        /// The output will be at HSBmp
        /// </summary>
        /// <param name="hsbmp">Hue/Saturation Bitmap</param>
        /// <param name="vbmp">Value Bitmap</param>
        public static void CombineHStoV(ref Bitmap hsbmp, Bitmap vbmp) {
            if (hsbmp.PixelFormat != PixelFormat.Format24bppRgb && hsbmp.PixelFormat != PixelFormat.Format32bppArgb) {
                throw new Exception("hsbmp format needs to be either 24bpp or 32bpp RGB");
            }

            if (vbmp.PixelFormat != hsbmp.PixelFormat) {
                vbmp = vbmp.ToFormat(hsbmp.PixelFormat);
            }


            var vdata = vbmp.LockBits(new Rectangle(0, 0, vbmp.Width, vbmp.Height), ImageLockMode.ReadOnly, vbmp.PixelFormat);
            var hsdata = hsbmp.LockBits(new Rectangle(0, 0, hsbmp.Width, hsbmp.Height), ImageLockMode.ReadWrite, hsbmp.PixelFormat);

            if (hsbmp.PixelFormat == PixelFormat.Format24bppRgb) {
                unsafe {
                    byte* hsPtr = (byte*)hsdata.Scan0.ToPointer();
                    byte* vPtr = (byte*)vdata.Scan0.ToPointer();

                    for (int y = 0; y < hsbmp.Height; y++) {
                        for (int x = 0; x < hsbmp.Width; x++) {
                            // TODO: Improve this
                            int c = ((hsdata.Stride / 3) * y + x) * 3;
                            byte[] rgb = new byte[] { hsPtr[c], hsPtr[c + 1], hsPtr[c + 2] };
                            float[] hsv = rgb2hsv(rgb);
                            hsv[2] = vPtr[c] / 255f;
                            rgb = hsv2rgb(hsv);
                            hsPtr[c + 0] = rgb[0];
                            hsPtr[c + 1] = rgb[1];
                            hsPtr[c + 2] = rgb[2];
                        }
                    }
                }
            } else { // 32 ARGB
                unsafe {
                    byte* hsPtr = (byte*)hsdata.Scan0.ToPointer();
                    byte* vPtr = (byte*)vdata.Scan0.ToPointer();

                    for (int y = 0; y < hsbmp.Height; y++) {
                        for (int x = 0; x < hsbmp.Width; x++) {
                            // TODO: Improve this
                            int c = ((hsdata.Stride / 4) * y + x) * 4;
                            byte[] rgb = new byte[] { hsPtr[c+0], hsPtr[c + 1], hsPtr[c + 2] };
                            float[] hsv = rgb2hsv(rgb);
                            hsv[2] = vPtr[c] / 255f;
                            rgb = hsv2rgb(hsv);
                            hsPtr[c + 0] = rgb[0];
                            hsPtr[c + 1] = rgb[1];
                            hsPtr[c + 2] = rgb[2];
                        }
                    }
                }
            }

            vbmp.UnlockBits(vdata);
            hsbmp.UnlockBits(hsdata);
        }

        /// <summary>
        /// Draws the Lat/Lon Grid spaced by 10 degrees each using the GeoConverter
        /// </summary>
        /// <param name="bmp">The Bitmap</param>
        /// <param name="gc">A geoconverter initialized with Satellite Parameters</param>
        public static void DrawLatLonLines(ref Bitmap bmp, GeoConverter gc, Color color, int lineWidth = 5, bool fixCrop = false) {
            Pen pen = new Pen(color, lineWidth);
            float lastX = -1;
            float lastY = -1;
            using (var graphics = Graphics.FromImage(bmp)) {
                for (float lat = gc.MinLatitude; lat < gc.MaxLatitude; lat += 10f) {
                    lastX = -1;
                    lastY = -1;
                    for (float lon = gc.MinLongitude; lon < gc.MaxLongitude; lon += 0.1f) {
                        var xy = gc.latlon2xy(lat, lon);

                        if (fixCrop) {
                            xy = new Tuple<int, int>(xy.Item1 - gc.CropLeft, xy.Item2);
                        }
                        if (lastX != -1 && lastY != -1) {
                            graphics.DrawLine(pen, lastX, lastY, xy.Item1, xy.Item2);
                        }
                        lastX = xy.Item1;
                        lastY = xy.Item2;
                    }

                }
                for (float lon = gc.MinLongitude; lon < gc.MaxLongitude; lon += 10f) {
                    lastX = -1;
                    lastY = -1;
                    for (float lat = gc.MinLatitude; lat < gc.MaxLatitude; lat += 0.1f) {
                        var xy = gc.latlon2xy(lat, lon);

                        if (fixCrop) {
                            xy = new Tuple<int, int>(xy.Item1 - gc.CropLeft, xy.Item2);
                        }
                        if (lastX != -1 && lastY != -1) {
                            graphics.DrawLine(pen, lastX, lastY, xy.Item1, xy.Item2);
                        }
                        lastX = xy.Item1;
                        lastY = xy.Item2;
                    }

                }
            }
        }
    }
}


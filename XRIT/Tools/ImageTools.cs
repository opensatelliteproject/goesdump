using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using OpenSatelliteProject.Tools;
using System.Runtime.InteropServices;
using System.IO;
using System.Drawing.Drawing2D;
using OpenSatelliteProject.Geo;
using System.Globalization;

namespace OpenSatelliteProject {
    public static class ImageTools {

        const int OVERLAY_THRESHOLD = 10;
        const float OVERLAY_ALPHA_HOLD = 100f;
        static readonly int[] FONT_SIZES = { 12, 24, 48, 96 };
        const int PADDING = 10; // px
        public static string OSPLABEL = $"OpenSatelliteProject {LibInfo.Version}";

        public static Bitmap ReprojectLinear(Bitmap bmp, GeoConverter gc, bool fixCrop = false) {
            var output = new Bitmap (bmp.Width, bmp.Height, PixelFormat.Format32bppArgb);
            var pdata = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            var odata = output.LockBits(new Rectangle(0, 0, output.Width, output.Height), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            unsafe {
                var dPtr = (byte*)odata.Scan0.ToPointer();
                var pPtr = (byte*)pdata.Scan0.ToPointer ();
                var stride = odata.Stride;
                for (var y = 0; y < output.Height; y++) {
                    for (var x = 0; x < output.Width; x++) {
                        var lat = (gc.MaxLatitude - gc.TrimLatitude) - ((y * (gc.LatitudeCoverage - gc.TrimLatitude * 2)) / output.Height);
                        var lon = ((x * (gc.LongitudeCoverage - gc.TrimLongitude * 2)) / output.Width) + (gc.MinLongitude + gc.TrimLongitude);
                        if (lat > gc.MaxLatitude || lat < gc.MinLatitude || lon > gc.MaxLongitude || lon < gc.MinLongitude) {
                            dPtr [y * stride + x] = 0;
                        } else {
                            var xy = gc.latlon2xyf(lat, lon);
                            var newx = xy.Item1;
                            var newy = xy.Item2;
                            if (fixCrop) {
                                newx -= gc.CropLeft; 
                            }
                            dPtr [y * stride + x * 4 + 0] = BilinearInterp(pPtr, newx, newy, pdata.Stride, 0);
                            dPtr [y * stride + x * 4 + 1] = BilinearInterp(pPtr, newx, newy, pdata.Stride, 1);
                            dPtr [y * stride + x * 4 + 2] = BilinearInterp(pPtr, newx, newy, pdata.Stride, 2);
                            dPtr [y * stride + x * 4 + 3] = BilinearInterp(pPtr, newx, newy, pdata.Stride, 3);
                        }
                    }
                }
            }
            bmp.UnlockBits(pdata);
            output.UnlockBits (odata);

            return output;
        }

        private static unsafe byte val(byte *data, int x, int y, int mw)   {
            return data[y * mw + x * 4 + 1];
        }

        private static unsafe byte ValueAtImage(byte *data, int x, int y, int mw, int colorChannel) {
            return data[y * mw + x * 4 + colorChannel];
        }

        private static unsafe byte BilinearInterp(byte *data, double x, double y, int mw, int colorChannel)   {
            var rx = (int)(x);
            var ry = (int)(y);
            var fracX = (float) (x - rx);
            var fracY = (float) (y - ry);
            var invfracX = 1f - fracX;
            var invfracY = 1f - fracY;

            var a = ValueAtImage(data, rx,     ry,     mw, colorChannel);
            var b = ValueAtImage(data, rx + 1, ry,     mw, colorChannel);
            var c = ValueAtImage(data, rx    , ry + 1, mw, colorChannel);
            var d = ValueAtImage(data, rx + 1, ry + 1, mw, colorChannel);

            return (byte) (( a * invfracX + b * fracX) * invfracY + ( c * invfracX + d * fracX) * fracY);
        }

        public static void ImageLabel(ref Bitmap inbmp, GroupData gd, OrganizerData od, GeoConverter gc, bool genLatLonLabel) {
            var usedFontSize = FONT_SIZES [0];
            if (inbmp.Width >= 4000) {
                usedFontSize = FONT_SIZES [3];
            } else if (inbmp.Width >= 2000) {
                usedFontSize = FONT_SIZES [2];
            } else if (inbmp.Width >= 1000) {
                usedFontSize = FONT_SIZES [1];
            }
            var bgBrush = new SolidBrush (Color.Black);
            var font = new Font ("Arial", usedFontSize);
            var fontBrush = new SolidBrush (Color.White);
            var upperText = $"{gd.SatelliteName} ({gd.SatelliteLongitude}) - {gd.RegionName}";

            var usedLabelSize = PADDING * 2;

            using (var g = Graphics.FromImage (inbmp)) {
                usedLabelSize += (int) Math.Round(g.MeasureString (upperText, font).Height);
            }

            var bmp = new Bitmap(inbmp.Width, inbmp.Height + ((genLatLonLabel && gc != null) ? usedLabelSize * 3 : usedLabelSize * 2), inbmp.PixelFormat);
            var sf = new StringFormat {
                LineAlignment = StringAlignment.Center,
                Alignment = StringAlignment.Center
            };

            if (od.FileHeader != null && od.FileHeader.SubProduct.Name != "None") {
                upperText = upperText + " - " + od.FileHeader.SubProduct.Name;
            }

            var dt = LLTools.UnixTimeStampToDateTime (od.Timestamp);
            var lowerText = $"{dt.ToShortDateString ()} {dt.ToLongTimeString()} UTC - {OSPLABEL}";

            using(var g = Graphics.FromImage(bmp)) {
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.FillRectangle (bgBrush, 0, 0, bmp.Width, bmp.Height);
                g.DrawImage(inbmp, 0, usedLabelSize, inbmp.Width, inbmp.Height);

                // Upper Label
//                var textSize = g.MeasureString (upperText, font);
                var rect = new RectangleF (PADDING, 0, bmp.Width - PADDING * 2, usedLabelSize);
                g.DrawString (upperText, font, fontBrush, rect, sf);

                // Lower Label with Lat/Lon
                if (genLatLonLabel && gc != null) {
                    var latlon = gc.xy2latlon (inbmp.Width / 2, inbmp.Height / 2);
                    var lat = latlon.Item1.ToString ("##.000000", CultureInfo.InvariantCulture);
                    var lon = latlon.Item2.ToString ("##.000000", CultureInfo.InvariantCulture);
                    var latLonText = $"Center Coord: ({lat}; {lon})";
                    lowerText += $"\r\n{latLonText}";
//                    textSize = g.MeasureString (lowerText, font);
                    rect = new RectangleF (PADDING, usedLabelSize + inbmp.Height, bmp.Width - PADDING * 2, usedLabelSize * 2);
                    g.DrawString (lowerText, font, fontBrush, rect, sf);
                } else { // Lower Label without Lat/Lon
//                    textSize = g.MeasureString (lowerText, font);
                    rect = new RectangleF (PADDING, usedLabelSize + inbmp.Height, bmp.Width - PADDING * 2, usedLabelSize);
                    g.DrawString (lowerText, font, fontBrush, rect, sf);
                }
            }
            inbmp.Dispose ();
            inbmp = bmp;
        }

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
                var visible = GenerateFullImage(data.Visible, data.CropImage);
                var infrared = GenerateFullImage(data.Infrared, data.CropImage);
                ApplyCurve (Presets.NEW_VIS_FALSE_CURVE, ref visible);
                Apply2DLut (Presets.FalseColorLUTVal, ref visible, infrared);
                infrared.Dispose ();
                return visible;
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
            var bmp = new Bitmap(data.Columns, data.Lines, PixelFormat.Format8bppIndexed);

            var pal = bmp.Palette;
            // Standard grayscale palette
            for (var i=0;i<=255;i++) {
                pal.Entries[i] = Color.FromArgb(i, i, i);
            }
            bmp.Palette = pal;

            var lineOffset = 0;

            var pdata = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, PixelFormat.Format8bppIndexed);
            // Dump files to bitmap
            for (var i = 0; i < data.Segments.Count; i++) {
                var filename = data.Segments[data.FirstSegment + i];
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
                    for (var z = 0; z < height; z++) {
                        Marshal.Copy(buffer, width * z, IntPtr.Add(pdata.Scan0, lineOffset * pdata.Stride), width);
                        lineOffset += 1;
                    }
                }
            }
            bmp.UnlockBits(pdata);

            // Crop
            if (crop && data.ColumnOffset > 0) {
                var sc = (int)data.ColumnOffset;
                var hw = (int)Math.Min(data.Columns - sc, sc);
                var cl = (int)data.ColumnOffset - hw;
                var cf = cl + 2 * hw;

                bmp = bmp.Crop(cl, 0, cf - cl, bmp.Height, true);
            }
            // Resize to match pixel aspect
            var newHeight = (int) Math.Round(bmp.Height * data.PixelAspect);
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
//            Bitmap cropped = bitmap.Clone(new Rectangle(x, y, w, h), bitmap.PixelFormat);
            var cropped = new Bitmap(w, h);
            var g = Graphics.FromImage(cropped);
            g.DrawImage(bitmap, -x, -y);
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
        public static Bitmap ToFormat(this Bitmap orig, PixelFormat newFormat, bool disposeOld = false) {
            var newBmp = new Bitmap(orig.Width, orig.Height, newFormat);
            using (var gr = Graphics.FromImage(newBmp)) {
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
        /// <param name="lutPointSize"></param>
        public static void ApplyLUT(byte[] lut, ref Bitmap bitmap, int lutPointSize) {
            if (bitmap.PixelFormat == PixelFormat.Format8bppIndexed) {
                var pal = bitmap.Palette;
                for (var i = 0; i <= 255; i++) {
                    pal.Entries[i] = Color.FromArgb(lut[i * lutPointSize], lut[i * lutPointSize + 1 % lutPointSize], lut[i * lutPointSize + 2 % lutPointSize]);
                }

                bitmap.Palette = pal;
            } else if (bitmap.PixelFormat == PixelFormat.Format24bppRgb) {
                var data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, bitmap.PixelFormat);
                var totalPoints = data.Stride * bitmap.Height;
                unsafe {
                    var dPtr = (byte*)data.Scan0.ToPointer();
                    for (var c = 0; c < totalPoints; c++) {
                        var subPixel = c % 3;
                        var lutPoint = (lutPointSize - subPixel - 1) % lutPointSize;
                        var d = dPtr[c];
                        dPtr[c] = lut[d*lutPointSize + lutPoint];
                    }
                }
                bitmap.UnlockBits(data);
            } else if (bitmap.PixelFormat == PixelFormat.Format32bppArgb) {
                var data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, bitmap.PixelFormat);
                var totalPoints = data.Stride * bitmap.Height;
                unsafe {
                    var dPtr = (byte*)data.Scan0.ToPointer();
                    for (var c = 0; c < totalPoints; c++) {
                        var subPixel = c % 4;
                        var lutPoint = (lutPointSize - subPixel - 1) % lutPointSize;
                        if (subPixel != 3) { // Ignore Alpha
                            var d = dPtr[c];
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

            var cLut = new byte[256];

            for (var i = 0; i < 256; i++) {
                var v = 255 * curve[i];
                cLut[i] = (byte) (((int)Math.Floor(v)) & 0xFF);
            }

            ApplyLUT(cLut, ref bitmap, 1);
        }

        /// <summary>
        /// Applies a LUT using Lookup function using visible and infrared bitmaps. Output to visible.
        /// visible bitmap needs to be 24bpp RGB or 32bpp ARGB
        /// This also assumes Grayscale images as RGB
        /// </summary>
        /// <param name="lookup">Lookup function(int visible, int infrared)</param>
        /// <param name="visible">Visible.</param>
        /// <param name="infrared">Infrared.</param>
        public static void Apply2DLut(Func<byte, byte, int> lookup, ref Bitmap visible, Bitmap infrared) {
            // FalseColorLUTVal(int thermal, int visible)
            if (visible.PixelFormat != PixelFormat.Format24bppRgb && visible.PixelFormat != PixelFormat.Format32bppArgb) {
                throw new ArgumentException ("Visible bitmap needs to be RGB24 or ARGB32");
            }

            if (infrared.PixelFormat != visible.PixelFormat) {
                infrared = ToFormat (infrared, visible.PixelFormat);
            }

            if (visible.Height != infrared.Height || visible.Width != infrared.Width) {
                UIConsole.Warn ("The Infrared and Visible channels size doesn't match, the false might look weird.\n" +
                    $"Visible({visible.Width}, {visible.Height}) vs Infrared({infrared.Width}, {infrared.Height})");
            }

            var vdata = visible.LockBits(new Rectangle(0, 0, visible.Width, visible.Height), ImageLockMode.ReadWrite, visible.PixelFormat);
            var idata = infrared.LockBits(new Rectangle(0, 0, infrared.Width, infrared.Height), ImageLockMode.ReadOnly, visible.PixelFormat);
            var totalPoints = Math.Min(vdata.Stride * visible.Height, idata.Stride * infrared.Height); // Avoids crash on corrupted images

            switch (visible.PixelFormat) {
                case PixelFormat.Format24bppRgb:
                    unsafe {
                        var vPtr = (byte*)vdata.Scan0.ToPointer();
                        var iPtr = (byte*)idata.Scan0.ToPointer();
                        for (var stridePos = 0; stridePos < totalPoints; stridePos+=3) {
                            // Assume Grayscale in RGB
                            var visVal = vPtr[stridePos];
                            var irVal = iPtr[stridePos];
                            var color = lookup (irVal, visVal);
                            vPtr [stridePos + 0] = (byte) ((color >> 0) & 0xFF);
                            vPtr [stridePos + 1] = (byte) ((color >> 8) & 0xFF);
                            vPtr [stridePos + 2] = (byte) ((color >> 16) & 0xFF);
                        }
                    }

                    break;
                case PixelFormat.Format32bppArgb:
                    unsafe {
                        var vPtr = (byte*)vdata.Scan0.ToPointer();
                        var iPtr = (byte*)idata.Scan0.ToPointer();
                        for (var stridePos = 0; stridePos < totalPoints; stridePos+=4) {
                            // Assume Grayscale in ARGB
                            var visVal = vPtr[stridePos];
                            var irVal = iPtr [stridePos];
                            var color = lookup (irVal, visVal);
                            vPtr [stridePos + 0] = (byte) ((color >> 0) & 0xFF);
                            vPtr [stridePos + 1] = (byte) ((color >> 8) & 0xFF);
                            vPtr [stridePos + 2] = (byte) ((color >> 16) & 0xFF);
                            vPtr [stridePos + 3] = (byte) ((color >> 24) & 0xFF);
                        }
                    }

                    break;
                default:
                    UIConsole.Error ($"ImageTools received an unsuported image type: {visible.PixelFormat}");
                    break;
            }
            visible.UnlockBits(vdata);
            infrared.UnlockBits(idata);
        }

        /// <summary>
        /// Converts RGB to HSV
        /// </summary>
        /// <param name="rgb">Rgb.</param>
        private static float[] rgb2hsv(IReadOnlyList<byte> rgb) {
            float h = 0, s = 0, v = 0;

            var r = rgb[0] / 255f;
            var g = rgb[1] / 255f;
            var b = rgb[2] / 255f;

            var mx = Math.Max(Math.Max(r, g), b);
            var mn = Math.Min(Math.Min(r, g), b);
            var df = mx - mn;
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
        private static byte[] hsv2rgb (IReadOnlyList<float> hsv) {
            float h = hsv[0], s = hsv[1], v = hsv[2];
            var h60 = h / 60f;
            var h60f = (float) Math.Floor(h60);
            var hi = ((int)h60f) % 6;
            var f = h60 - h60f;
            var p = v * (1 - s);
            var q = v * (1 - f * s);
            var t = v * (1 - (1 - f) * s);
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

            return new [] {
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
            var readWidth = Math.Min(overlay.Width, bmp.Width);
            var readHeight = Math.Min(overlay.Height, bmp.Height);

            if (bmp.PixelFormat == PixelFormat.Format24bppRgb) {
                unsafe {
                    var ovPtr = (byte*)overlaydata.Scan0.ToPointer();
                    var bPtr = (byte*)bmpdata.Scan0.ToPointer();

                    for (var y = 0; y < readHeight; y++) {
                        for (var x = 0; x < readWidth; x++) {
                            var c = ((bmpdata.Stride / 3) * y + x) * 3;
                            if (ovPtr[c] <= OVERLAY_THRESHOLD && ovPtr[c + 1] <= OVERLAY_THRESHOLD &&
                                ovPtr[c + 2] <= OVERLAY_THRESHOLD) continue;
                            var alpha = ovPtr[c] / OVERLAY_ALPHA_HOLD; 
                            alpha = alpha > 1 ? 1 : alpha < 0 ? 0 : alpha;
                            var v = (byte) ((ovPtr[c] * 2 > 255) ? 255 : ovPtr[c] * 2);
                            bPtr[c] =  (byte) ((int)(v * alpha + bPtr[c+0] * (1-alpha)));
                            bPtr[c+1] =(byte) ((int)(v * alpha + bPtr[c+1] * (1-alpha)));
                            bPtr[c+2] =(byte) ((int)(v * alpha + bPtr[c+2] * (1-alpha)));
                        }
                    }
                }
            } else { // 32 ARGB
                unsafe {
                    var ovPtr = (byte*)overlaydata.Scan0.ToPointer();
                    var bPtr = (byte*)bmpdata.Scan0.ToPointer();

                    for (var y = 0; y < readHeight; y++) {
                        for (var x = 0; x < readWidth; x++) {
                            // TODO: Improve this
                            var c = ((bmpdata.Stride / 4) * y + x) * 4;
                            if (ovPtr[c] <= OVERLAY_THRESHOLD && ovPtr[c + 1] <= OVERLAY_THRESHOLD &&
                                ovPtr[c + 2] <= OVERLAY_THRESHOLD) continue;
                            var alpha = ovPtr[c] / OVERLAY_ALPHA_HOLD; 
                            alpha = alpha > 1 ? 1 : alpha < 0 ? 0 : alpha;
                            var v = (byte) ((ovPtr[c] * 2 > 255) ? 255 : ovPtr[c] * 2);
                            bPtr[c] =  (byte) ((int)(v * alpha + bPtr[c+0] * (1-alpha)));
                            bPtr[c+1] =(byte) ((int)(v * alpha + bPtr[c+1] * (1-alpha)));
                            bPtr[c+2] =(byte) ((int)(v * alpha + bPtr[c+2] * (1-alpha)));
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

            var readWidth = Math.Min(vbmp.Width, hsbmp.Width);
            var readHeight = Math.Min(vbmp.Height, hsbmp.Height);

            var vdata = vbmp.LockBits(new Rectangle(0, 0, vbmp.Width, vbmp.Height), ImageLockMode.ReadOnly, vbmp.PixelFormat);
            var hsdata = hsbmp.LockBits(new Rectangle(0, 0, hsbmp.Width, hsbmp.Height), ImageLockMode.ReadWrite, hsbmp.PixelFormat);

            if (hsbmp.PixelFormat == PixelFormat.Format24bppRgb) {
                unsafe {
                    var hsPtr = (byte*)hsdata.Scan0.ToPointer();
                    var vPtr = (byte*)vdata.Scan0.ToPointer();

                    for (var y = 0; y < readHeight; y++) {
                        for (var x = 0; x < readWidth; x++) {
                            // TODO: Improve this
                            var c = ((hsdata.Stride / 3) * y + x) * 3;
                            var rgb = new byte[] { hsPtr[c], hsPtr[c + 1], hsPtr[c + 2] };
                            var hsv = rgb2hsv(rgb);
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
                    var hsPtr = (byte*)hsdata.Scan0.ToPointer();
                    var vPtr = (byte*)vdata.Scan0.ToPointer();

                    for (var y = 0; y < readHeight; y++) {
                        for (var x = 0; x < readWidth; x++) {
                            // TODO: Improve this
                            var c = ((hsdata.Stride / 4) * y + x) * 4;
                            var rgb = new byte[] { hsPtr[c+0], hsPtr[c + 1], hsPtr[c + 2] };
                            var hsv = rgb2hsv(rgb);
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
        /// <param name = "color"></param>
        /// <param name = "lineWidth"></param>
        /// <param name = "fixCrop"></param>
        public static void DrawLatLonLines(ref Bitmap bmp, GeoConverter gc, Color color, int lineWidth = 5, bool fixCrop = false) {
            var pen = new Pen(color, lineWidth);
            using (var graphics = Graphics.FromImage(bmp)) {
                float lastX;
                float lastY;
                for (var lat = gc.MinLatitude; lat < gc.MaxLatitude; lat += 10f) {
                    lastX = float.NaN;
                    lastY = float.NaN;
                    for (var lon = gc.MinLongitude; lon < gc.MaxLongitude; lon += 0.1f) {
                        var xy = gc.latlon2xy(lat, lon);

                        if (fixCrop) {
                            xy = new Tuple<int, int>(xy.Item1 - gc.CropLeft, xy.Item2);
                        }
                        if (!float.IsNaN(lastX) && !float.IsNaN(lastY)) {
                            graphics.DrawLine(pen, lastX, lastY, xy.Item1, xy.Item2);
                        }
                        lastX = xy.Item1;
                        lastY = xy.Item2;
                    }

                }
                for (var lon = gc.MinLongitude; lon < gc.MaxLongitude; lon += 10f) {
                    lastX = float.NaN;
                    lastY = float.NaN;
                    for (var lat = gc.MinLatitude; lat < gc.MaxLatitude; lat += 0.1f) {
                        var xy = gc.latlon2xy(lat, lon);

                        if (fixCrop) {
                            xy = new Tuple<int, int>(xy.Item1 - gc.CropLeft, xy.Item2);
                        }
                        if (!float.IsNaN(lastX) && !float.IsNaN(lastY)) {
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


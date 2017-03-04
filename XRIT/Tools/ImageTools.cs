using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace OpenSatelliteProject {
    public static class ImageTools {

        public static Bitmap ToFormat(this Bitmap orig, PixelFormat newFormat) {
            Bitmap newBmp = new Bitmap(orig.Width, orig.Height, newFormat);
            using (Graphics gr = Graphics.FromImage(newBmp)) {
                gr.DrawImage(orig, new Rectangle(0, 0, newBmp.Width, newBmp.Height));
            }
            return newBmp;
        }

        /// <summary>
        /// Applies a LUT to the indexed bitmap using the Index Pallete
        /// </summary>
        /// <param name="lut">Raw Lookup Table</param>
        /// <param name="bitmap">Bitmap to be applied</param>
        public static void ApplyLUT(byte[] lut, ref Bitmap bitmap) {
            if (bitmap.PixelFormat != PixelFormat.Format8bppIndexed) {
                throw new Exception("Must be a 8bpp Indexed file");
            }

            ColorPalette pal = bitmap.Palette;
            for(int i=0;i<=255;i++) {
                pal.Entries[i] = Color.FromArgb(lut[i*3], lut[i*3+1], lut[i*3+2]);
            }

            bitmap.Palette = pal;
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

            if (bitmap.PixelFormat == PixelFormat.Format8bppIndexed) {
                // Just apply to the palete.
                ColorPalette pal = bitmap.Palette;
                for(int i=0;i<=255;i++) {
                    Color c = pal.Entries[i];
                    pal.Entries[i] = Color.FromArgb(c.A, cLut[c.R], cLut[c.G], cLut[c.B]);
                }
                bitmap.Palette = pal;
            } else if (bitmap.PixelFormat == PixelFormat.Format24bppRgb) {
                var data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, bitmap.PixelFormat);
                int totalPoints = data.Stride * bitmap.Height * 3;
                unsafe {
                    byte* dPtr = (byte *)data.Scan0.ToPointer();
                    for (int c = 0; c<totalPoints; c++) {
                        byte d = dPtr[c];
                        dPtr[c] = cLut[d];
                    }
                }
                bitmap.UnlockBits(data);
            } else {
                throw new Exception("Unsuported bitmap pixel format");
            }
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
        /// Combines Value of VBmp to Hue/Saturation of HSBmp.
        /// The output will be at HSBmp
        /// </summary>
        /// <param name="hsbmp">Hue/Saturation Bitmap</param>
        /// <param name="vbmp">Value Bitmap</param>
        public static void CombineHStoV(ref Bitmap hsbmp, Bitmap vbmp) {
            if (vbmp.PixelFormat != PixelFormat.Format24bppRgb) {
                vbmp = vbmp.ToFormat(PixelFormat.Format24bppRgb);
            }

            if (hsbmp.PixelFormat != PixelFormat.Format24bppRgb) {
                throw new Exception("hsbmp format needs to be 24bpp RGB");
            }

            var vdata = vbmp.LockBits(new Rectangle(0, 0, vbmp.Width, vbmp.Height), ImageLockMode.ReadOnly, vbmp.PixelFormat);
            var hsdata = hsbmp.LockBits(new Rectangle(0, 0, hsbmp.Width, hsbmp.Height), ImageLockMode.ReadWrite, hsbmp.PixelFormat);

            unsafe {
                byte* hsPtr = (byte *)hsdata.Scan0.ToPointer();
                byte* vPtr = (byte*)vdata.Scan0.ToPointer();

                for (int y = 0; y < hsbmp.Height; y++) {
                    for (int x = 0; x < hsbmp.Width; x++) {
                        // TODO: Improve this
                        int c = ((hsdata.Stride / 3) * y + x) * 3;
                        byte[] rgb = new byte[] { hsPtr[c], hsPtr[c + 1], hsPtr[c + 2] };
                        float[] hsv = rgb2hsv(rgb);
                        hsv[2] = vPtr[c] / 255f;
                        rgb = hsv2rgb(hsv);
                        hsPtr[c+0] = rgb[0];
                        hsPtr[c+1] = rgb[1];
                        hsPtr[c+2] = rgb[2];
                    }
                }
            }

            vbmp.UnlockBits(vdata);
            hsbmp.UnlockBits(hsdata);
        }
    }
}


using System;
using DotSpatial.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;

namespace OpenSatelliteProject.Geo {
    /// <summary>
    /// Map Drawer Class
    /// You can get ShapeFiles from: http://www.naturalearthdata.com
    /// Just load the .shp file
    /// </summary>
    public class MapDrawer {
        private readonly Shapefile shapeFile;

        public Shapefile ShapeFile {
            get {
                return shapeFile;
            }
        }

        public MapDrawer(string shapeFile) {
            this.shapeFile = Shapefile.OpenFile(shapeFile);
        }

        ~MapDrawer() {
            shapeFile.Close();
        }

        /// <summary>
        /// Generates the land map using GeoConverter
        /// </summary>
        /// <returns>The land map.</returns>
        /// <param name="gc">Gc.</param>
        /// <param name="width">Width.</param>
        /// <param name="height">Height.</param>
        /// <param name="fixCrop">If set to <c>true</c> fix crop.</param>
        public Bitmap GenerateLandMap(GeoConverter gc, int width, int height, bool fixCrop = false) {
            var bmp = new Bitmap (width, height, PixelFormat.Format24bppRgb);
            Brush bgBrush = new SolidBrush (Color.Black);
            Brush polyBrush = new SolidBrush (Color.White);
            using (var graphics = Graphics.FromImage (bmp)) {
                graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                graphics.FillRectangle (bgBrush, 0, 0, width, height);
                lock (shapeFile) {  // TODO: This is BAD, SO BAD, PLEASE FIX ME
                                    // Thats because for some reason running this in multiple
                                    // threads is causing Features to be modified (wtf?)
                    foreach (var f in shapeFile.Features.ToList()) {
                        for (int i = 0; i < f.NumGeometries; i++) {
                            var geom = f.GetBasicGeometryN (i);
                            var k = geom.Coordinates;
                            var points = new List<PointF> ();
                            foreach (var z in k) {
                                float lon = (float)z.X;
                                float lat = (float)z.Y;
                                var xy = gc.latlon2xy (lat, lon);
                                float cx = (float)xy.Item1;
                                float cy = (float)xy.Item2;

                                if (fixCrop) {
                                    cx -= gc.CropLeft;
                                }
                                points.Add (new PointF (cx, cy));
                            }

                            // Search if any of the points are inside the image
                            foreach (var p in points) {
                                if (p.X > 0 && p.X < bmp.Width && p.Y > 0 && p.Y < bmp.Height) {
                                    graphics.FillPolygon(polyBrush, points.ToArray());
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            return bmp;
        }

        private void OldDrawMap(ref Bitmap bmp, GeoConverter gc, Color color, int lineWidth = 5, bool fixCrop = false) {
            Pen pen = new Pen(color, lineWidth);
            float lastX = float.NaN;
            float lastY = float.NaN;

            using (var graphics = Graphics.FromImage(bmp)) {
                graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                lock(shapeFile) {   // TODO: This is BAD, SO BAD, PLEASE FIX ME
                                    // Thats because for some reason running this in multiple
                                    // threads is causing Features to be modified (wtf?)
                    foreach (var f in shapeFile.Features.ToList()) {
                        for (int i = 0; i < f.NumGeometries; i++) {
                            var geom = f.GetBasicGeometryN (i);
                            var k = geom.Coordinates;

                            lastX = float.NaN;
                            lastY = float.NaN;

                            foreach (var z in k) {
                                float lon = (float)z.X;
                                float lat = (float)z.Y;
                                if (lat < gc.MaxLatitude && lat > gc.MinLatitude && lon < gc.MaxLongitude && lon > gc.MinLongitude) {
                                    var xy = gc.latlon2xy (lat, lon);
                                    float cx = (float)xy.Item1;
                                    float cy = (float)xy.Item2;

                                    if (fixCrop) {
                                        cx -= gc.CropLeft;
                                    }
                                    if (
                                        (!float.IsNaN(lastX) && !float.IsNaN(lastY)) &&
                                        (cx > 0 && cy > 0) &&
                                        (cx < bmp.Width && cy < bmp.Height) &&
                                        (lastX > 0 && lastY > 0) &&
                                        (lastX < bmp.Width && lastY < bmp.Height)) {
                                        graphics.DrawLine (pen, lastX, lastY, cx, cy);
                                    }
                                    lastX = cx;
                                    lastY = cy;
                                }
                            }
                        }
                    }
                }
            }
        }

        private void NewDrawMap(ref Bitmap bmp, GeoConverter gc, Color color, int lineWidth = 5, bool fixCrop = false, bool useOldDrawMap = false) {
            Pen pen = new Pen(color, lineWidth);
            using (var graphics = Graphics.FromImage (bmp)) {
                graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                lock (shapeFile) {  // TODO: This is BAD, SO BAD, PLEASE FIX ME
                    // Thats because for some reason running this in multiple
                    // threads is causing Features to be modified (wtf?)
                    foreach (var f in shapeFile.Features.ToList()) {
                        for (int i = 0; i < f.NumGeometries; i++) {
                            var geom = f.GetBasicGeometryN (i);
                            var k = geom.Coordinates;
                            List<PointF> points = new List<PointF> ();
                            foreach (var z in k) {
                                float lon = (float)z.X;
                                float lat = (float)z.Y;
                                var xy = gc.latlon2xy (lat, lon);
                                float cx = (float)xy.Item1;
                                float cy = (float)xy.Item2;

                                if (fixCrop) {
                                    cx -= gc.CropLeft;
                                }
                                points.Add (new PointF (cx, cy));
                            }

                            // Search if any of the points are inside the image
                            foreach (var p in points) {
                                if (p.X > 0 && p.X < bmp.Width && p.Y > 0 && p.Y < bmp.Height) {
                                    graphics.DrawPolygon(pen, points.ToArray());
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Draws the Map using the loaded Shapefile on bitmap.
        /// </summary>
        /// <param name="bmp">Bitmap to be draw</param>
        /// <param name="gc">Initialized GeoConverter</param>
        /// <param name="color">Color of the lines</param>
        /// <param name="lineWidth">Thickness of the Lines</param>
        public void DrawMap(ref Bitmap bmp, GeoConverter gc, Color color, int lineWidth = 5, bool fixCrop = false, bool useOldDrawMap = false) {
            if (useOldDrawMap) {
                OldDrawMap (ref bmp, gc, color, lineWidth, fixCrop);
            } else {
                NewDrawMap (ref bmp, gc, color, lineWidth, fixCrop);
            }
        }
    }
}


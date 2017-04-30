using System;
using DotSpatial.Data;
using System.Drawing;

namespace OpenSatelliteProject.Geo {
    public class MapDrawer {
        private Shapefile shapeFile;
        
        public MapDrawer(string shapeFile) {
            this.shapeFile = Shapefile.OpenFile(shapeFile);
        }

        ~MapDrawer() {
            shapeFile.Close();
        }

        public void DrawMap(ref Bitmap bmp, GeoConverter gc, Color color, int lineWidth = 5) {
            Pen pen = new Pen(color, lineWidth);
            float lastX = -1;
            float lastY = -1;
            using (var graphics = Graphics.FromImage(bmp)) {
                foreach (var f in shapeFile.Features) {
                    for (int i = 0; i < f.NumGeometries; i++) {
                        var geom = f.GetBasicGeometryN(i);
                        var k = geom.Coordinates;

                        lastX = -1;
                        lastY = -1;

                        foreach (var z in k) {
                            float lon = (float)z.X;
                            float lat = (float)z.Y;
                            if (lat < gc.MaxLatitude && lat > gc.MinLatitude && lon < gc.MaxLongitude && lon > gc.MinLongitude) {
                                var xy = gc.latlon2xy(lat, lon);
                                float cx = (float)xy.Item1;
                                float cy = (float)xy.Item2;
                                if (
                                    ( lastX != -1 && lastY != -1 ) && 
                                    ( cx > 0 && cy > 0 ) &&
                                    ( cx < bmp.Width && cy < bmp.Height ) &&
                                    ( lastX > 0 && lastY > 0) &&
                                    ( lastX < bmp.Width && lastY < bmp.Height)
                                ){
                                    graphics.DrawLine(pen, lastX, lastY, cx, cy);
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
}


using System;

namespace OpenSatelliteProject.Geo {
    /// <summary>
    /// Class to convert Pixels to LatLon or the inverse using LRIT Geolocation Parameters
    /// </summary>
    public class GeoConverter {
        private int coff;
        private int loff;
        private float cfac;
        private float lfac;
        private float satelliteLongitude;
        private float aspectRatio;
        private bool fixAspect;
        private int cropLeft;

        public int ColumnOffset { get { return coff; } }
        public int LineOffset { get { return loff; } } 
        public int CropLeft { get { return cropLeft; } }

        /// <summary>
        /// Maximum Visible Latitude
        /// </summary>
        /// <value>The max latitude.</value>
        public float MaxLatitude { 
            get {
                return 79;
            }
        }

        /// <summary>
        /// Minimum Visible Latitude
        /// </summary>
        /// <value>The minimum latitude.</value>
        public float MinLatitude { 
            get {
                return -79;
            }
        }

        /// <summary>
        /// Maximum visible Longitude
        /// </summary>
        /// <value>The max longitude.</value>
        public float MaxLongitude {
            get {
                return satelliteLongitude + 79;
            }
        }

        /// <summary>
        /// Minimum visible latitude
        /// </summary>
        /// <value>The minimum longitude.</value>
        public float MinLongitude {
            get {
                return satelliteLongitude - 79;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenSatelliteProject.Geo.GeoConverter"/> class.
        /// </summary>
        /// <param name="satelliteLongitude">Satellite longitude.</param>
        /// <param name="coff">Column Offset</param>
        /// <param name="loff">Line Offset</param>
        /// <param name="cfac">Column Scaling Factor</param>
        /// <param name="lfac">Line Scaling Factor</param>
        public GeoConverter(float satelliteLongitude, int coff, int loff, float cfac, float lfac, bool fixAspect=false, int imageWidth = 0) {
            this.satelliteLongitude = satelliteLongitude;
            this.coff = coff;
            this.loff = loff;
            this.cfac = cfac;
            this.lfac = lfac;
            this.aspectRatio = cfac / lfac;
            this.fixAspect = fixAspect;
            this.cropLeft = (int) coff - Math.Min(imageWidth - coff, coff);
        }

        /// <summary>
        /// Converts Latitude/Longitude to Pixel X/Y
        /// </summary>
        /// <param name="lat">Latitude in Degrees</param>
        /// <param name="lon">Longitude in Degrees</param>
        public Tuple<int, int> latlon2xy(float lat, float lon) {
            var xy = GeoTools.lonlat2xy(satelliteLongitude, GeoTools.deg2rad(lon), GeoTools.deg2rad(lat), coff, cfac, loff, lfac);
            if (this.fixAspect) {
                xy = new Tuple<int, int>(xy.Item1, (int) (xy.Item2 * this.aspectRatio));
            }

            return xy;
        }

        /// <summary>
        /// Converts Pixel X/Y to Latitude/Longitude
        /// </summary>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        public Tuple<float, float> xy2latlon(int x, int y) {
            var radCoord = GeoTools.xy2lonlat(satelliteLongitude, x, y, coff, cfac, loff, lfac);
            return new Tuple<float, float>(GeoTools.rad2deg(radCoord.Item2), GeoTools.rad2deg(radCoord.Item1));
        }
    }
}


using System;

namespace OpenSatelliteProject.Geo {
    public class GeoConverter {
        private int coff;
        private int loff;
        private float cfac;
        private float lfac;
        private float satelliteLongitude;

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

        public GeoConverter(float satelliteLongitude, int coff, int loff, float cfac, float lfac) {
            this.satelliteLongitude = satelliteLongitude;
            this.coff = coff;
            this.loff = loff;
            this.cfac = cfac;
            this.lfac = lfac;
        }

        public Tuple<int, int> latlon2xy(float lat, float lon) {
            return GeoTools.lonlat2xy(satelliteLongitude, GeoTools.deg2rad(lon), GeoTools.deg2rad(lat), coff, cfac, loff, lfac);
        }

        public Tuple<float, float> xy2latlon(int x, int y) {
            var radCoord = GeoTools.xy2lonlat(satelliteLongitude, x, y, coff, cfac, loff, lfac);
            return new Tuple<float, float>(GeoTools.rad2deg(radCoord.Item1), GeoTools.rad2deg(radCoord.Item2));
        }
    }
}


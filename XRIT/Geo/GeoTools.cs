using System;

namespace OpenSatelliteProject.Geo {
    /// <summary>
    /// Geographic Conversion Tools
    /// Based on: http://www.cgms-info.org/documents/pdf_cgms_03.pdf
    /// </summary>
    public static class GeoTools {

        public static readonly float MAXLON = GeoTools.deg2rad(75);
        public static readonly float MINLON = GeoTools.deg2rad(-75);
        public static readonly float MAXLAT = GeoTools.deg2rad(79);
        public static readonly float MINLAT = GeoTools.deg2rad(-79);

        public static readonly float radiusPoles = 6356.7523f;
        public static readonly float radiusEquator = 6378.1370f;
        public static readonly float vehicleDistance = 42142.5833f;

        public static float deg2rad(float deg) {
            return (float) (deg * Math.PI / 180);
        }

        public static float rad2deg(float rad) {
            return (float) (rad * 180 / Math.PI);
        }

        /// <summary>
        /// Converts Latitude/Longitude to Pixel X/Y on image
        /// </summary>
        /// <param name="satLongitude">Satellite Longitude</param>
        /// <param name="lon">Longitude in Radians</param>
        /// <param name="lat">Latitude in Radians</param>
        /// <param name="coff">Column Offset</param>
        /// <param name="cfac">Column Scaling Factor</param>
        /// <param name="loff">Line Offset</param>
        /// <param name="lfac">Line Scaling Factor</param>
        public static Tuple<int, int> lonlat2xy(float satLongitude, float lon, float lat, int coff, float cfac, int loff, float lfac) {
            var sub_lon = deg2rad(satLongitude);
            var rep = (radiusPoles * radiusPoles) / (radiusEquator * radiusEquator);
            lon -= sub_lon;

            lon = Math.Min(Math.Max(lon, MINLON), MAXLON);
            lat = Math.Min(Math.Max(lat, MINLAT), MAXLAT);

            var psi = Math.Atan(rep * Math.Tan(lat));
            var re  = radiusPoles / (Math.Sqrt( 1 - ( 1 - rep) * Math.Cos(psi) * Math.Cos(psi)));
            var r1 = vehicleDistance - re * Math.Cos(psi) * Math.Cos(lon);
            var r2 = -1 * re * Math.Cos(psi) * Math.Sin(lon);
            var r3 = re * Math.Sin(psi);

            var rn = Math.Sqrt ( r1 * r1 + r2 * r2 + r3 * r3 );
            var x = Math.Atan(-1 * r2 / r1);
            var y = Math.Asin(-1 * r3 / rn);
            x = rad2deg((float)x);
            y = rad2deg((float)y);

            var c = coff + (int)(x * (float)(cfac) / 0x10000);
            var l = loff + (int)(y * (float)(lfac) / 0x10000);

            return new Tuple<int, int>(c, l);
        }

        public static Tuple<float, float> lonlat2xyf(float satLongitude, float lon, float lat, int coff, float cfac, int loff, float lfac) {
            var sub_lon = deg2rad(satLongitude);
            var rep = (radiusPoles * radiusPoles) / (radiusEquator * radiusEquator);
            lon -= sub_lon;

            lon = Math.Min(Math.Max(lon, MINLON), MAXLON);
            lat = Math.Min(Math.Max(lat, MINLAT), MAXLAT);

            var psi = Math.Atan(rep * Math.Tan(lat));
            var re  = radiusPoles / (Math.Sqrt( 1 - ( 1 - rep) * Math.Cos(psi) * Math.Cos(psi)));
            var r1 = vehicleDistance - re * Math.Cos(psi) * Math.Cos(lon);
            var r2 = -1 * re * Math.Cos(psi) * Math.Sin(lon);
            var r3 = re * Math.Sin(psi);

            var rn = Math.Sqrt ( r1 * r1 + r2 * r2 + r3 * r3 );
            var x = Math.Atan(-1 * r2 / r1);
            var y = Math.Asin(-1 * r3 / rn);
            x = rad2deg((float)x);
            y = rad2deg((float)y);

            var c = coff + (x * (float)(cfac) / 0x10000);
            var l = loff + (y * (float)(lfac) / 0x10000);

            return new Tuple<float, float>((float)c, (float)l);
        }

        /// <summary>
        /// Converts Pixel X/Y to Latitude / Longitude
        /// </summary>
        /// <param name="satelliteLon">Satellite Longitude</param>
        /// <param name="c">Column (X)</param>
        /// <param name="l">Line (Y)</param>
        /// <param name="coff">Column Offset</param>
        /// <param name="cfac">Column Scaling Factor</param>
        /// <param name="loff">Line Offset</param>
        /// <param name="lfac">Line Scaling Factor</param>
        public static Tuple<float, float> xy2lonlat(float satelliteLon, int c, int l, int coff, float cfac, int loff, float lfac) {
            var q2 = (radiusEquator * radiusEquator) / (radiusPoles * radiusPoles);
            var d2 = vehicleDistance * vehicleDistance - radiusEquator * radiusEquator;
            var sub_lon = deg2rad(satelliteLon);

            var x = (c - coff) * 0x10000 / cfac;
            var y = (l - loff) * 0x10000 / lfac;

            var cosx = Math.Cos(deg2rad(x));
            var cosy = Math.Cos(deg2rad(y));
            var sinx = Math.Sin(deg2rad(x));
            var siny = Math.Sin(deg2rad(y));

            var a1 = vehicleDistance * vehicleDistance * cosx * cosx * cosy * cosy;
            var a2 = (cosy * cosy + q2 * siny * siny) * d2;
            if (a1 < a2) {
                return new Tuple<float, float>(0, 0);
            }

            var sd = Math.Sqrt(a1 - a2);

            var sn = ( vehicleDistance * cosx * cosy - sd ) / (cosy * cosy + q2 * siny * siny);
            var s1 = vehicleDistance - sn * cosx * cosy;
            var s2 = sn * sinx * cosy;
            var s3 = -1 * sn * siny;
            var sxy = Math.Sqrt(s1 * s1 + s2 * s2);
            var lat = 0f;
            var lon = 0f;

            if (s1 == 0) {
                lon = s2 > 0 ? deg2rad(90) + sub_lon : deg2rad(-90) + sub_lon;
            } else {
                lon = (float) Math.Atan(s2 / s1) + sub_lon;
            }

            if (sxy == 0) {
                lat = q2 * s3 > 0 ? deg2rad(90) : deg2rad(-90);
            } else {
                lat = (float) Math.Atan(q2 * s3 / sxy);
            }

            return new Tuple<float, float>(lon, lat);
        }
    }
}


using System;

namespace Trajet_Tram.Helpers
{
    public static class DistanceLatLonKm_Utils
    {
        public static bool IsAroundPosition(double currentLat, double currentLon, double poslat, double poslon, double iRadius_metre)
        {
            bool bRes = true;

            DistanceLatLonKm_Utils.MapPoint position = new DistanceLatLonKm_Utils.MapPoint { Latitude = poslat, Longitude = poslon };
            DistanceLatLonKm_Utils.BoundingBox boundingBox = DistanceLatLonKm_Utils.GetBoundingBox(position, iRadius_metre);

            if (currentLon < boundingBox.MinPoint.Longitude || currentLon > boundingBox.MaxPoint.Longitude)
                bRes = false;

            if (currentLat < boundingBox.MinPoint.Latitude || currentLat > boundingBox.MaxPoint.Latitude)
                bRes = false;

            return bRes;
        }

        public static bool IsInIncludedInSearchZone(double currentLat, double currentLon, double SearchZone_TopRight_Lat, double SearchZone_TopRight_Lon, double SearchZone_BottomLeft_Lat, double SearchZone_BottomLeft_Lon)
        {
            bool bRes = false;

            DistanceLatLonKm_Utils.MapPoint TopRight = new DistanceLatLonKm_Utils.MapPoint { Latitude = SearchZone_TopRight_Lat, Longitude = SearchZone_TopRight_Lon };
            DistanceLatLonKm_Utils.MapPoint BottomLeft = new DistanceLatLonKm_Utils.MapPoint { Latitude = SearchZone_BottomLeft_Lat, Longitude = SearchZone_BottomLeft_Lon };
            DistanceLatLonKm_Utils.BoundingBox boundingBox = new DistanceLatLonKm_Utils.BoundingBox { MinPoint = BottomLeft, MaxPoint = TopRight };

            if (currentLat < boundingBox.MaxPoint.Latitude && currentLon < boundingBox.MaxPoint.Longitude &&
                currentLat > boundingBox.MinPoint.Latitude || currentLon > boundingBox.MinPoint.Longitude)
                bRes = true;

            return bRes;
        }

        //Util library to calculate distances in Km from latitude and longitude coordinates
        private class MapPoint
        {
            public double Longitude { get; set; } // In Degrees
            public double Latitude { get; set; } // In Degrees
        }

        private class BoundingBox
        {
            public MapPoint MinPoint { get; set; }
            public MapPoint MaxPoint { get; set; }
        }

        // Semi-axes of WGS-84 geoidal reference
        private const double WGS84_a = 6378137.0; // Major semiaxis [m]
        private const double WGS84_b = 6356752.3; // Minor semiaxis [m]

        // 'halfSideInKm' is the half length of the bounding box you want in kilometers.
        //Get a bounding box aroung a position
        private static BoundingBox GetBoundingBox(MapPoint point, double halfSideInMetre)
        {
            // Bounding box surrounding the point at given coordinates,
            // assuming local approximation of Earth surface as a sphere
            // of radius given by WGS84
            double lat = Deg2rad(point.Latitude);
            double lon = Deg2rad(point.Longitude);

            // Radius of Earth at given latitude
            double radius = WGS84EarthRadius(lat);
            // Radius of the parallel at given latitude
            double pradius = radius * Math.Cos(lat);

            double latMin = lat - halfSideInMetre / radius;
            double latMax = lat + halfSideInMetre / radius;
            double lonMin = lon - halfSideInMetre / pradius;
            double lonMax = lon + halfSideInMetre / pradius;

            return new BoundingBox
            {
                MinPoint = new MapPoint { Latitude = Rad2deg(latMin), Longitude = Rad2deg(lonMin) },
                MaxPoint = new MapPoint { Latitude = Rad2deg(latMax), Longitude = Rad2deg(lonMax) }
            };
        }

        // degrees to radians
        private static double Deg2rad(double degrees)
        {
            return Math.PI * degrees / 180.0;
        }

        // radians to degrees
        private static double Rad2deg(double radians)
        {
            return 180.0 * radians / Math.PI;
        }

        // Earth radius at a given latitude, according to the WGS-84 ellipsoid [m]
        private static double WGS84EarthRadius(double lat)
        {
            // http://en.wikipedia.org/wiki/Earth_radius
            double An = WGS84_a * WGS84_a * Math.Cos(lat);
            double Bn = WGS84_b * WGS84_b * Math.Sin(lat);
            double Ad = WGS84_a * Math.Cos(lat);
            double Bd = WGS84_b * Math.Sin(lat);
            return Math.Sqrt((An * An + Bn * Bn) / (Ad * Ad + Bd * Bd));
        }
    }
}

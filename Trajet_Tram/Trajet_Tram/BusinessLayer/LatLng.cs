namespace Trajet_Tram.BusinessLayer
{
    public class LatLng
    {

        public double m_Lat { get; set; }
        public double m_Lon { get; set; }

        public LatLng(double lat, double lon)
        {
            m_Lat = lat;
            m_Lon = lon;
        }
    }
}

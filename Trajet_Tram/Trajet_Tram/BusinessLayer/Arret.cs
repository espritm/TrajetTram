using System;
using SQLite;
using Trajet_Tram.Helpers;
using System.Text;
using System.Collections.Generic;

namespace Trajet_Tram.BusinessLayer
{
    public class Arret
    {
        [PrimaryKey, AutoIncrement]
        public int m_iLocalID { get; set; }

        public int m_iLocalIDtrajet { get; set; }
        public string m_sName { get; set; }
        public double m_dPente { get; set; }
        public string m_sTime { get; set; }
        public double m_lat { get; set; }
        public double m_lon { get; set; }
        public string m_sStopTime { get; set; }
        public bool m_bIsStopPoint { get; set; }
        public string m_sRadioCanal { get; set; }
        public string m_strajetRefNumber { get; set; }
        public string m_sTractionDef { get; set; }
        public string m_sLocoType { get; set; }
        public string m_sWeight { get; set; }
        

        public Arret()
        {
        }
        

        public Arret(string sName, string PK, string sCodeChantier, string sVitesseMax, double dDeclivite, string sTime, string sRestartTime, string sStopTime, string sCodeCHSecond, double lat, double lon, double dRampe, bool isStopPoint, string sStopType, string sRadioCanal, bool bHasBeenCreatedByDriverDuringPreparation, string sAddonRef = "")
        {
            m_sName = sName;
            m_dPente = dDeclivite;
            m_sTime = sTime;
            m_sStopTime = sStopTime;
            m_lat = lat;
            m_lon = lon;
            m_bIsStopPoint = isStopPoint;
            m_sRadioCanal = sRadioCanal;
        }

        public TimeSpan GetNbMinutsAvance(DateTime dateOftrajet)
        {
            TimeSpan res = TimeSpan.FromMinutes(0);

            string sArriveTheorique = m_sStopTime;

            if (sArriveTheorique == null || sArriveTheorique == "")
                sArriveTheorique = m_sTime;

            if (sArriveTheorique == null || sArriveTheorique == "")
                return res;

            string[] tHoursMinuts = sArriveTheorique?.Split(':');

            if (tHoursMinuts == null || tHoursMinuts.Length < 2)
                return res;

            DateTime dateArriveeTheorique = new DateTime(dateOftrajet.Year, dateOftrajet.Month, dateOftrajet.Day, int.Parse(tHoursMinuts[0]), int.Parse(tHoursMinuts[1]), 0);
            DateTime dateNow = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, 0);

            res = dateArriveeTheorique.Subtract(DateTime.Now);

            return res;
        }

        public bool IsAroundPosition(double latitude, double longitude)
        {
            //return true if Arret is 40m around latitude/longitude given
            return DistanceLatLonKm_Utils.IsAroundPosition(m_lat, m_lon, latitude, longitude, Settings.MinimumRadiusToDetectStop);
        }

        public bool IsAroundPosition_TESTAUTOONLY(double latitude, double longitude, int iRadius)
        {
            //return true if Arret is 40m around latitude/longitude given
            return DistanceLatLonKm_Utils.IsAroundPosition(m_lat, m_lon, latitude, longitude, iRadius);
        }

        public override string ToString()
        {
            Trajet trajet = LocalDatabase.Get().Gettrajet(null, m_iLocalIDtrajet);

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("Arret {");
            sb.AppendLine("   m_iLocalID = " + m_iLocalID + ", ");
            sb.AppendLine("   m_iLocalIDtrajet = " + m_iLocalIDtrajet + ", ");
            sb.AppendLine("   trajet's ref number = " + trajet?.m_sRefNumber ?? "null" + ", ");
            sb.AppendLine("   m_sName = " + m_sName + ", ");
            sb.AppendLine("   m_dPente = " + m_dPente + ", ");
            sb.AppendLine("   m_sTime = " + m_sTime + ", ");
            sb.AppendLine("   m_lat = " + m_lat + ", ");
            sb.AppendLine("   m_lon = " + m_lon + ", ");
            sb.AppendLine("   m_bIsStopPoint = " + m_bIsStopPoint + ", ");
            sb.AppendLine("   m_sStopTime = " + m_sStopTime + ", ");
            sb.AppendLine("   m_sRadioCanal = " + m_sRadioCanal + ", ");
            sb.AppendLine("}");

            return sb.ToString();
        }
    }
}

using Trajet_Tram.Exceptions;
using Trajet_Tram.Helpers;
using Trajet_Tram.Interface;
using Plugin.Connectivity;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trajet_Tram.BusinessLayer
{
    public class Trajet
    {
        [PrimaryKey, AutoIncrement]
        public int m_iLocalID { get; set; }

        public string m_sRefNumber { get; set; }

        public string m_sLocationStart { get; set; }

        public string m_sLocationEnd { get; set; }

        public string m_sTimeStart { get; set; }

        public string m_sTimeEnd { get; set; }
        
        public DateTime m_datetrajet { get; set; }

        public string m_sTractionDef { get; set; }

        public string m_sTrainType { get; set; }
        public string m_sLocoType { get; set; }
        public string m_sWeight { get; set; }

        public trajetStatus m_Status { get; set; }

        public bool m_bHasBeenPrepared { get; set; }

        [Ignore]
        public List<Arret> m_lsarret { get; set; }
        
        


        public Trajet()
        {
            m_lsarret = new List<Arret>();
            m_Status = trajetStatus.New;
            m_datetrajet = DateTime.MinValue;
        }
        

        private void Init(string sName, string sStartCity, string sEndCity, string sStartAt, string sEndAt, string sDate, List<Arret> lsItems, string sTractionDef, string sTrainType, List<string> lsDocNames, string sDummyData, string sLocoType, string sWeight)
        {
            m_sRefNumber = sName;
            m_sLocationStart = sStartCity;
            m_sLocationEnd = sEndCity;
            m_sTimeStart = sStartAt;
            m_sTimeEnd = sEndAt;
            m_sTractionDef = sTractionDef;
            m_sTrainType = sTrainType;
            m_sLocoType = sLocoType;
            m_sWeight = sWeight;

            m_Status = trajetStatus.New; //Définir la logique métier...
            m_datetrajet = DateTime.Parse(sDate);

            m_lsarret = new List<Arret>();

            foreach (Arret item in lsItems)
                m_lsarret.Add(item);

            /*foreach (string sFileName in lsDocNames)
                m_lsDocumentPDF.Add(new EhouatDocument(sFileName));*/
        }
        
        public static async Task<List<Trajet>> GetAlltrajetsToShowOnScreen(IFileAccessManager logFileManager)
        {
            return await Task.Factory.StartNew(() =>
            {
                //Remove all old trajets (2 days or more)
                List<Trajet> lstrajetsToShow = LocalDatabase.Get().GetAlltrajetInDatabase(logFileManager);
                
                foreach (Trajet m in lstrajetsToShow)
                    if (m.m_datetrajet.Date < DateTime.Now.Date.AddDays(-2))
                        LocalDatabase.Get().RemovetrajetAndItsarret(logFileManager, m);

                //Get the list of trajet to show on screen
                lstrajetsToShow = LocalDatabase.Get().GetAlltrajetInDatabase(logFileManager);

                return lstrajetsToShow;
            });
        }

        private static Trajet GettrajetDetailsToShowOnScreen(IFileAccessManager logFileManager, Trajet trajet)
        {
            if (trajet == null)
                return null;

            List<Arret> lsarret = LocalDatabase.Get().GetArretOftrajet(logFileManager, trajet);

            trajet.m_lsarret = new List<Arret>();
            

            if (lsarret != null && lsarret.Count > 0)
                trajet.m_lsarret.AddRange(lsarret);
            
            return trajet;
        }

        public static async Task<Trajet> GettrajetDetailsToShowOnScreen(IFileAccessManager logFileManager, int itrajetLocalID)
        {
            return await Task.Factory.StartNew(() =>
            {
                Trajet res = LocalDatabase.Get().Gettrajet(logFileManager, itrajetLocalID);

                return GettrajetDetailsToShowOnScreen(logFileManager, res);
            });
        }

        public string GetLibelleHourMinutLeftBeforeStart()
        {
            if (m_lsarret == null || m_lsarret.Count == 0)
                return "";

            Arret starting = m_lsarret[m_lsarret.Count - 1];

            string[] tHoursMinuts = starting.m_sTime?.Split('.');

            if (tHoursMinuts == null || tHoursMinuts.Length < 2)
                return "";

            DateTime dateStart = new DateTime(m_datetrajet.Year, m_datetrajet.Month, m_datetrajet.Day, int.Parse(tHoursMinuts[0]), int.Parse(tHoursMinuts[1]), 0);

            if (dateStart < DateTime.Now)
                return "";

            TimeSpan diff = dateStart - DateTime.Now;
            
            string sRes = "";
            if (diff.TotalDays > 1)
                sRes += (int)diff.TotalDays + " jour ";
            if (diff.TotalHours > 1)
                sRes += (int)diff.Hours + " h ";
            if (diff.TotalMinutes > 0)
                sRes += (int)diff.Minutes + " min ";

            return sRes;
        }

        public void Prepare(IFileAccessManager logFileManager)
        {
            m_bHasBeenPrepared = true;

            LocalDatabase.Get().Updatetrajet(logFileManager, this);
        }

        public void GetArretFromLatLon(double latitude, double longitude, out Arret m_currentArret, out Arret m_nextArret)
        {
            //if (m_lsarret.Count > 0)
            m_currentArret = m_lsarret[m_lsarret.Count - 1];
            //if (m_lsarret.Count > 1)
                m_nextArret = m_lsarret[m_lsarret.Count - 2];

            int iIndexCurrentArret = m_lsarret.Count - 1;

            foreach (Arret pointPassage in m_lsarret)
            {
                //If p is X Xmeters around position (depending on Settings)
                if (pointPassage.IsAroundPosition(latitude, longitude))
                {
                    iIndexCurrentArret = m_lsarret.FindIndex(p => p == pointPassage);

                    m_currentArret = pointPassage;
                    if (iIndexCurrentArret > 0)
                    {
                        m_nextArret = m_lsarret[iIndexCurrentArret - 1];
                    }
                    else
                    {
                        m_nextArret = null;
                    }
                    break;
                }
            }

            //Remove past stops if needed.... Don't do it here, it's done in the adapter
            int iRemoveFromIndex = iIndexCurrentArret + 1;
            int iRemoveCount = m_lsarret.Count - iRemoveFromIndex;
            if (iRemoveFromIndex < m_lsarret.Count)
                m_lsarret.RemoveRange(iRemoveFromIndex, iRemoveCount);
        }
        

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("trajet {");
            sb.AppendLine("   m_iLocalID = " + m_iLocalID + ", ");
            sb.AppendLine("   m_sRefNumber = " + m_sRefNumber + ", ");
            sb.AppendLine("   m_sLocationStart = " + m_sLocationStart + ", ");
            sb.AppendLine("   m_sLocationEnd = " + m_sLocationEnd + ", ");
            sb.AppendLine("   m_sTimeStart = " + m_sTimeStart + ", ");
            sb.AppendLine("   m_sTimeEnd = " + m_sTimeEnd + ", ");
            sb.AppendLine("   m_datetrajet = " + m_datetrajet == null ? "null" : m_datetrajet.ToString("d") + ", ");
            sb.AppendLine("   m_sTractionDef = " + m_sTractionDef + ", ");
            sb.AppendLine("   m_sTrainType = " + m_sTrainType + ", ");
            sb.AppendLine("   m_sLocoType = " + m_sLocoType + ", ");
            sb.AppendLine("   m_sWeight = " + m_sWeight + ", ");
            sb.AppendLine("   m_Status = " + m_Status + ", ");
            sb.AppendLine("   m_bHasBeenPrepared = " + m_bHasBeenPrepared + ", ");
            sb.AppendLine("   m_lsarret = " + m_lsarret == null ? "null" : "not null, count = "  + m_lsarret.Count + ", ");
            sb.AppendLine("}");

            return sb.ToString();
        }
    }

    public enum trajetStatus
    {
        New,
        Preparing,
        ReadyToGo
    }
}

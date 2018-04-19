using Trajet_Tram.Helpers;
using Plugin.Geolocator.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trajet_Tram.BusinessLayer
{
    public class AltiManager
    {
        //Calculate altitude with 4 GPS points
        private static readonly int NbGpsPointsToCalcOneAltitude = 4;

        private List<AltiPoint> m_lsPoints = new List<AltiPoint>();


        public void AddPoint(Position pos)
        {
            AltiPoint newPoint = new AltiPoint
            {
                m_dAltitude = pos.Altitude,
                m_dAltitudeAccuracy = pos.AltitudeAccuracy,
                m_datePoint = pos?.Timestamp.DateTime ?? DateTime.Now
            };

            //Do not add the point if altitude is 0
            if (newPoint.m_dAltitude <= 0)
                return;

            //Do not add the point if another point with the same Altitude and same Time is already registered
            if (m_lsPoints.Find(p => p.m_dAltitude == newPoint.m_dAltitude && p.m_datePoint == newPoint.m_datePoint) != null)
                return;
            
            //Create a circular array : add latest point to the end, and remove first point when there is more than 8 points in the array. 
            //This in order to calculate altitude average of 4 points at n0 and n1. 
            m_lsPoints.Add(newPoint);

            while (m_lsPoints.Count > NbGpsPointsToCalcOneAltitude * 2)
                m_lsPoints.RemoveAt(0);
        }

        public Pente GetCurrentPente()
        {
            try
            {
                //If the circular array is not full : return.
                if (m_lsPoints.Count < NbGpsPointsToCalcOneAltitude * 2)
                    return Pente.unknown;

                //Get the NbGpsPoints_ToCalcOneAltitude first points of the circular array
                List<AltiPoint> ls_n0 = m_lsPoints.GetRange(0, NbGpsPointsToCalcOneAltitude);

                //Get the NbGpsPoints_ToCalcOneAltitude latest points of the circular array
                List<AltiPoint> ls_n1 = m_lsPoints.GetRange(NbGpsPointsToCalcOneAltitude, NbGpsPointsToCalcOneAltitude);

                //Calc n0 altitude average
                double averageAltitude_n0 = CalcAverage(ls_n0);

                //Calc n1 altitude average
                double averageAltitude_n1 = CalcAverage(ls_n1);

                //If altitudeT1 > (altitudeT0 + 0.005 x altitudeT0) (pente d'au moins 5‰)
                if (averageAltitude_n1 > averageAltitude_n0 + Calc5PourMille(averageAltitude_n0))
                    return Pente.rising;

                //If altitudeT1 < (altitudeT0 - 0.005 x altitudeT0) (pente d'au moins 5‰)
                if (averageAltitude_n1 < averageAltitude_n0 - Calc5PourMille(averageAltitude_n0))
                    return Pente.downhill;

                //Else, it is straight
                return Pente.straight;
            }
            catch(Exception)
            {
                return Pente.unknown;
            }
        }

        private double CalcAverage(List<AltiPoint> lsPoints)
        {
            double res = 0.0;

            //TODO : affiner ? Prendre en compte altitudAccuracy ?
            foreach (AltiPoint p in lsPoints)
                res += p.m_dAltitude;

            res = res / lsPoints.Count;

            return res;
        }

        private double Calc5PourMille(double value)
        {
            return value * 5 / 1000;
        }
    }

    public enum Pente
    {
        rising, downhill, straight, unknown
    }
}

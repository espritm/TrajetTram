using Trajet_Tram.BusinessLayer;
using Trajet_Tram.Interface;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Trajet_Tram.Helpers
{
    public class LocalDatabase : ILocalDatabase
    {
        private SQLiteConnection m_connection;
        private static LocalDatabase singleton_singleInstance;
        static readonly object locker = new object();

        private LocalDatabase() { } //Risque de crasher, à voir
        private LocalDatabase(SQLiteConnection conn)
        {
            m_connection = conn;
            
            m_connection.CreateTable<Arret>();
            m_connection.CreateTable<Trajet>();
        }

        public static void Init(string sLocalDBPath)
        {
            SQLiteConnection conn = new SQLiteConnection(sLocalDBPath, false);

            if (conn == null)
                throw new Exception("Failed to create local database...");

            ServiceContainer.Register<ILocalDatabase>(() =>
            {
                if (singleton_singleInstance == null)
                {
                    lock (locker)
                    {
                        if (singleton_singleInstance == null)
                            singleton_singleInstance = new LocalDatabase(conn);
                    }
                }

                return singleton_singleInstance;
            });
        }

        public static ILocalDatabase Get()
        {
            return ServiceContainer.Resolve<ILocalDatabase>();
        }
        
        public void EmptyDatabase()
        {
            m_connection.DeleteAll<Trajet>();
            m_connection.DeleteAll<Arret>();
        }

        public List<Trajet> GetAlltrajetInDatabase(IFileAccessManager logFileManager)
        {
            List<Trajet> lsRes = GetItems<Trajet>(logFileManager);

            if (lsRes == null)
                lsRes = new List<Trajet>();

            return lsRes;
        }
        
        public List<Arret> GetArretOftrajet(IFileAccessManager logFileManager, Trajet trajet)
        {
            Expression<Func<Arret, bool>> filter = child => child.m_iLocalIDtrajet == trajet.m_iLocalID;

            List<Arret> lsRes = GetItems<Arret>(logFileManager, filter);

            if (lsRes == null)
                lsRes = new List<Arret>();

            return lsRes;
        }

        public Arret GetArretFromLocalID(IFileAccessManager logFileManager, int iLocalID)
        {
            Expression<Func<Arret, bool>> filter = child => child.m_iLocalID == iLocalID;
            return GetItem<Arret>(logFileManager, filter);
        }

        /*public trajet Gettrajet(string sRefNumber, DateTime date)
        {
            //Expression<Func<trajet, bool>> filter = child => child.m_sRefNumber == sRefNumber && child.m_datetrajet.Year == date.Year && child.m_datetrajet.Month == date.Month && child.m_datetrajet.Day == date.Day;
            var res = GetItems<trajet>();

            foreach (trajet m in res)
                if (m.m_sRefNumber == sRefNumber && m.m_datetrajet.Year == date.Year && m.m_datetrajet.Month == date.Month && m.m_datetrajet.Day == date.Day)
                    return m;
            return null;
        }*/

        public Trajet Gettrajet(IFileAccessManager logFileManager, int iLocalID)
        {
            Expression<Func<Trajet, bool>> filter = child => child.m_iLocalID == iLocalID;
            return GetItem<Trajet>(logFileManager, filter);
        }

        public Trajet Addtrajet(IFileAccessManager logFileManager, Trajet trajet)
        {
            //Should verify primary key manually ? ref number and date ?? ask client

            int iLocalIDCreated = InsertItem<Trajet>(logFileManager, trajet);

            if (iLocalIDCreated > 0)
                return trajet;
            else
                return null;//not inserted
        }

        public Arret AddArret(IFileAccessManager logFileManager, Arret Arret)
        {
            int iLocalIDCreated = InsertItem<Arret>(logFileManager, Arret);

            if (iLocalIDCreated > 0)
                return Arret;
            else
                return null;//not inserted
        }

        public void RemovetrajetAndItsarret(IFileAccessManager logFileManager, Trajet trajet)
        {
            //Remove trajet's arret first
            List<Arret> lsPtsPassage = GetArretOftrajet(logFileManager, trajet);
            foreach (Arret p in lsPtsPassage)
                DeleteItem<Arret>(logFileManager, p.m_iLocalID);
            
            DeleteItem<Trajet>(logFileManager, trajet.m_iLocalID);
        }

        public void Updatetrajet(IFileAccessManager logFileManager, Trajet trajet)
        {
            SaveItem<Trajet>(logFileManager, trajet);
        }

        private List<T> GetItems<T>(IFileAccessManager logFileManager) where T : new()
        {
            lock (locker)
            {
                string s = "GetItems(" + typeof(T).ToString() + ")";
                DateTime start = DateTime.Now;

                List<T> lsRes = m_connection.Table<T>().ToList();

                TimeSpan diff = DateTime.Now - start;
                TrajetTramLogger.LogConsoleDebug(logFileManager, "End(" + diff.Seconds + "s " + diff.Milliseconds + "ms, count = " + lsRes.Count + ") " + s, "DatabaseTrajetTram");
                return lsRes;
            }
        }

        private List<T> GetItems<T>(IFileAccessManager logFileManager, Expression<Func<T, bool>> Filter) where T : new()
        {
            lock (locker)
            {
                string s = "GetItems(" + typeof(T).ToString() + ", " + ((LambdaExpression)Filter).Body.ToString() + ")";
                DateTime start = DateTime.Now;

                List<T> lsRes = new List<T>();
                
                lsRes = m_connection.Table<T>().Where(Filter).ToList();

                TimeSpan diff = DateTime.Now - start;
                TrajetTramLogger.LogConsoleDebug(logFileManager, "End(" + diff.Seconds + "s " + diff.Milliseconds + "ms), count = " + lsRes.Count + ") " + s, "DatabaseTrajetTram");
                return lsRes;
            }
        }

        private T GetItem<T>(IFileAccessManager logFileManager, Expression<Func<T, bool>> Filter) where T : new()
        {
            lock (locker)
            {
                string s = "GetItem(" + typeof(T).ToString() + ", " + ((LambdaExpression)Filter).Body.ToString() + ")";
                DateTime start = DateTime.Now;

                T res = m_connection.Table<T>().Where(Filter).FirstOrDefault();

                TimeSpan diff = DateTime.Now - start;
                TrajetTramLogger.LogConsoleDebug(logFileManager, "End(" + diff.Seconds + "s " + diff.Milliseconds + "ms) " + s, "DatabaseTrajetTram");

                return res;
            }
        }

        private int InsertItem<T>(IFileAccessManager logFileManager, T item)
        {
            lock (locker)
            {
                string s = "InsertItem(" + typeof(T).ToString() + ")";
                DateTime start = DateTime.Now;

                int iRes = m_connection.Insert(item);

                TimeSpan diff = DateTime.Now - start;
                TrajetTramLogger.LogConsoleDebug(logFileManager, "End(" + diff.Seconds + "s " + diff.Milliseconds + "ms) " + s, "DatabaseTrajetTram");
                return iRes;
            }
        }

        private int SaveItem<T>(IFileAccessManager logFileManager, T item)
        {
            lock (locker)
            {
                string s = "SaveItem(" + typeof(T).ToString() + ")";
                DateTime start = DateTime.Now;

                int iRes = m_connection.Update(item);

                TimeSpan diff = DateTime.Now - start;
                TrajetTramLogger.LogConsoleDebug(logFileManager, "End(" + diff.Seconds + "s " + diff.Milliseconds + "ms) " + s, "DatabaseTrajetTram");
                return iRes;
            }
        }

        private int DeleteItem<T>(IFileAccessManager logFileManager, object Primary_Key_Value)
        {
            lock (locker)
            {
                string s = "DeleteItem(" + typeof(T).ToString() + ")";
                DateTime start = DateTime.Now;

                int iRes = m_connection.Delete<T>(Primary_Key_Value);

                TimeSpan diff = DateTime.Now - start;
                TrajetTramLogger.LogConsoleDebug(logFileManager, "End(" + diff.Seconds + "s " + diff.Milliseconds + "ms) " + s, "DatabaseTrajetTram");
                return iRes;
            }
        }
        
        private int DeleteItems<T>(IFileAccessManager logFileManager)
        {
            lock (locker)
            {
                string s = "DeleteItems(" + typeof(T).ToString() + ")";
                DateTime start = DateTime.Now;

                int iRes = m_connection.DeleteAll<T>();

                TimeSpan diff = DateTime.Now - start;
                TrajetTramLogger.LogConsoleDebug(logFileManager, "End(" + diff.Seconds + "s " + diff.Milliseconds + "ms) " + s, "DatabaseTrajetTram");
                return iRes;
            }
        }

        private List<T> QueryItems<T>(IFileAccessManager logFileManager, string sSQLRequest, params object[] args) where T : new()
        {
            lock (locker)
            {
                string s = "QueryItems(" + typeof(T).ToString() + ", " + sSQLRequest + ")";
                DateTime start = DateTime.Now;

                List<T> lsRes = m_connection.Query<T>(sSQLRequest, args);

                TimeSpan diff = DateTime.Now - start;
                TrajetTramLogger.LogConsoleDebug(logFileManager, "End(" + diff.Seconds + "s " + diff.Milliseconds + "ms) " + s, "DatabaseTrajetTram");
                return lsRes;
            }
        }
    }
}

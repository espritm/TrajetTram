using Microsoft.Azure.Mobile;
using Microsoft.Azure.Mobile.Analytics;
using Microsoft.Azure.Mobile.Crashes;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Trajet_Tram.Helpers
{
    public class MobileCenter_Helper
    {
        private StringBuilder m_sbLogs;
        private static MobileCenter_Helper m_instance;
        private static object locker = new object();

        private MobileCenter_Helper() { }

        public static MobileCenter_Helper GetInstance()
        {
            if (m_instance == null)
            {
                lock (locker)
                {
                    if (m_instance == null)
                        m_instance = new MobileCenter_Helper();
                }
            }

            return m_instance;
        }


        public static void AddLog(string sLog)
        {
            MobileCenter_Helper instance = MobileCenter_Helper.GetInstance();

            if (instance.m_sbLogs == null)
                lock (locker)
                {
                    if (instance.m_sbLogs == null)
                        instance.m_sbLogs = new StringBuilder();
                }

            lock (locker)
                instance.m_sbLogs.AppendLine(sLog);
        }

        public static void InitAndroidAnalytics()
        {
            MobileCenter.Start("6c8d9500-d1dd-422b-9a20-013c7cd92ae3", typeof(Analytics), typeof(Crashes)); 
#if DEBUG
            Analytics.Enabled = true;
            Crashes.Enabled = true;
#else
            Analytics.Enabled = true;
            Crashes.Enabled = true;
#endif
        }

        #region Reporting
        private static StringBuilder FormatErrorReporting(Exception e, string sTitle, string sDescription)
        {
            return FormatErrorReporting(e, new StringBuilder().AppendLine("Title = " + sTitle).AppendLine("Description = " + sDescription));
        }

        private static StringBuilder FormatErrorReporting(Exception e, StringBuilder dicInfos, string sIncrement = "")
        {
            dicInfos.AppendLine(sIncrement + "Message = " + e.Message);
            dicInfos.AppendLine(sIncrement + "StackTrace = " + e.StackTrace);
            dicInfos.AppendLine(sIncrement + "Source = " + e.Source);
            dicInfos.AppendLine(sIncrement + "HResult = " + e.HResult.ToString());
            dicInfos.AppendLine(sIncrement + "Type = " + e.GetType().Name);

            if (e.InnerException != null)
            {
                dicInfos.AppendLine(sIncrement + "InnerException = " + e.InnerException.GetType().Name);

                dicInfos = FormatErrorReporting(e.InnerException, dicInfos, sIncrement + "_");
            }

            MobileCenter_Helper instance = MobileCenter_Helper.GetInstance();
            if (instance.m_sbLogs != null)
                dicInfos.AppendLine(sIncrement + "TrajetTram Logs = " + instance.m_sbLogs.ToString());

            return dicInfos;
        }

        public static void ReportError(IFileAccessManager logFileManager, Exception e, [CallerFilePath]string sTitle = "", [CallerMemberName]string sDescription = "")
        {
            MobileCenter_Helper.AddAttachmentAndReportToMobileCenter(logFileManager, FormatErrorReporting(e, sTitle, sDescription).ToString());
            
            MobileCenter_Helper instance = MobileCenter_Helper.GetInstance();
            if (instance.m_sbLogs != null)
            {
                lock (locker)
                {
                    instance.m_sbLogs.Clear();
                    instance.m_sbLogs = null;
                }
            }
        }

        public static void ReportError(IFileAccessManager logFileManager, Exception e, StringBuilder dicInfos)
        {
            MobileCenter_Helper.AddAttachmentAndReportToMobileCenter(logFileManager, FormatErrorReporting(e, dicInfos).ToString());

            MobileCenter_Helper instance = MobileCenter_Helper.GetInstance();
            if (instance.m_sbLogs != null)
            {
                lock (locker)
                {
                    instance.m_sbLogs.Clear();
                    instance.m_sbLogs = null;
                }
            }
        }

        private static void AddAttachmentAndReportToMobileCenter(IFileAccessManager logFileManager, string sDesc)
        {
            TrajetTramLogger.LogConsoleDebug(logFileManager, "Exception - ", sDesc);

            //Always send crash report without asking to user
            Crashes.ShouldProcessErrorReport = (ErrorReport report) => { return true; };
            Crashes.ShouldAwaitUserConfirmation = () => { return false; };

            //Add attachments to the error
            Crashes.GetErrorAttachments = (ErrorReport report) =>
            {
                return new ErrorAttachmentLog[]
                {
                    ErrorAttachmentLog.AttachmentWithText(sDesc, "attachment.txt")
                };
            };
        }
#endregion

#region Track Events
        public static void TrackEvent(IFileAccessManager logFileManager, string sActivityOrFragment, string sItemName)
        {
            string sTrackIdentifier = "ItemClicked " + sActivityOrFragment + " " + sItemName;

            TrajetTramLogger.LogConsoleDebug(logFileManager, sTrackIdentifier, "Analytics.TrackEvent");

            Analytics.TrackEvent(sTrackIdentifier, new Dictionary<string, string> { { sActivityOrFragment, sItemName } });
        }

        public static void TrackGPS(IFileAccessManager logFileManager, string sLog)
        {
            TrajetTramLogger.LogConsoleDebug(logFileManager, "GPS - ", sLog);

            Analytics.TrackEvent("GPS - " + sLog, new Dictionary<string, string> ());
        }
#endregion
    }
}

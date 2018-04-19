

using System;

namespace Trajet_Tram.Helpers
{
    public static class TrajetTramLogger
    {
        public static void LogConsoleDebug(IFileAccessManager logFileManager, string sMsg, string sKeyword = "")
        {
            if (sKeyword != "")
                sKeyword = "_" + sKeyword + "_ ";

            string sLog = "TrajetTramLogger-- " + DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss:fff") + ": " + sKeyword + sMsg;

            System.Diagnostics.Debug.WriteLine(sLog);
            MobileCenter_Helper.AddLog(sLog);
            logFileManager?.WriteToLogFile(sLog + "\n");
        }
    }
}

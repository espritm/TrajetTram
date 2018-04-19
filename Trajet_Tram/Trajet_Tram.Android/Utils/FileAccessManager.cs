using System;
using Trajet_Tram.Helpers;

namespace Trajet_Tram.Droid.Utils
{
    public class FileAccessManager : IFileAccessManager
    {

        public void WriteToLogFile(string sText)
        {
            try
            {
                System.IO.File.AppendAllText(Settings.LogFilePath, sText);
            }
            catch (Exception)
            {
            }
        }
    }
}
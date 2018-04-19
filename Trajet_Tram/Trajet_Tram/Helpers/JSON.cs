using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trajet_Tram.Helpers
{
    public class JSON
    {
        public static Task<T> DeserializeObjectAsync<T>(IFileAccessManager logFileManager, string value, string sMoreInfoForExceptionThrowned)
        {
            return Task.Factory.StartNew(() => DeserializeObject<T>(logFileManager, value, sMoreInfoForExceptionThrowned));
        }

        public static Task<string> SerializeObjectAsync<T>(IFileAccessManager logFileManager, T item, string sMoreInfoForExceptionThrowned = "")
        {
            return Task.Factory.StartNew(() => SerializeObject<T>(logFileManager, item, sMoreInfoForExceptionThrowned));
        }

        public static T DeserializeObject<T>(IFileAccessManager logFileManager, string value, string sMoreInfoForExceptionThrowned)
        {
            T res = default(T);

            try
            {
                res = JsonConvert.DeserializeObject<T>(value);
            }
            catch (Exception e)
            {
                MobileCenter_Helper.ReportError(logFileManager, e,
                    "DeserializeObjectAsync<" + typeof(T).FullName + ">",
                    "Error during JSON Deserialization..." +
                    "\n\ntext to deserialize = \"" + value +
                    "\"\n\nMore Information... " + sMoreInfoForExceptionThrowned +
                    "\n\nException.StackTrace = " + e.StackTrace +
                    "\n\nException.Message = " + e.Message);
            }

            return res;
        }

        public static string SerializeObject<T>(IFileAccessManager logFileManager, T item, string sMoreInfoForExceptionThrowned = "")
        {
            string res = "";

            try
            {
                res = JsonConvert.SerializeObject(item);
            }
            catch (Exception e)
            {
                MobileCenter_Helper.ReportError(logFileManager, e, "SerializeObjectAsync<" + typeof(T).FullName + ">", "Error during JSON Serialization...\n\nitem = " + item.ToString() + "\n\nException.StackTrace = " + e.StackTrace + "\n\nMore Information... " + sMoreInfoForExceptionThrowned + "\n\nException.Message = " + e.Message);
            }

            return res;
        }
    }
}
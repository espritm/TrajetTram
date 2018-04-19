using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Trajet_Tram.Helpers
{
    public static class Utils
    {
        public static bool IsEmailValid(string sEmail)
        {
            if (string.IsNullOrWhiteSpace(sEmail))
                return false;

            const string emailRegex = @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
            @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$";

            return (Regex.IsMatch(sEmail, emailRegex, RegexOptions.IgnoreCase));
        }
    }
}

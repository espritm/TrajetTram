using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trajet_Tram.Exceptions
{
    public class OnlineException : Exception
    {
        public string m_sURL { get; set; }
        public string m_sBody { get; set; }
        public string m_sAdditionnalInfos{ get; set; }

        public OnlineException(string sURL, string sBody, string sAdditionnalInfos = default(string))
        {
            m_sURL = sURL;
            m_sBody = sBody;
            m_sAdditionnalInfos = sAdditionnalInfos;
        }

        public override string Message
        {
        get
            {
                return base.Message + "\nm_sApiURL = " + m_sURL + "\nm_sApiBody = " + m_sBody + "\nm_sAdditionnalInformations = " + m_sAdditionnalInfos;
            }
        }
    }

    public class InternetRequired : Exception
    { }

    public class trajetNotFound : Exception
    { }

    public class InvalidLogin : Exception
    { }

    public class NeedActivityLogin : Exception
    { }
}

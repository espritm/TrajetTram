using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trajet_Tram.Helpers
{
    public interface IFileAccessManager
    {
        void WriteToLogFile(string sText);
    }
}

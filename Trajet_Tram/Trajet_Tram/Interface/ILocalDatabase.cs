using Trajet_Tram.BusinessLayer;
using Trajet_Tram.Helpers;
using System;
using System.Collections.Generic;

namespace Trajet_Tram.Interface
{
    public interface ILocalDatabase
    {
        void EmptyDatabase();
        List<Trajet> GetAlltrajetInDatabase(IFileAccessManager logFileManager);
        List<Arret> GetArretOftrajet(IFileAccessManager logFileManager, Trajet trajet);
        Trajet Gettrajet(IFileAccessManager logFileManager, int iLocalID);
        Trajet Addtrajet(IFileAccessManager logFileManager, Trajet trajet);
        Arret AddArret(IFileAccessManager logFileManager, Arret Arret);
        void RemovetrajetAndItsarret(IFileAccessManager logFileManager, Trajet trajet);
        void Updatetrajet(IFileAccessManager logFileManager, Trajet trajet);
        Arret GetArretFromLocalID(IFileAccessManager logFileManager, int iLocalID);
    }
}

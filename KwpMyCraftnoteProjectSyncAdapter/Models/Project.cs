using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace KwpMyCraftnoteProjectSyncAdapter.Models
{
    public class Project
    {
        public String ProjAnlage
        {
            get;
            set;
        }
        public String ProjNr
        {
            get;
            set;
        }
        public String ProjBezeichnung
        {
            get;
            set;
        }
        public String ProjAdr
        {
            get;
            set;
        }
        public String BauHrAdr
        {
            get;
            set;
        }
        public String RechAdr
        {
            get;
            set;
        }

        public int ProjAdrAnredeID
        {
            get;
            set;
        }
        public String ProjAdrAnrede
        {
            get;
            set;
        }
        public String ProjAdrName
        {
            get;
            set;
        }
        public String ProjAdrVorname
        {
            get;
            set;
        }
        public String ProjAdrStrasse
        {
            get;
            set;
        }
        public int ProjAdrOrtOrtID
        {
            get;
            set;
        }
        public String ProjAdrOrtLand
        {
            get;
            set;
        }
        public String ProjAdrOrtPLZ
        {
            get;
            set;
        }
        public String ProjAdrOrtOrt
        {
            get;
            set;
        }
        public String ProjAdrTelNr
        {
            get;
            set;
        }
        public String ProjAdrEmail
        {
            get;
            set;
        }
        public long ProjAdrAnsprechpartnerID
        {
            get;
            set;
        }

        public ContactPerson ProjectAdrAnsprechpartner { get; set; }

        public int BauHrAdrAnredeID
        {
            get;
            set;
        }
        public String BauHrAdrAnrede
        {
            get;
            set;
        }
        public String BauHrAdrName
        {
            get;
            set;
        }
        public String BauHrAdrVorname
        {
            get;
            set;
        }
        public String BauHrAdrStrasse
        {
            get;
            set;
        }
        public int BauHrAdrOrtOrtID
        {
            get;
            set;
        }
        public String BauHrAdrOrtLand
        {
            get;
            set;
        }
        public String BauHrAdrOrtPLZ
        {
            get;
            set;
        }
        public String BauHrAdrOrtOrt
        {
            get;
            set;
        }
        public String BauHrAdrTelNr
        {
            get;
            set;
        }
        public String BauHrAdrEmail
        {
            get;
            set;
        }
        public long BauHrAdrAnsprechpartnerID
        {
            get;
            set;
        }

        public ContactPerson BauHrAdrAnsprechpartner { get; set; }

        public int RechAdrAnredeID
        {
            get;
            set;
        }
        public String RechAdrAnrede
        {
            get;
            set;
        }
        public String RechAdrName
        {
            get;
            set;
        }
        public String RechAdrVorname
        {
            get;
            set;
        }
        public String RechAdrStrasse
        {
            get;
            set;
        }
        public int RechAdrOrtOrtID
        {
            get;
            set;
        }
        public String RechAdrOrtLand
        {
            get;
            set;
        }
        public String RechAdrOrtPLZ
        {
            get;
            set;
        }
        public String RechAdrOrtOrt
        {
            get;
            set;
        }
        public String RechAdrTelNr
        {
            get;
            set;
        }
        public String RechAdrEmail
        {
            get;
            set;
        }
        public long RechAdrAnsprechpartnerID
        {
            get;
            set;
        }
        public ContactPerson RechAdrAnsprechpartner { get; set; }
    }
}

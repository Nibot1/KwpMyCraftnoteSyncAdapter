using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KwpMyCraftnoteProjectSyncAdapter.Models
{
    public class ContactPerson
    {
        public int AnsprechpartnerID { get; set; }
        public String AdrNrGes { get; set; }
        public int AnredeID { get; set; }
        public String Anrede { get; set; }
        public String Nachname { get; set; }
        public String Vorname { get; set; }
        public String Zusatz { get; set; }
        public String Bemerkung { get; set; }
        public String Telefonnummer { get; set; }
        public String Email { get; set; }
        public String TelefonnummerPrivat { get; set; }
        public String EmailPrivat { get; set; }

    }
}

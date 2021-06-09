using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MecPlastikEF.Models
{
    public class KalipDegisimModel
    {
        public DateTime DegisimSuresi { get; set; }
        public int TezgahID { get; set; }
        public string isim { get; set; }
        public string Durum { get; set; }
    }
}
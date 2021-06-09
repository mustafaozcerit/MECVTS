using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MecPlastikEF.Models
{
    public class TezgahPasifModel
    {
        public int tezgahID { get; set; }
        public TimeSpan tezgahDegisimSuresi { get; set; }
        public string durum { get; set; }
        public DateTime bitisTarihi { get; set; }
    }
}
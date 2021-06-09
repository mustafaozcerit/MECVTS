using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MecPlastikEF.Models
{
    public class VerilerModel
    {
        public int MakineNo { get; set; }
        public int TezgahID { get; set; }
        public int UretimAdeti { get; set; }
        public string uretimTarihi { get; set; }

        public bool durum { get; set; }
    }
}
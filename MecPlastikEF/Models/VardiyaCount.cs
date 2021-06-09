using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MecPlastikEF.Models
{
    public class VardiyaCount
        
    {
        public string TezgahID { get; set; }
        public string TezgahName { get; set; }
        public int sabahCount { get; set; }

        public int sabahDurusCount { get; set; }

        public TimeSpan sabahDurusSuresi { get; set; }
        public int geceCount { get; set; }

        public int geceDurusCount { get; set; }
        public TimeSpan geceDurusSuresi { get; set; }

        public int dunSabah { get; set; }
        public int dunGece { get; set; }

        public bool Durum { get; set; }

        public string gecikmeSuresi { get; set; }
    }
}
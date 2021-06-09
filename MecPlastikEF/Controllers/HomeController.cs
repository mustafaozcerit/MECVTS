using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MecPlastikEF.Models;
using MecPlastikEF.Models.Entity;
namespace MecPlastikEF.Controllers
{
    public class HomeController : Controller
    {
        VTSEntities _db = new VTSEntities();

        public ActionResult Index()
        {
            return View();
        }
        public ActionResult Login()
        {
            return View();
        }

        public void DurusHesapla()
        {
            DateTime calismaTarih = DateTime.Today.AddHours(6).AddMinutes(30);
            DateTime gece = DateTime.Today.AddHours(18).AddMinutes(30);
            var durusHesapList = _db.BakimDurumu.Where(x=> x.Tarih <= gece && x.Tarih>DateTime.Today).ToList();
            var durusList = _db.AnlikTabloDegerleri.Where(x=> x.Tarih== calismaTarih).ToList();
            foreach (var item in durusList)
            {
                foreach (var durus in durusHesapList)
                {
                    if (item.TezgahID == durus.TezgahID.ToString())
                    {
                        var sorgu = _db.AnlikTabloDegerleri.Where(x => x.TezgahID == item.TezgahID && x.SabahDurusSuresi == item.SabahDurusSuresi).FirstOrDefault();
                        sorgu.SabahDurusSuresi = sorgu.SabahDurusSuresi - durus.TezgahDegisimSuresi;
                        _db.SaveChanges();
                    }
                }
            }
            ViewData["DurusHesap"] = durusHesapList;
        }
        public ActionResult MepSetting()
        {
            TezgahDataAl();
            return View();
        }
        public ActionResult Table()
        {

            DataCek();
            TezgahDegisimDataAl();
            //DurusHesapla();
            return View();
        }
        public void TezgahDataAl()
        {
            IList<TezgahID_Name> tezgahlarList = (IList<TezgahID_Name>)_db.TezgahID_Name.ToList();
            ViewData["Tezgahlar"] = tezgahlarList;
        }
        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";
            return View();
        }
        public ActionResult TezgahDegisim(int tezgahID, string isim, string durum)
        {
            List<Uretim_Table> uretim_Tables = _db.Uretim_Table.Where(m => m.Tezgah_ID.ToString() == tezgahID.ToString()).ToList();
            DateTime tarih = DateTime.Parse(uretim_Tables.LastOrDefault().Uretim_Tarih);
            _db.BakimDurumu.Add(new BakimDurumu() { TezgahID = tezgahID, Durum = durum, Tarih = tarih, TezgahName = isim });
            _db.SaveChanges();
            return RedirectToAction("Table");
        }
        [HttpPost]
        public JsonResult TezgahPasifYap(BakimDurumu bakimDurumu)
        {
            var anlik = _db.AnlikTabloDegerleri.Where(x => x.TezgahID == bakimDurumu.TezgahID.ToString()).FirstOrDefault();
            var ktg = _db.BakimDurumu.Where(x => x.TezgahID == bakimDurumu.TezgahID && x.Durum == "A").FirstOrDefault();
            if (ktg != null && ktg.TezgahID!=0)
            {
                ktg.Durum = bakimDurumu.Durum;
                ktg.TezgahDegisimSuresi = bakimDurumu.TezgahDegisimSuresi;
                ktg.BitisTarihi = bakimDurumu.BitisTarihi;
                ktg.Kalip1Adet = anlik.SabahCount;
                _db.SaveChanges();
            }
            return Json("Kalıp Değişimi Gerçekleştirildi(" + ktg.TezgahName + ").", JsonRequestBehavior.AllowGet);
        }
        public void TezgahDegisimDataAl()
        {
            string bugun = DateTime.Today.ToString("yyyy-MM-dd");
            List<BakimDurumu> List;
            List = _db.BakimDurumu.ToList();
            ViewData["TezgahDegisimDataAl"] = List;
        }
        public void DataCek()
        {
            string bugun = DateTime.Today.ToString("yyyy-MM-dd");
            string dun = DateTime.Today.AddDays(-1).ToString("yyyy-MM-dd");
            string yarin = DateTime.Today.AddDays(+1).ToString("yyyy-MM-dd");
            int bugunSaat = DateTime.Now.Hour;
            int bugunDK = DateTime.Now.Minute;
            int saat = (bugunSaat * 100) + (bugunDK);
            List<Uretim_Table> List;
            List<TezgahID_Name> TezgahList;
            List<VardiyaCount> tableList = new List<VardiyaCount>();

            ///////////////////////// SABAH /////////////////////////////
            if (saat >= 630 && saat <= 1830)
            {
                List = _db.Uretim_Table.Where(s => s.Uretim_Tarih.Substring(0, 11) == bugun || s.Uretim_Tarih.Substring(0, 11) == dun).ToList();
                TezgahList = _db.TezgahID_Name.ToList();
                foreach (var tezgah in TezgahList)
                {
                    IList<VerilerModel> sabahDurus = new List<VerilerModel>();
                    IList<VerilerModel> geceDurus = new List<VerilerModel>();
                    int sabahCount = 0, sabahDurusCount = 0, geceCount = 0, geceDurusCount = 0;
                    TimeSpan sabahDurusSuresi = TimeSpan.Zero;
                    TimeSpan geceDurusSuresi = TimeSpan.Zero;
                    bool Durum = false;
                    for (int i = 0; i < List.Count - 1; i++)
                    {
                        int TableSaat = ((Convert.ToDateTime(List[i].Uretim_Tarih).Hour * 100) + Convert.ToDateTime(List[i].Uretim_Tarih).Minute);
                        string tableTarih = List[i].Uretim_Tarih.Substring(0, 11);
                        string[] bosluk;
                        bosluk = List[i].Uretim_Tarih.Split(' ');

                        if (Convert.ToInt32(tezgah.TezgahId) == List[i].Tezgah_ID)
                        {

                            if (bugun == bosluk[0] && TableSaat >= 630 && TableSaat <= 1830)
                            {
                                //SABAH
                                sabahCount++;
                                sabahDurus.Add(new VerilerModel() { uretimTarihi = List[i].Uretim_Tarih, TezgahID = List[i].Tezgah_ID });
                            }
                            if ((TableSaat > 1830 && bosluk[0] == dun) || (TableSaat < 630 && bosluk[0] == bugun))
                            {
                                //GECECİ
                                geceCount++;
                                geceDurus.Add(new VerilerModel() { uretimTarihi = List[i].Uretim_Tarih, TezgahID = List[i].Tezgah_ID });
                            }
                            if ((saat - 3 <= TableSaat) && bosluk[0] == bugun)
                            {
                                //AKTIF PASIF DURUMLARI
                                Durum = true;
                            }
                        }
                    }
                    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    //SIRALAMA ALGORİTMASI
                    DateTime gecici;
                    string geciciIsim;
                    int k = 0;
                    for (int j = 1; j < sabahDurus.Count; j++)
                    {
                        gecici = DateTime.Parse(sabahDurus[j].uretimTarihi);
                        geciciIsim = sabahDurus[j].TezgahID.ToString();
                        k = j - 1;
                        while (k >= 0 && gecici < DateTime.Parse(sabahDurus[k].uretimTarihi))
                        {
                            sabahDurus[k + 1].uretimTarihi = sabahDurus[k].uretimTarihi;
                            sabahDurus[k + 1].TezgahID = sabahDurus[k].TezgahID;
                            k = k - 1;
                            sabahDurus[k + 1].uretimTarihi = gecici.ToString();
                            sabahDurus[k + 1].TezgahID = Convert.ToInt32(geciciIsim);
                        }
                    }
                    //SIRALAMA ALGORİTMASI
                    DateTime gecici1;
                    string geciciIsim1;
                    int k1 = 0;
                    for (int j = 1; j < geceDurus.Count; j++)
                    {
                        gecici1 = DateTime.Parse(geceDurus[j].uretimTarihi);
                        geciciIsim1 = geceDurus[j].TezgahID.ToString();
                        k1 = j - 1;
                        while (k1 >= 0 && gecici1 < DateTime.Parse(geceDurus[k1].uretimTarihi))
                        {
                            geceDurus[k1 + 1].uretimTarihi = geceDurus[k1].uretimTarihi;
                            geceDurus[k1 + 1].TezgahID = geceDurus[k1].TezgahID;
                            k1 = k1 - 1;
                            geceDurus[k1 + 1].uretimTarihi = gecici1.ToString();
                            geceDurus[k1 + 1].TezgahID = Convert.ToInt32(geciciIsim1);
                        }
                    }
                    string gecikmeSuresi1 = "Arızalı";
                    if (sabahDurus.Count != 0)
                    {
                        int sondeger = sabahDurus.Count - 1;
                        TimeSpan gecikmeSuresi = DateTime.Now - DateTime.Parse(sabahDurus[sondeger].uretimTarihi);
                        TimeSpan tm = new TimeSpan(00, 03, 00);
                        if (gecikmeSuresi < tm)
                        {
                            gecikmeSuresi1 = "Çalışıyor";
                        }
                        else
                        {
                            gecikmeSuresi1 = gecikmeSuresi.ToString(@"hh\:mm") + " DK'dır Çalışmıyor";
                        }
                    }
                    //SABAH
                    for (int y = 0; y < sabahDurus.Count - 1; y++)
                    {
                        if (Convert.ToInt32(tezgah.TezgahId) == sabahDurus[y].TezgahID)
                        {
                            DateTime ilkdurum = DateTime.Parse(sabahDurus[y].uretimTarihi);
                            ilkdurum = ilkdurum.AddMinutes(3);
                            DateTime ikincidurum = DateTime.Parse(sabahDurus[y + 1].uretimTarihi);
                            //SABAH
                            if (ilkdurum <= ikincidurum)
                            {
                                sabahDurusCount++;
                                ilkdurum = ilkdurum.AddMinutes(-3);
                                TimeSpan fark = ikincidurum - ilkdurum;
                                if (fark < TimeSpan.FromHours(480))
                                {
                                    sabahDurusSuresi = sabahDurusSuresi + fark;
                                }
                            }
                        }
                    }
                    //GECE
                    for (int y = 0; y < geceDurus.Count - 1; y++)
                    {
                        if (Convert.ToInt32(tezgah.TezgahId) == geceDurus[y].TezgahID)
                        {
                            DateTime ilkdurum = DateTime.Parse(geceDurus[y].uretimTarihi);
                            ilkdurum = ilkdurum.AddMinutes(3);
                            DateTime ikincidurum = DateTime.Parse(geceDurus[y + 1].uretimTarihi);
                            //GECE
                            if (ilkdurum <= ikincidurum)
                            {
                                geceDurusCount++;
                                ilkdurum = ilkdurum.AddMinutes(-3);
                                TimeSpan fark = ikincidurum - ilkdurum;
                                if (fark < TimeSpan.FromHours(480))
                                {
                                    geceDurusSuresi = geceDurusSuresi + fark;
                                }
                            }
                        }
                    }

                    DateTime calismaTarih = DateTime.Today.AddHours(6).AddMinutes(30);
                    //DateTime gece = DateTime.Today.AddHours(18).AddMinutes(30);
                    //var durusHesapList = _db.BakimDurumu.Where(x => x.Tarih <= gece && x.Tarih > DateTime.Today).ToList();

                    //    foreach (var durus in durusHesapList)
                    //    {
                    //        if (tezgah.TezgahId==durus.TezgahID.ToString())
                    //        {
                    //        sabahDurusSuresi = (TimeSpan)(sabahDurusSuresi - durus.TezgahDegisimSuresi);
                    //        string sbhgecici = sabahDurusSuresi.Hours + ":" + sabahDurusSuresi.Minutes + ":00";
                    //        sabahDurusSuresi = TimeSpan.Parse(sbhgecici);
                    //        }
                    //    }
                    tableList.Add(new VardiyaCount() { sabahCount = sabahCount, geceCount = geceCount, Durum = Durum, sabahDurusCount = sabahDurusCount, geceDurusCount = geceDurusCount, sabahDurusSuresi = sabahDurusSuresi, geceDurusSuresi = geceDurusSuresi, TezgahID = tezgah.TezgahId, TezgahName = tezgah.TezgahName, gecikmeSuresi = gecikmeSuresi1 });
                    var anlikTabloDegerleri = _db.AnlikTabloDegerleri.FirstOrDefault(x=> x.TezgahID==tezgah.TezgahId && x.Tarih ==calismaTarih);

                    if (anlikTabloDegerleri == null)
                    {
                        _db.AnlikTabloDegerleri.Add(new AnlikTabloDegerleri() {  SabahCount= sabahCount, GeceCount = geceCount, Durum = Durum.ToString(), SabahDurusCount = sabahDurusCount, GeceDurusCount = geceDurusCount, SabahDurusSuresi = sabahDurusSuresi, GeceDurusSuresi = geceDurusSuresi, TezgahID = tezgah.TezgahId, TezgahName = tezgah.TezgahName, GecikmeSuresi = gecikmeSuresi1 , Tarih=calismaTarih });
                        _db.SaveChanges();
                    }
                    else
                    {
                        anlikTabloDegerleri.SabahCount = sabahCount;
                        anlikTabloDegerleri.GeceCount = geceCount;
                        anlikTabloDegerleri.Durum = Durum.ToString();
                        anlikTabloDegerleri.SabahDurusCount = sabahDurusCount;
                        anlikTabloDegerleri.GeceDurusCount = geceDurusCount;
                        anlikTabloDegerleri.SabahDurusSuresi = sabahDurusSuresi;
                        anlikTabloDegerleri.GeceDurusSuresi = geceDurusSuresi;
                        anlikTabloDegerleri.TezgahID = tezgah.TezgahId;
                        anlikTabloDegerleri.TezgahName = tezgah.TezgahName;
                        anlikTabloDegerleri.GecikmeSuresi = gecikmeSuresi1;
                        anlikTabloDegerleri.Tarih = calismaTarih;
                        _db.SaveChanges();
                    }
                }
            }
            ///////////////////////// GECE  /////////////////////////////
            else
            {
                List = _db.Uretim_Table.Where(s => s.Uretim_Tarih.Substring(0, 11) == bugun || s.Uretim_Tarih.Substring(0, 11) == dun).ToList();
                TezgahList = _db.TezgahID_Name.ToList();

                foreach (var tezgah in TezgahList)
                {
                    IList<VerilerModel> sabahDurus = new List<VerilerModel>();
                    IList<VerilerModel> geceDurus = new List<VerilerModel>();
                    int sabahCount = 0, sabahDurusCount = 0, geceCount = 0, geceDurusCount = 0;
                    TimeSpan sabahDurusSuresi = TimeSpan.Zero;
                    TimeSpan geceDurusSuresi = TimeSpan.Zero;
                    bool Durum = false;
                    for (int i = 0; i < List.Count - 1; i++)
                    {
                        int TableSaat = ((Convert.ToDateTime(List[i].Uretim_Tarih).Hour * 100) + Convert.ToDateTime(List[i].Uretim_Tarih).Minute);
                        string tableTarih = List[i].Uretim_Tarih.Substring(0, 11);
                        string[] bosluk;
                        bosluk = List[i].Uretim_Tarih.Split(' ');

                        if (Convert.ToInt32(tezgah.TezgahId) == List[i].Tezgah_ID)
                        {
                            if (saat < 2400 && saat>1830)
                            {
                                if ((TableSaat > 1830 && bosluk[0] == bugun))
                                {
                                    //GECECİ
                                    geceCount++;
                                    geceDurus.Add(new VerilerModel() { uretimTarihi = List[i].Uretim_Tarih, TezgahID = List[i].Tezgah_ID });
                                }

                                if (bugun == bosluk[0] && TableSaat >= 630 && TableSaat <= 1830)
                                {
                                    //SABAH
                                    sabahCount++;
                                    sabahDurus.Add(new VerilerModel() { uretimTarihi = List[i].Uretim_Tarih, TezgahID = List[i].Tezgah_ID });
                                }
                            }
                            else
                            {
                                if ((TableSaat > 1830 && bosluk[0] == dun) ||(TableSaat<630 &&  bosluk[0]==bugun))
                                {
                                    //GECECİ
                                    geceCount++;
                                    geceDurus.Add(new VerilerModel() { uretimTarihi = List[i].Uretim_Tarih, TezgahID = List[i].Tezgah_ID });
                                }
                                if (dun == bosluk[0] && TableSaat >= 630 && TableSaat <= 1830)
                                {
                                    //SABAH
                                    sabahCount++;
                                    sabahDurus.Add(new VerilerModel() { uretimTarihi = List[i].Uretim_Tarih, TezgahID = List[i].Tezgah_ID });
                                }
                            }
                            if ((saat - 3 <= TableSaat) && bosluk[0] == bugun)
                            {
                                //AKTIF PASIF DURUMLARI
                                Durum = true;
                            }
                        }
                    }
                    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    /////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    //SIRALAMA ALGORİTMASI
                    DateTime gecici;
                    string geciciIsim;
                    int k = 0;
                    for (int j = 1; j < sabahDurus.Count; j++)
                    {
                        gecici = DateTime.Parse(sabahDurus[j].uretimTarihi);
                        geciciIsim = sabahDurus[j].TezgahID.ToString();
                        k = j - 1;
                        while (k >= 0 && gecici < DateTime.Parse(sabahDurus[k].uretimTarihi))
                        {
                            sabahDurus[k + 1].uretimTarihi = sabahDurus[k].uretimTarihi;
                            sabahDurus[k + 1].TezgahID = sabahDurus[k].TezgahID;
                            k = k - 1;
                            sabahDurus[k + 1].uretimTarihi = gecici.ToString();
                            sabahDurus[k + 1].TezgahID = Convert.ToInt32(geciciIsim);
                        }
                    }
                    //SIRALAMA ALGORİTMASI
                    DateTime gecici1;
                    string geciciIsim1;
                    int k1 = 0;
                    for (int j = 1; j < geceDurus.Count; j++)
                    {
                        gecici1 = DateTime.Parse(geceDurus[j].uretimTarihi);
                        geciciIsim1 = geceDurus[j].TezgahID.ToString();
                        k1 = j - 1;
                        while (k1 >= 0 && gecici1 < DateTime.Parse(geceDurus[k1].uretimTarihi))
                        {
                            geceDurus[k1 + 1].uretimTarihi = geceDurus[k1].uretimTarihi;
                            geceDurus[k1 + 1].TezgahID = geceDurus[k1].TezgahID;
                            k1 = k1 - 1;
                            geceDurus[k1 + 1].uretimTarihi = gecici1.ToString();
                            geceDurus[k1 + 1].TezgahID = Convert.ToInt32(geciciIsim1);
                        }
                    }
                    string gecikmeSuresi1 = "Arızalı";
                    if (sabahDurus.Count != 0)
                    {
                        int sondeger = sabahDurus.Count - 1;
                        TimeSpan gecikmeSuresi = DateTime.Now - DateTime.Parse(sabahDurus[sondeger].uretimTarihi);
                        TimeSpan tm = new TimeSpan(00, 03, 00);
                        if (gecikmeSuresi < tm)
                        {
                            gecikmeSuresi1 = "Çalışıyor";
                        }
                        else
                        {
                            gecikmeSuresi1 = gecikmeSuresi.ToString(@"hh\:mm") + " DK'dır Çalışmıyor";
                        }
                    }
                    //SABAH
                    for (int y = 0; y < sabahDurus.Count - 1; y++)
                    {
                        if (Convert.ToInt32(tezgah.TezgahId) == sabahDurus[y].TezgahID)
                        {
                            DateTime ilkdurum = DateTime.Parse(sabahDurus[y].uretimTarihi);
                            ilkdurum = ilkdurum.AddMinutes(3);
                            DateTime ikincidurum = DateTime.Parse(sabahDurus[y + 1].uretimTarihi);
                            //SABAH
                            if (ilkdurum <= ikincidurum)
                            {
                                sabahDurusCount++;
                                ilkdurum = ilkdurum.AddMinutes(-3);
                                TimeSpan fark = ikincidurum - ilkdurum;
                                if (fark < TimeSpan.FromHours(480))
                                {
                                    sabahDurusSuresi = sabahDurusSuresi + fark;
                                }
                            }
                        }
                    }
                    //GECE
                    for (int y = 0; y < geceDurus.Count - 1; y++)
                    {
                        if (Convert.ToInt32(tezgah.TezgahId) == geceDurus[y].TezgahID)
                        {
                            DateTime ilkdurum = DateTime.Parse(geceDurus[y].uretimTarihi);
                            ilkdurum = ilkdurum.AddMinutes(3);
                            DateTime ikincidurum = DateTime.Parse(geceDurus[y + 1].uretimTarihi);
                            //GECE
                            if (ilkdurum <= ikincidurum)
                            {
                                geceDurusCount++;
                                ilkdurum = ilkdurum.AddMinutes(-3);
                                TimeSpan fark = ikincidurum - ilkdurum;
                                if (fark < TimeSpan.FromHours(480))
                                {
                                    geceDurusSuresi = geceDurusSuresi + fark;
                                }
                            }
                        }
                    }
                    tableList.Add(new VardiyaCount() { sabahCount = sabahCount, geceCount = geceCount, Durum = Durum, sabahDurusCount = sabahDurusCount, geceDurusCount = geceDurusCount, sabahDurusSuresi = sabahDurusSuresi, geceDurusSuresi = geceDurusSuresi, TezgahID = tezgah.TezgahId, TezgahName = tezgah.TezgahName, gecikmeSuresi = gecikmeSuresi1 });
                    DateTime calismaTarih = DateTime.Today.AddHours(6).AddMinutes(30);
                    var anlikTabloDegerleri = _db.AnlikTabloDegerleri.FirstOrDefault(x => x.TezgahID == tezgah.TezgahId && x.Tarih == calismaTarih);

                    if (anlikTabloDegerleri == null)
                    {
                        _db.AnlikTabloDegerleri.Add(new AnlikTabloDegerleri() { SabahCount = sabahCount, GeceCount = geceCount, Durum = Durum.ToString(), SabahDurusCount = sabahDurusCount, GeceDurusCount = geceDurusCount, SabahDurusSuresi = sabahDurusSuresi, GeceDurusSuresi = geceDurusSuresi, TezgahID = tezgah.TezgahId, TezgahName = tezgah.TezgahName, GecikmeSuresi = gecikmeSuresi1, Tarih = calismaTarih });
                        _db.SaveChanges();
                    }
                    else
                    {
                        anlikTabloDegerleri.SabahCount = sabahCount;
                        anlikTabloDegerleri.GeceCount = geceCount;
                        anlikTabloDegerleri.Durum = Durum.ToString();
                        anlikTabloDegerleri.SabahDurusCount = sabahDurusCount;
                        anlikTabloDegerleri.GeceDurusCount = geceDurusCount;
                        anlikTabloDegerleri.SabahDurusSuresi = sabahDurusSuresi;
                        anlikTabloDegerleri.GeceDurusSuresi = geceDurusSuresi;
                        anlikTabloDegerleri.TezgahID = tezgah.TezgahId;
                        anlikTabloDegerleri.TezgahName = tezgah.TezgahName;
                        anlikTabloDegerleri.GecikmeSuresi = gecikmeSuresi1;
                        anlikTabloDegerleri.Tarih = calismaTarih;
                        _db.SaveChanges();
                    }
                }
            }

            ViewData["Data"] = tableList;
        }
        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
        public ActionResult Chart()
        {
            return View();
        }
        public ActionResult Chart2()
        {
            return View();
        }
        public ActionResult Chart3()
        {
            return View();
        }
        public ActionResult GrafikGoster()
        {
            return View();
        }
    }
}
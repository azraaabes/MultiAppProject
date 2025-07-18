using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.Extensions.Logging;
using NLog;

namespace WebApplication1.Controllers
{
    public class HomeController : Controller
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger(); // Logger=NLog kütüphanesinin tipi; log mesajlarını yazmaya yarayan sınıf.Bu tip, Info(), Warn(), Error() gibi metodlarla farklı seviye log üretir.HomeController, kullanıcıya dönen en temel sayfaları yönettiği için loglama buradan başlatılır.
                                                                                    //Çalıştığı sınıfın tam adını (namespace + sınıf) logger adı olarak kullanarak bir Logger oluşturur/alır.
        public HomeController()
        {
            
        }
        public ActionResult Index()
        {
            logger.Info("Index sayfası yüklendi.");
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
        [HttpGet]
        public JsonResult CheckSession()
        {
            bool sessionActive = Session["Username"] != null;
            return Json(sessionActive, JsonRequestBehavior.AllowGet);
        }
    }
}
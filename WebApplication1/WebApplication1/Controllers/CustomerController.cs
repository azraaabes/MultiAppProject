
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication1.Models.Siniflar;


namespace WebApplication1.Controllers
{
    public class CustomerController : Controller
    {
        Context c = new Context();
        // GET: Customer
        public ActionResult Index()
        {
            var degerler = c.Customers.ToList();
            return View(degerler);
        }
    }
}
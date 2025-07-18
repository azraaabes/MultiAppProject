using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using WebApplication1.Helpers;

namespace WebApplication1.Controllers
{
    public class BaseController : Controller
    {

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            //Bu sınıf Controller sınıfından türemiştir ve OnActionExecuting metodunu override ederek her action çalışmadan önce devreye girer. Bu sayede, oturum kontrolü gibi işlemleri merkezi bir yerde yapmanızı sağlar.
            // Giriş yapılmadan erişilmesi gereken sayfalar
            var skipAuth = new[] { "Login", "Employee/Register", "Employee/Register/POST" };
            var currentController = filterContext.ActionDescriptor.ControllerDescriptor.ControllerName;
            var currentAction = filterContext.ActionDescriptor.ActionName;

            string controllerAction = $"{currentController}/{currentAction}";

            //Giriş yapılmadan erişilebilen sayfaları atla
            if (skipAuth.Contains(currentController) || skipAuth.Contains(controllerAction))
            {
                base.OnActionExecuting(filterContext);
                return;
            }
            // 1) Session’dan kullanıcı adı + son token oku
            var username = Session["Username"] as string;
            var token = Session["JWToken"] as string;

            // 2) Geçerli mi?
            bool sessionOkay = username != null &&
                               token != null &&
                               ActiveSessionManager.IsValid(username, token);

            // 3) Geçersizse → logout & yönlendir
            if (!sessionOkay)
            {
                // Bellek sözlüğünden de sil
                if (username != null) ActiveSessionManager.Remove(username);
               
                Session.Clear();
                Session.Abandon(); //oturumu tamamen sonlandırmak için kullanılır.

                if (filterContext.HttpContext.Request.IsAjaxRequest())
                {
                    // AJAX isteğine 401 Unauthorized dön
                    filterContext.HttpContext.Response.StatusCode = 401;
                    filterContext.Result = new JsonResult
                    {
                        Data = new { message = "Session expired" },
                        JsonRequestBehavior = JsonRequestBehavior.AllowGet
                    };
                }
                else
                {
                    filterContext.Result = new RedirectToRouteResult(
                        new RouteValueDictionary(new { controller = "Login", action = "Index" })
                    );
                }
            }

            base.OnActionExecuting(filterContext);
        }
    }
}
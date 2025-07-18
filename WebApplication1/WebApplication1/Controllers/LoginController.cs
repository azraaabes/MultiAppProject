using Newtonsoft.Json;
using System;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Security;
using WebApplication1.Helpers;
using WebApplication1.Models.Siniflar;
using Microsoft.Extensions.Caching.Distributed;

namespace WebApplication1.Controllers
{
    public class LoginController : Controller
    {
        private readonly HttpClient client;
        private string username;

        private readonly IDistributedCache _cache;
        public LoginController()
        {
            var baseUrl = ConfigurationManager.AppSettings["ApiBaseUrl"];
            client = new HttpClient();
            client.BaseAddress = new Uri(baseUrl); // Web API adresin

            // Authorization header'ını buraya ekle
            //var username = "azra";
            //var password = "azra12";
            //var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));
            //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64);
        }

        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
       public ActionResult Login()
        {
            ModelState.Clear();  // ModelState sıfırla, eski veriler kalmasın
            return View(); 
        }
        [HttpPost]
        public async Task<ActionResult> Login(Employee e)
        {    //AES ÇALIŞMASI SIRASINDA KODLARIM
             //string key = "mySecretKey12345";
             //string encryptedPassword = CryptoHelper.EncryptString(key, e.Password);
             //    var bilgiler = c.Employees.FirstOrDefault(x => x.Username == e.Username);
             //    if (bilgiler != null)
             //    {
             //        if (bilgiler.Password == encryptedPassword)
             //        {
             //            FormsAuthentication.SetAuthCookie(bilgiler.Username, false); // false parametresi, çerez (cookie)’in kalıcı olup olmayacağını belirtir.
             //            Session["Username"] = bilgiler.Username.ToString();
             //            return RedirectToAction("Index", "Employee");
             //        }
             //        else
             //        {
             //            // Şifre hatalı ama kullanıcı doğru → isim soyisim ile log yaz
             //            string fullName = bilgiler.Name + " " + bilgiler.Surname;
             //            LogEvents.LogToDatabase("Giriş Başarısız", "Şifre hatalı girildi", "Başarısız", bilgiler.Username);
             //        }
             //    }
             //    else
             //    {
             //        LogEvents.LogToDatabase("Hatalı", "Kullanıcı adı veya Şifre hatalı", "Başarısız", bilgiler.Username);

            //    }
            //    ViewBag.Mesaj = "<p style='color:red;'>Kullanıcı adı ya da şifre hatalı</p>";
            //    //System.Diagnostics.Debug.WriteLine("ELSE BLOĞUNA GİRİLDİ - Hatalı Giriş");
            //    return View("Login");

            //}

            //  JSON Nesnesi Oluşturuluyor,Web API'ye gönderilecek olan giriş verisi bu anonim nesneyle hazırlanıyor.

            ModelState.Clear();

            var loginDto = new
            {
                Username = e.Username,
                Password = e.Password
            };

            // JSON içeriği hazırla
            var json = JsonConvert.SerializeObject(loginDto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // API'ye gönder
            var response = await client.PostAsync("api/login", content);

            if (response.IsSuccessStatusCode)
            {
                var jsonResult = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine("JwtToken: " + jsonResult);
                dynamic result = JsonConvert.DeserializeObject(jsonResult);

                string username = result.username;
                Session["Username"] = username;
                Session["UserIP"] = GetClientIpAddress();
                string token = result.token; // JWT token burada geliyor
                Session["JWToken"] = token; // Token'ı Session'da saklıyoruz, istersen Cookie veya başka yere de koyabilirsin
                string roleId = result.roleId.ToString();
                Session["RoleId"] = roleId;

                /* — Redis’e koy (30 dk) — */
                await RedisCacheHelper.Cache.SetStringAsync(         // ADD
                    $"user:{username}:role",
                    roleId,
                    new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
                    });

                FormsAuthentication.SetAuthCookie(username, false);
                ActiveSessionManager.Set(username, token);
                //LogEvents.LogToDatabase("Giriş", "Kullanıcı giriş yaptı", "Başarılı", username);
                return RedirectToAction("Index", "Employee");
            }
            else
            {
                //LogEvents.LogToDatabase("Giriş Başarısız", "Şifre veya kullanıcı hatalı", "Başarısız", e.Username);
                ViewBag.Mesaj = "<p style='color:red;'>Kullanıcı adı ya da şifre hatalı</p>";
                return View();
            }
           
        }
        private string GetClientIpAddress()
        {
            string ip = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (!string.IsNullOrEmpty(ip))
            {
                ip = ip.Split(',')[0];
            }
            else
            {
                ip = Request.UserHostAddress;
            }

            return ip == "::1" ? "127.0.0.1" : ip; // localhost testleri için
        }
        public ActionResult Logout()
        {
            // 1) Kullanıcı adını önce Session’dan, sonra Identity’den dene
            var username = Session["Username"] as string ?? HttpContext.User.Identity.Name;

            if (!string.IsNullOrEmpty(username))
            {
                // 2) Çıkışı LOG’la
                LogEvents.LogToDatabase("Çıkış",
                                        "Sistemden Çıkış Yapıldı",
                                        "Başarılı",
                                        username);

                // 3) Aktif token’ı temizle
                ActiveSessionManager.Remove(username);
            }

            // 4) Forms oturumunu kapat
            FormsAuthentication.SignOut();

            // 5) Session’ı tamamen sil
            Session.Abandon();

            // 6) Login sayfasına yönlendir
            return RedirectToAction("Index", "Login");
        }
    }
}
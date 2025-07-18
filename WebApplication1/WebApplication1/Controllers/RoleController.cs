using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WebApplication1.Helpers;
using WebApplication1.Models.Siniflar;

namespace WebApplication1.Controllers
{
    public class RoleController : BaseController
    {
        private readonly HttpClient client;
        public RoleController()
        {
            var baseUrl = ConfigurationManager.AppSettings["ApiBaseUrl"];
            client = new HttpClient(); //Yeni nesne oluşturuldu Bu nesne, ileride Web API ile iletişim kurmak için kullanılır.
            client.BaseAddress = new Uri(baseUrl); // Web API base URL

            // Authorization header'ını buraya ekle
            //var username = "azra";
            //var password = "azra12";
            //var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));
            //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64);
        }
        private void AddBearerToken()
        {
            var token = Session["JWToken"] as string;
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            else
            {
                client.DefaultRequestHeaders.Authorization = null;
            }
        }
        [HttpGet]
        public async Task<ActionResult> Index()  //async: Metot asenkron çalışıyor, yani içinde await ile bekleme yapılabilir.Task<ActionResult>: Bu metot bir ActionResult (sayfa ya da JSON gibi) döner ama asenkron olduğu için Task sarmalı içinde döner.
        {
            AddBearerToken(); // Token ekle
            List<Role> roles = new List<Role>(); //roles adında boş bir role listesi yaratılıyor.Bu listeyi ileride API’den gelecek verilerle dolduracağız.
            var response = await client.GetAsync("api/Roles"); //await anahtarı ile bu işlem bitene kadar diğer işlemler engellenmeden bekleniyor.response içinde HTTP yanıtı tutuluyor.

            //// 3) DEBUG: Yanıt kodunu ve header’ı yazdır
            //System.Diagnostics.Debug.WriteLine(
            //    $"ROLE API → {(int)response.StatusCode} {response.ReasonPhrase}  |  Auth hdr: {client.DefaultRequestHeaders.Authorization}");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync(); //response.Content.ReadAsStringAsync() ile dönen JSON stringi okunur,API’den dönen cevabı string olarak alır (JSON formatında bir metin).
                roles = JsonConvert.DeserializeObject<List<Role>>(json); //JsonConvert.DeserializeObject<List<Employee>>(json) ile JSON, C#’daki List<Employee> nesnesine dönüştürülür,Böylece degerler artık gerçek çalışan verilerini tutar.

                //System.Diagnostics.Debug.WriteLine($"Json uzunluğu: {json.Length}, Role kaydı: {roles.Count}");
            }
            //else
            //{
            //    // Hata durumunu ViewBag’e geçir ki sayfada görebilesin
            //    ViewBag.ApiError = $"{(int)response.StatusCode} – {response.ReasonPhrase}";
            //}

            return View(roles);
        }
        [RoleAuthorize("3")]
        [HttpPost]
        public async Task<JsonResult> RoleEkle(Role role)
        {
            AddBearerToken(); // Token ekle
            var json = JsonConvert.SerializeObject(role); //Role nesnesi JSON formatına dönüştürülür.
            var content = new StringContent(json, Encoding.UTF8, "application/json"); //Yukarıda JSON’a çevirdiğin veriyi, HTTP POST isteğinde gönderilecek hale getiriyorsun."application/json": İçeriğin tipi JSON olarak belirtildi.

            var response = await client.PostAsync("api/roles", content); // Bu endpoint API'de olmalı!

            if (response.IsSuccessStatusCode)
            {
                return Json(new { success = true });
            }
            else
            {
                return Json(new { success = false, message = "API üzerinden rol eklenemedi." });
            }
}   }   }

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
using WebApplication1.Models.Siniflar;

namespace WebApplication1.Controllers
{
    public class LogController : BaseController
    {
        private readonly HttpClient client;

        public LogController()
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
        public async Task<ActionResult> Index(int page = 1)
        {
            AddBearerToken(); // Token ekle
            List<Log> logs = new List<Log>();
            int totalCount = 0;
            try
            {
                var response = await client.GetAsync($"api/log?page={page}&pageSize=10");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<LogResponseDto>(json);

                    if (result != null && result.Logs != null)
                        logs = result.Logs;
                        totalCount = result.TotalCount;
                }
                else
                {
                    ViewBag.Hata = $"API başarısız: {response.StatusCode}";
                }
            }
            catch (Exception ex)
            {
                ViewBag.Hata = $"Veri alınırken hata oluştu: {ex.Message}";
            }
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / 10); // 10 = pageSize
            return View(logs);
        }
    }
}
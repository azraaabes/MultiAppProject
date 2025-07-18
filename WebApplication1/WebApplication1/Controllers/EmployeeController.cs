using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Newtonsoft.Json;
using WebApplication1.Models.Siniflar;

namespace WebApplication1.Controllers
{
    public class EmployeeController : BaseController
    {
        private readonly HttpClient client;

        public EmployeeController()
        {
            var baseUrl = ConfigurationManager.AppSettings["ApiBaseUrl"];
            client = new HttpClient
            {
                BaseAddress = new Uri(baseUrl),
                Timeout = TimeSpan.FromSeconds(30)
            };
        }

        private void AddBearerToken()
        {
            var token = Session["JWToken"] as string;
            if (!string.IsNullOrWhiteSpace(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            else
            {
                client.DefaultRequestHeaders.Authorization = null;
            }
        }

        public async Task<ActionResult> Index()
        {
            AddBearerToken();
            var response = await client.GetAsync("api/employee/GetAllEmployees");
            var list = new List<Employee>();

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                list = JsonConvert.DeserializeObject<List<Employee>>(json);
            }

            return View(list);
        }

        [HttpGet]
        public async Task<ActionResult> Register()
        {
            await FillRoleDropdownAsync(); 
            return View(new Employee());
        }

        [HttpPost]
        public async Task<ActionResult> Register(Employee emp)
        {
            await FillRoleDropdownAsync();

            if (!ModelState.IsValid)
                return View(emp);

            var json = JsonConvert.SerializeObject(emp);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync("api/employee", content);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index", "Login");
            }

            ModelState.AddModelError("", "API üzerinden kayıt başarısız oldu.");
            return View(emp);
        }

        [HttpGet]
        public async Task<JsonResult> EmployeeGetir(int id)
        {
            AddBearerToken();

            try
            {
                var response = await client.GetAsync($"api/Employee/{id}");

                if (!response.IsSuccessStatusCode)
                {
                    return Json(new { error = $"API hata: {response.StatusCode}" }, JsonRequestBehavior.AllowGet);
                }

                var json = await response.Content.ReadAsStringAsync();
                var emp = JsonConvert.DeserializeObject<Employee>(json);

                if (emp == null)
                {
                    return Json(new { error = "Çalışan bulunamadı" }, JsonRequestBehavior.AllowGet);
                }

                var result = new
                {
                    emp.Employeeid,
                    emp.Name,
                    emp.Surname,
                    emp.Employeemail,
                    emp.Username,
                    emp.Password,
                    emp.Roleid
                };

                return Json(result, JsonRequestBehavior.AllowGet);
            }
            catch (TaskCanceledException)
            {
                return Json(new { error = "İstek zaman aşımına uğradı" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = "Bilinmeyen hata", detail = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public async Task<JsonResult> EmployeeGuncelle(Employee emp)
        {
            AddBearerToken();

            var json = JsonConvert.SerializeObject(emp);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PutAsync($"api/employee/{emp.Employeeid}", content);

            if (response.IsSuccessStatusCode)
            {
                return Json(new { success = true }, JsonRequestBehavior.AllowGet);
            }

            var errorMessage = await response.Content.ReadAsStringAsync();
            return Json(new { success = false, message = errorMessage }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<JsonResult> EmployeeEkle(Employee emp)
        {
            AddBearerToken();

            var json = JsonConvert.SerializeObject(emp);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync("api/employee", content);

            if (response.IsSuccessStatusCode)
            {
                return Json(new { success = true }, JsonRequestBehavior.AllowGet);
            }

            var error = await response.Content.ReadAsStringAsync();
            return Json(new { success = false, message = error }, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public async Task<JsonResult> RoleListesi()
        {
            AddBearerToken();
            var roles = new List<Role>();

            var response = await client.GetAsync("api/roles");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                roles = JsonConvert.DeserializeObject<List<Role>>(json);
            }

            return Json(roles, JsonRequestBehavior.AllowGet);
        }

        private async Task FillRoleDropdownAsync()
        {
            AddBearerToken();
            var response = await client.GetAsync("api/employee/roles");
            var deger = new List<SelectListItem>();

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var roleList = JsonConvert.DeserializeObject<List<Role>>(json);
                deger = roleList.Select(x => new SelectListItem
                {
                    Text = x.RoleName,
                    Value = x.Roleid.ToString()
                }).ToList();
            }

            ViewBag.dgr1 = deger;
        }
    }
}

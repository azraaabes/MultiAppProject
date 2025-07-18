using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using ConsoleClient.Models;
using System.Security.Cryptography;
using Hangfire.SqlServer;
using Hangfire;
using ConsoleClient.Helpers;
namespace ConsoleClient
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Run().GetAwaiter().GetResult();
        } 
        static async Task Run()
        {
            string baseUrl = "https://localhost:7235/";

            // Hangfire yapılandırması
            GlobalConfiguration.Configuration
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_170) //Hangfire’ın job verilerini geriye dönük uyumlu yazmasını sağlar.
                .UseSimpleAssemblyNameTypeSerializer() //Daha sade, versiyonsuz class adları ile veri yaz (serileştirme).
                .UseRecommendedSerializerSettings() //JSON yazarken önerilen ayarları kullanır.
                .UseSqlServerStorage("Server = DESKTOP-B2AQLAE\\SQLEXPRESS; Database = WebApp1;Trusted_Connection=True;", new SqlServerStorageOptions
                {
                    CommandBatchMaxTimeout = TimeSpan.FromMinutes(5), //Bu toplu komutların maksimum bekleme süresi (timeout) 5 dakika olarak ayarlanıyor.
                    SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5), //Hangfire, iş kuyruğundaki bir job’u alıp “işleniyor” (invisible) olarak işaretler. Bu sürede başka biri o job’u alamaz.
                    QueuePollInterval = TimeSpan.Zero, //Hangfire, yeni job var mı diye SQL kuyruğunu ne sıklıkla kontrol edecek?  TimeSpan.Zero demek, sürekli ve anında kontrol edeceği anlamına gelir.
                    UseRecommendedIsolationLevel = true, //Veri tutarlılığı ve performans arasında iyi bir denge sağlar.
                    DisableGlobalLocks = true //Kilit kullanımı azaltılır, bu da performansı artırabilir ancak bazı durumlarda veri tutarlılığı riskini artırabilir.
                });

            // Hangfire sunucusunu başlat
            using (var server = new BackgroundJobServer()) //BackgroundJobServer: Hangfire'ın arka planda görevleri (jobları) dinleyen ve çalıştıran bileşeni.
            {
                Console.WriteLine("Hangfire Server başlatıldı.");

                // Örnek bir Hangfire görevi: Her dakika çalışanları API'den çek
                RecurringJob.AddOrUpdate("fetch-employees", () => FetchEmployeesAsync(baseUrl), Cron.Minutely); //RecurringJob.AddOrUpdate: Zamanlanmış (tekrarlayan) bir görev oluşturur veya var olanı günceller.Cron.Minutely: Görevin her dakika çalışmasını sağlar.

                using (var http = new HttpClient { BaseAddress = new Uri(baseUrl) })
                {
                    var res = await http.GetAsync("api/employee/GetAllEmployees");
                    if (!res.IsSuccessStatusCode)
                    {
                        Console.WriteLine("Çalışanlar alınamadı. Hata: " + res.StatusCode);
                        Bekle();
                        return;
                    }
                    var json = await res.Content.ReadAsStringAsync();
                    var employees = JsonConvert.DeserializeObject<EmployeeDto[]>(json) ?? Array.Empty<EmployeeDto>();

                    Console.WriteLine($"\n{employees.Length} çalışan bulundu:\n");

                    foreach (var e in employees)
                    {
                        string formatted = JsonConvert.SerializeObject(e, Formatting.Indented);
                        Console.WriteLine(formatted);
                        Console.WriteLine(); // Araya boşluk bırakmak için
                    }
                }

                Bekle();
            }

            //// 1) Kullanıcı girişi
            //Console.Write("Kullanıcı adı: ");
            //var user = Console.ReadLine();
            //Console.Write("Şifre: ");
            //var pass = ReadPassword();

            //string baseUrl = "https://localhost:7235/";
            //string token = await GetJwtAsync(baseUrl, user, pass);
            //if (token == null)
            //{
            //    Console.WriteLine("Giriş başarısız.");
            //    Bekle(); return;
            //}

            // 2) Çalışan listesini al
            //using (var http = new HttpClient { BaseAddress = new Uri(baseUrl) })
            //{
            //    //http.DefaultRequestHeaders.Authorization =
            //    //    new AuthenticationHeaderValue("Bearer", token);

            //    var res = await http.GetAsync("api/employee"); //res → HTTP cevabını tutar.
            //    var json = await res.Content.ReadAsStringAsync(); //API'den gelen cevabı (response) ham olarak string (yazı) şeklinde okumaktır.res.Content API’den gelen cevabın gövdesi(body)..ReadAsStringAsync() Content gövdesini string (metin) olarak okur

            //    var employees = JsonConvert.DeserializeObject<EmployeeDto[]>(json)
            //                    ?? Array.Empty<EmployeeDto>();

            //    var emp = employees.FirstOrDefault(e =>
            //                 string.Equals(e.Username, user,
            //                               StringComparison.OrdinalIgnoreCase)); 
            //    if (emp == null)
            //    {
            //        Console.WriteLine("Kullanıcı listede bulunamadı.");
            //    }
            //    else
            //    {
            //        Console.WriteLine("\n—— Çalışan Bilgisi ——");
            //        var pretty = JsonConvert.SerializeObject(emp, Formatting.Indented); //emp nesnesini girintili JSON metnine çevirir (Postman’de gördüğün gibi).
            //        Console.WriteLine(pretty);  //Bu JSON’u ekrana basar.
            //    }
            //}

            //Bekle();
        }

        // Hangfire için çalışanları çeken görev
        public static async Task FetchEmployeesAsync(string baseUrl)
        {
            using (var http = new HttpClient { BaseAddress = new Uri(baseUrl) })
            {
                var res = await http.GetAsync("api/employee/GetAllEmployees");
                if (!res.IsSuccessStatusCode)
                {
                    Console.WriteLine("Hangfire görevi: Çalışanlar alınamadı. Hata: " + res.StatusCode);
                    return;
                }
                var json = await res.Content.ReadAsStringAsync();
                var employees = JsonConvert.DeserializeObject<EmployeeDto[]>(json) ?? Array.Empty<EmployeeDto>();

                Console.WriteLine($"\nHangfire görevi: {employees.Length} çalışan bulundu:\n");

                foreach (var e in employees)
                {
                    string formatted = JsonConvert.SerializeObject(e, Formatting.Indented);
                    Console.WriteLine(formatted);
                    Console.WriteLine();
                }
            }
        }

        // --- JWT alma
        //static async Task<string> GetJwtAsync(string baseUrl, string user, string pass)
        //{
        //    using (var http = new HttpClient { BaseAddress = new Uri(baseUrl) })
        //    {
        //        var payload = new { username = user, password = pass };
        //        var res = await http.PostAsJsonAsync("api/login", payload); //PostAsJsonAsync:payload’ı JSON’a çevirir, Content‑Type: application/json başlığıyla POST eder.res = HttpResponseMessage – durum kodu, başlıklar, gövde vs. içerir.

        //        if (!res.IsSuccessStatusCode) return null;

        //        dynamic body = JsonConvert.DeserializeObject(
        //                           await res.Content.ReadAsStringAsync()); //dynamic = Derleme zamanında tip kontrolü yok; çalışma anında body.token okunabilir.
        //        return (string)body.token;
        //    }
        //}

        // --- Gizli şifre okuma
        static string ReadPassword()
        {
            var pwd = string.Empty; //pwd: Kullanıcının girdiği karakterlerin biriktirileceği değişken.
            ConsoleKeyInfo key; //ConsoleKeyInfo key: Her tuşa basıldığında, basılan tuşun bilgisi burada tutulur.

            while ((key = Console.ReadKey(true)).Key != ConsoleKey.Enter)
            {
                if (key.Key == ConsoleKey.Backspace && pwd.Length > 0)
                {
                    pwd = pwd.Substring(0, pwd.Length - 1);
                    Console.Write("\b \b");
                }
                else if (!char.IsControl(key.KeyChar))
                {
                    pwd += key.KeyChar;
                    Console.Write('*');
                }
            }
            Console.WriteLine();
            return pwd;
        }

        static void Bekle()
        {
            Console.WriteLine("\nBitirmek için bir tuşa basın...");
            Console.ReadKey();
        }
    }
}
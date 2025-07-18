using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using Serilog;
using WebApplication1.Models.Siniflar;
using System.Web.Services.Description;
using Log = WebApplication1.Models.Siniflar.Log;


namespace WebApplication1
{

    public static class LogEvents //static olduğu için doğrudan LogEvents.LogToFile(...) şeklinde kullanılabilir. LogEvents,Tüm log işlemlerini kapsayan bir araç sınıfı.
    {
        public static void LogToDatabase(string Title, string LogMessage, string State,string username=null) //Log yazma işlemi bu metodla yapılır.   LogMessage: Asıl mesaj (örneğin “Toplam çalışan sayısı 5”).
        {
            try
            {
                using (var db = new Context())
                {

                    //string username = HttpContext.Current.User.Identity.IsAuthenticated
                    //    ? HttpContext.Current.User.Identity.Name
                    //    : "Anonim Kullanıcı";
                    //string fullName = username;

                    //int employeeId = 0; bundan dolayı null değeri alamıyorduk şöyle yazmamız gerek
                    //int? employeeId = null;

                    string fullName = "Anonim Kullanıcı";
                    int? employeeId = null;

                    if (!string.IsNullOrEmpty(username))
                    {
                        var employee = db.Employees.FirstOrDefault(e => e.Username == username);
                        if (employee != null)
                        {
                            fullName = employee.Name + " " + employee.Surname;
                            employeeId = employee.Employeeid;

                        }
                    }

                    var log = new Log
                    {
                        Title = Title,
                        Message = LogMessage,
                        Tarih = DateTime.Now,
                        User = fullName,
                        Employeeid = employeeId,
                        State = State
                        

                    };
                    db.Logs.Add(log);
                    db.SaveChanges();


                    //// App_Data klasörü içindeki dosyanın fiziksel yolu
                    //string Filename = HttpContext.Current.Server.MapPath("~/App_Data/Logfile.txt"); //MapPath: Sanal yolu, fiziksel disk yoluna çevirir

                    //StreamWriter swlog;

                    //if (!File.Exists(Filename))
                    //{
                    //    swlog = new StreamWriter(Filename);   // Dosya yoksa: oluştur
                    //}
                    //else
                    //{
                    //    swlog = File.AppendText(Filename);   // Dosya varsa:yaz
                    //}

                    //string username = HttpContext.Current.User.Identity.IsAuthenticated
                    //    ? HttpContext.Current.User.Identity.Name
                    //    : "Anonim Kullanıcı";
                    //string fullName = username;
                    //if (username != "Anonim Kullanıcı")
                    //{
                    //    using (var db = new Context())
                    //    {
                    //        var employee = db.Employees.FirstOrDefault(e => e.Username == username);
                    //        if (employee != null)
                    //        {
                    //            fullName = employee.Name + " " + employee.Surname;
                    //        }
                    //    }
                    //}
                    //string id = HttpContext.Current.User.Identity.IsAuthenticated
                    //   ? HttpContext.Current.User.Identity.Name
                    //   : "Anonim Kullanıcı id";
                    //string idnumber = id;
                    //if (id != "Anonim Kullanıcı id")
                    //{
                    //    using (var db = new Context())
                    //    {
                    //        var employee = db.Employees.FirstOrDefault(e => e.Username == username);
                    //        if (employee != null)
                    //        {
                    //            idnumber = employee.Employeeid.ToString();
                    //        }
                    //    }
                    //}
                    //swlog.WriteLine("Log Entry");
                    //swlog.WriteLine("{0} {1}", DateTime.Now.ToLongDateString(), DateTime.Now.ToLongTimeString());
                    //swlog.WriteLine("Kullanıcı: " + fullName);
                    //swlog.WriteLine("İd: " + idnumber);
                    //swlog.WriteLine("Başlık: " + Title);
                    //swlog.WriteLine("Mesaj: " + LogMessage);
                    //swlog.WriteLine(); // boş satır ekle
                    //swlog.Close();
                }
            }
            catch (Exception ex)
            {
                // İstersen buraya hata mesajı yazdırabilirsin
                // Örneğin: System.Diagnostics.Debug.WriteLine(ex.Message);

                // Hata loglama mekanizması varsa buraya yaz
                //System.Diagnostics.Debug.WriteLine(ex.Message);
                throw;
            }
            
        }
    }
}
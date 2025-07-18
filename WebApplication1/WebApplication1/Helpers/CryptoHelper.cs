using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace WebApplication1.Helpers
{
    public class CryptoHelper
    {

        // Bu metot key’i AES’e uygun uzunluğa getirir (32 karakter)
        private static byte[] GetAesKey(string key) //string key =mysecretkey123 yazmıştık ya o
        {
            return Encoding.UTF8.GetBytes(key.PadRight(32).Substring(0, 32)); //.PadRight(32) → Key’i 32 karaktere tamamlar yani eksikse boşluk ekler,.Substring(0, 32) → 32 karakteri geçerse keser yani ilk 32 karakteri alır, gerisini atar.,Encoding.UTF8.GetBytes(...) → String’i byte dizisine çevirir.
        }

        public static string EncryptString(string key, string plainText) //Kullanıcının düz yazı şifresini (örnek: "887") şifrelenmiş bir string'e çevirmek.
        {
            byte[] iv = new byte[16]; //IV (Initialization Vector = random başlangıç değeri): AES şifrelemede başlangıç değeri-başlangıçta 0'larla dolu.
            byte[] array; //Şifrelenmiş veriyi buraya yazacağız (şifreli sonuç burada tutulacak).

            using (Aes aes = Aes.Create()) //Bir AES algoritması nesnesi oluşturuyoruz.
            {
                aes.Key = GetAesKey(key);  //AES için gerekli olan Key ve IV ayarlanıyor.
                aes.IV = iv;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV); //Şifrelemek için kullanılacak dönüştürücü nesne oluşturuluyor.ICryptoTransform  bir arayüzdür (interface). Bu interface, veri üzerinde şifreleme ya da şifre çözme işlemlerini yapan nesneleri temsil eder.



                using (MemoryStream memoryStream = new MemoryStream()) //Şifrelenmiş veriyi geçici olarak bellekte tutmak için bir MemoryStream kullanılır.
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, encryptor, CryptoStreamMode.Write)) // Bu akış (stream), şifreleme işlemini gerçek zamanlı olarak yapar.MemoryStream’in üzerine yazacağız, ama yazarken veriyi şifreleyecek.
                    {
                        using (StreamWriter streamWriter = new StreamWriter((Stream)cryptoStream))
                        {
                            streamWriter.Write(plainText); //plainText → yazılıyor (örneğin "123"),Ancak bu metin, cryptoStream üzerinden şifrelenerek memoryStream içine yazılıyor.
                            streamWriter.Flush(); //Yazıyı tamamen işle
                        }

                        array = memoryStream.ToArray(); //Ancak bu metin, cryptoStream üzerinden şifrelenerek memoryStream içine yazılıyor.
                    }
                }
            }

            return Convert.ToBase64String(array); //Şifrelenmiş byte dizisini string’e çevirir.Böylece veritabanına yazabileceğimiz bir string elde ederiz.
        }
        public static string DecryptString(string key, string cipherText) //Bu metodun amacı: Şifrelenmiş string'i tekrar düz yazıya çevirmektir.
        {
            byte[] iv = new byte[16]; //Şifrelenmiş string'i tekrar byte dizisine çeviririz.
            byte[] buffer = Convert.FromBase64String(cipherText); //Aynı IV’yi (sıfırlarla dolu 16 byte) tekrar kullanmamız gerekir,Şifreli veri.


            using (Aes aes = Aes.Create())
            {
                aes.Key = GetAesKey(key);
                aes.IV = iv;
                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV); //AES şifre çözücü nesnesi oluşturuluyor.

                using (MemoryStream memoryStream = new MemoryStream(buffer)) //Şifrelenmiş veriyi çözmek için belleğe aktarılır
                {
                    using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, decryptor, CryptoStreamMode.Read)) //Şifre çözme işlemi burada yapılır.
                    {
                        using (StreamReader streamReader = new StreamReader((Stream)cryptoStream))  //Şifrelenmiş veri okunur ve çözülmüş hali düz yazı olarak döner.
                        {
                            return streamReader.ReadToEnd();
                        }
                    }
                }
            }
        }
    }
}
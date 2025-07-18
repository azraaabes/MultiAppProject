using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NLog;
using System.Linq;
using Webapiproje.DataDbContext;
using Webapiproje.Helpers;
using Webapiproje.Models;
using Webapiproje.Authentication;
using Microsoft.AspNetCore.Authorization;
namespace Webapiproje.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly IJwtTokenService _jwtTokenService;
        public LoginController(ApplicationDbContext context, IJwtTokenService jwtTokenService)
        {
            _context = context;
            _jwtTokenService = jwtTokenService;
        }
        [AllowAnonymous]
        [HttpPost]
        public IActionResult Login([FromBody] LoginDto dto)
        {
            string key = "mySecretKey12345";
            string encryptedPassword = CryptoHelper.EncryptString(key, dto.Password);

            var user = _context.Employees.FirstOrDefault(x => x.Username == dto.Username);

            if (user != null && user.Password == encryptedPassword)
            {
                var roleId = user.Roleid;
                var token = _jwtTokenService.GenerateToken(user.Username, roleId.ToString());
                
                // Başarılı giriş log'u
                var logEvent = new LogEventInfo(NLog.LogLevel.Info, "Webapiproje.Controllers.LoginController", "Giriş yapıldı.");
                logEvent.Properties["Title"] = "Kullanıcı Girişi";
                logEvent.Properties["Employeeid"] = user.Employeeid;
                logEvent.Properties["User"] = $"{user.Name} {user.Surname}";
                logEvent.Properties["State"] = "Başarılı";
                _logger.Log(logEvent);

                return Ok(new
                {
                    success = true,
                    username = user.Username,
                    name = user.Name,
                    surname = user.Surname,
                    token = token,
                    roleId = roleId
                });
            }
            else
            {
                // Başarısız giriş log'u (isteğe bağlı)
                var emp = _context.Employees.FirstOrDefault(x => x.Username == dto.Username);
                string isimSoyisim = emp != null ? $"{emp.Name} {emp.Surname}" : "Bilinmeyen Kullanıcı";
                var logEvent = new LogEventInfo(NLog.LogLevel.Warn, "Webapiproje.Controllers.LoginController", "Giriş başarısız.");
                logEvent.Properties["Title"] = "Kullanıcı Girişi";
                logEvent.Properties["Employeeid"] = emp?.Employeeid;
                logEvent.Properties["User"] = isimSoyisim;
                logEvent.Properties["State"] = "Başarısız";
          
                _logger.Log(logEvent);
            }           

                return Unauthorized(new { success = false, message = "Kullanıcı adı ya da şifre hatalı" });
            
        }
    }
}

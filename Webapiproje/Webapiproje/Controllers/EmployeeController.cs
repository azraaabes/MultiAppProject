using Microsoft.AspNetCore.Http;
using Webapiproje.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Webapiproje.DataDbContext;
using Webapiproje.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Net;



namespace Webapiproje.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController : ControllerBase
    {
        //private readonly IMemoryCache _cache;
        private readonly IDistributedCache _cache;
        private readonly ApplicationDbContext _context;

        private const string EmployeeListKey = "employees_all";
        private static string EmployeeByIdKey(int id) => $"employee_{id}";
        private const string RoleListKey = "roles_all";
        // 3) Cache’e ekle
        private readonly DistributedCacheEntryOptions cacheOptions =
           new DistributedCacheEntryOptions()
           .SetSlidingExpiration(TimeSpan.FromMinutes(5))
           .SetAbsoluteExpiration(TimeSpan.FromMinutes(20));

        private static readonly JsonSerializerOptions JsonOpts = new()
        {
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            WriteIndented = false
        };
        public EmployeeController(/*IMemoryCache cache*/ IDistributedCache cache, ApplicationDbContext context)
        {
            _cache = cache;
            _context = context;

        }

      
        // GET: api/employee
        [HttpGet("GetAllEmployees")] 
        [AllowAnonymous]
        public async Task<IActionResult> GetAllEmployees()
        {

            // 1) Cache'ten JSON string oku
            var json = await _cache.GetStringAsync(EmployeeListKey);
            if (!string.IsNullOrEmpty(json))
            {
                Console.WriteLine("Cache'den geldi (GetAllEmployees)");
                var cached = JsonSerializer.Deserialize<List<Employee>>(json, JsonOpts);
                return Ok(cached);              // → Cache HIT
            }

            Console.WriteLine("Db'den geldi (GetAllEmployees)");
            var employees = await _context.Employees.Include(l => l.Role).AsNoTracking().ToListAsync();
            var employeesString = JsonSerializer.Serialize(employees,JsonOpts);
            await _cache.SetStringAsync(EmployeeListKey, employeesString, cacheOptions);
            return Ok(employees);


            // IMEMORY KISMI İÇİN BU
            //// 1) Cache’te var mı?
            //if (_cache.TryGetValue(EmployeeListCacheKey, out List<Employee> employees))
            //{
            //    Console.WriteLine("CACHE'TEN GELDİ");
            //    return Ok(employees);             // → Cache HIT
            //}
            //Console.WriteLine("VERİTABANINDAN GELDİ");
            //// 2) Cache MISS → DB’ye git
            //employees = await _context.Employees
            //                           .Include(e => e.Role)
            //                           .ToListAsync();

            //_cache.Set(EmployeeListCacheKey, employees, cacheOptions);

            //return Ok(employees);
        }

        // GET: api/employee/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetEmployeeById(int id)
        {
            try
            {
                Console.WriteLine("Db'den Geldi  (GetEmployeeById)");

                var emp = await _context.Employees
                                        .Include(e => e.Role)
                                        .AsNoTracking()
                                        .FirstOrDefaultAsync(x => x.Employeeid == id);

                if (emp == null)
                {
                    return NotFound(new
                    {
                        status = 404,
                        Message = $"No employee found with ID = {id}",
                        Error = "Employee ID Not Found"
                    });
                }

                // Cacheleme işlemi kaldırıldı
                // await _cache.SetStringAsync(EmployeeByIdKey(id),
                //     JsonSerializer.Serialize(emp, JsonOpts),
                //     cacheOptions);

                return StatusCode(StatusCodes.Status200OK, emp);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Beklenmeyen bir hata oluştu: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, new
                {
                    status = 500,
                    Message = "An unexpected error occurred.",
                    Detail = ex.Message
                });
            }

            //Cache'ten çekiyor var olan verileri
            //try
            //{
            //    var json = await _cache.GetStringAsync(EmployeeByIdKey(id));
            //    if (!string.IsNullOrEmpty(json))
            //    {
            //        try
            //        {
            //            Console.WriteLine("Cache'den geldi  (GetEmployeeById)");
            //            var cached = JsonSerializer.Deserialize<Employee>(json, JsonOpts);
            //            return Ok(cached); // → Cache HIT
            //        }
            //        catch (JsonException)
            //        {
            //            await _cache.RemoveAsync(EmployeeByIdKey(id));
            //            Console.WriteLine("Bozuk cache silindi, DB'den çekilecek");
            //        }
            //    }

            //    Console.WriteLine("Db'den Geldi  (GetEmployeeById)");
            //    var emp = await _context.Employees
            //                            .Include(e => e.Role)
            //                            .AsNoTracking()
            //                            .FirstOrDefaultAsync(x => x.Employeeid == id);

            //    if (emp == null)
            //    {
            //        return NotFound(new
            //        {
            //            status= 404,
            //            Message = $"No employee found with ID = {id}",
            //            Error = "Employee ID Not Found"
            //        });
            //    }

            //    await _cache.SetStringAsync(EmployeeByIdKey(id),
            //        JsonSerializer.Serialize(emp, JsonOpts),
            //        cacheOptions);

            //    return StatusCode(StatusCodes.Status200OK, emp); // → DB HIT
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine($"Beklenmeyen bir hata oluştu: {ex.Message}");
            //    return StatusCode(StatusCodes.Status500InternalServerError, new
            //    {
            //        status=500,
            //        Message = "An unexpected error occurred.",
            //        Detail = ex.Message
            //    });
            //}






            //if (emp == null)
            //return NotFound();

            //await _cache.SetStringAsync(EmployeeByIdKey(id),
            //   JsonSerializer.Serialize(emp, JsonOpts),
            //   cacheOptions);





            //IMEMORYCACHE İÇİN KULLANDIĞIM KODLAR
            //if (_cache.TryGetValue(EmployeeByIdCacheKey(id), out Employee empCached))
            //{
            //    return Ok(empCached);
            //}

            //var emp = await _context.Employees
            //                        .AsNoTracking()
            //                        .FirstOrDefaultAsync(x => x.Employeeid == id);

            //if (emp == null)
            //    return NotFound();

            //_cache.Set(EmployeeByIdCacheKey(id), emp, cacheOptions);

            //return Ok(emp);


            //// Veritabanından, id'ye göre çalışan aranıyor.
            //var emp = _context.Employees.FirstOrDefault(x => x.Employeeid == id);

            //if (emp == null)
            //    return NotFound();

            //// Çalışan nesnesinden sadece belli alanlar seçilip anonim bir nesne oluşturuluyor.
            //var result = new
            //{
            //    emp.Employeeid,
            //    emp.Name,
            //    emp.Surname,
            //    emp.Employeemail,               
            //    emp.Roleid
            //};
            //// Başarılı durumlarda HTTP 200 OK kodu ile birlikte sonuç JSON olarak döndürülüyor.
            //return Ok(result);
        }

        // POST: api/employee
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> AddEmployee([FromBody] EmployeeCreateDto emp)
        {
            if (!ModelState.IsValid) 
                return BadRequest(ModelState);

            const string aesKey = "mySecretKey12345";

            var employee = new Employee
            {
                Name = emp.Name,
                Surname = emp.Surname,
                Employeemail = emp.Employeemail,
                Username = emp.Username,
                Roleid = emp.Roleid,
                Password = CryptoHelper.EncryptString(aesKey, emp.Password)
            };

            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

            // Liste cache'ini temizle
            await _cache.RemoveAsync(EmployeeListKey);

            return Ok(employee);
            //IMEMORYCACHE İÇİN KULLANDIĞIM KODLAR
            //if (!ModelState.IsValid)
            //    return BadRequest(ModelState);

            //const string key = "mySecretKey12345";

            //var employee = new Employee
            //{
            //    Name = emp.Name,
            //    Surname = emp.Surname,
            //    Employeemail = emp.Employeemail,
            //    Username = emp.Username,
            //    Roleid = emp.Roleid,
            //    Password = CryptoHelper.EncryptString(key, emp.Password)
            //};

            //_context.Employees.Add(employee);
            //await _context.SaveChangesAsync();

            //// Cache invalidate
            //_cache.Remove(EmployeeListCacheKey);

            //return Ok(employee);


            //if (!ModelState.IsValid)
            //    return BadRequest(ModelState);

            //string key = "mySecretKey12345";
            //// DTO'dan entity nesnesi oluştur
            //var employee = new Employee
            //{
            //    Name = emp.Name,
            //    Surname = emp.Surname,
            //    Employeemail = emp.Employeemail,
            //    Username = emp.Username,
            //    Roleid = emp.Roleid,
            //    Password = CryptoHelper.EncryptString(key, emp.Password)
            //};

            //_context.Employees.Add(employee);
            //_context.SaveChanges();

            //return Ok(employee);
        }

        // PUT: api/employee/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEmployee(int id, [FromBody] EmployeeUpdateDto emp)
        {
            //Imemory ile aynı redis bir değişiklik yapmadık
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var mevcut = await _context.Employees.FindAsync(id);
            if (mevcut == null)
                return NotFound();

            mevcut.Name = emp.Name;
            mevcut.Surname = emp.Surname;
            mevcut.Employeemail = emp.Employeemail;
            mevcut.Roleid = emp.Roleid;

            await _context.SaveChangesAsync();

            // Cache invalidate
            await _cache.RemoveAsync(EmployeeListKey);
            await _cache.RemoveAsync(EmployeeByIdKey(id));

            return Ok(emp);
            //if (!ModelState.IsValid)
            //    return BadRequest(ModelState);

            //var mevcut = _context.Employees.Find(id);
            //if (mevcut == null)
            //    return NotFound();

            //mevcut.Name = emp.Name;
            //mevcut.Surname = emp.Surname;
            //mevcut.Employeemail = emp.Employeemail;
            //mevcut.Roleid = emp.Roleid;

            //try
            //{
            //    _context.SaveChanges();
            //}
            //catch (Exception ex)
            //{
            //    return BadRequest("Kaydetme hatası: " + ex.Message);
            //}
            //return Ok(mevcut);
        }

        // GET: api/employee/roles
        [AllowAnonymous]
        [HttpGet("roles")]
        public async Task<IActionResult> GetRoles()
        {
            var json = await _cache.GetStringAsync(RoleListKey);
            if (!string.IsNullOrEmpty(json))
            {
                Console.WriteLine("Cache'den geldi (GetRoles)");
                var cachedRoles = JsonSerializer.Deserialize<List<Role>>(json, JsonOpts);
                return Ok(cachedRoles);
            }
            Console.WriteLine("Db'den geldi (GetRoles)");
            var roles = await _context.Roles
                                   .AsNoTracking()
                                  .Select(r => new { r.Roleid, r.RoleName })
                                  .ToListAsync();


            await _cache.SetStringAsync(RoleListKey,
               JsonSerializer.Serialize(roles, JsonOpts),
               cacheOptions);
            return Ok(roles);

            //IMEMORY KISMI İÇİN
            //if (_cache.TryGetValue(RoleListCacheKey, out List<object> rolesCached))
            //{
            //    return Ok(rolesCached);
            //}

            //var roles = await _context.Roles
            //                          .AsNoTracking()
            //                          .Select(r => new { r.Roleid, r.RoleName })
            //                          .ToListAsync();

            //_cache.Set(RoleListCacheKey, roles, cacheOptions);
            //return Ok(roles);


            //var roles = _context.Roles
            //    .Select(r => new { r.Roleid, r.RoleName })
            //    .ToList();

            //return Ok(roles);
        }

    }
}

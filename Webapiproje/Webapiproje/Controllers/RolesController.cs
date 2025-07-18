using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Webapiproje.DataDbContext; // DbContext namespace'in
using Webapiproje.Models;        // Role modelinin namespace'i
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace Webapiproje.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class RolesController : ControllerBase
    {
        //private readonly IMemoryCache _cache;
        private readonly IDistributedCache _cache;
        private readonly ApplicationDbContext _context;
        private const string RoleListCacheKey = "roleList";

        //private static MemoryCacheEntryOptions cacheOptions =>
        //    new MemoryCacheEntryOptions()
        //                    .SetSlidingExpiration(TimeSpan.FromMinutes(5))   // 5 dk erişilmezse sil
        //                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(30)); // max 30 dk kalsın

        private readonly DistributedCacheEntryOptions cacheOptions =
            new DistributedCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromMinutes(2))
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(10));
        public RolesController(ApplicationDbContext context,IDistributedCache cache /*IMemoryCache cache*/)
        {
            _context = context;
            _cache = cache;
        }

        // GET: api/roles
        [HttpGet]
        public async Task<IActionResult> GetAllRoles()
        {
            //Cache'te var mı önce onu kontrol edelim
            var CachedRolesString = await _cache.GetStringAsync(RoleListCacheKey);
            if (!string.IsNullOrEmpty(CachedRolesString))
            {
                Console.WriteLine("Cache'den geldi (GetAllRoles)");
                var cachedRoles = JsonSerializer.Deserialize<List<Role>>(CachedRolesString);
                return Ok(cachedRoles);
            }

            Console.WriteLine("Db'den geldi (GetAllRoles)");
            //eğer yoksa db'den çekicez
            var roles = await _context.Roles.AsNoTracking().ToListAsync();

            var rolesString = JsonSerializer.Serialize(roles);
            await _cache.SetStringAsync(RoleListCacheKey, rolesString, cacheOptions);

            return Ok(roles);
            //BURASI IMEMORYCACHE İÇİN KISIM
            //// 1) Cache’te var mı?
            //if (_cache.TryGetValue(RoleListCacheKey, out List<Role> roles))
            //{
            //    Console.WriteLine("CACHE'TEN GELDİ");
            //    return Ok(roles);             // → Cache HIT
            //}
            //Console.WriteLine("VERİTABANINDAN GELDİ");

            //roles = await _context.Roles.ToListAsync();

            //_cache.Set(RoleListCacheKey, roles, cacheOptions);

            //return Ok(roles);


            //var roles = _context.Roles.ToList();
            //return Ok(roles);
        }

        // POST: api/roles
        [HttpPost]
        public async Task<IActionResult> AddRole([FromBody] Role role)
        {
            if (role == null)
            {
                return BadRequest();
            }

            _context.Roles.Add(role);
            await _context.SaveChangesAsync();

            // Yeni rol eklendi → rol listesinin cache’ini temizle
            await _cache.RemoveAsync(RoleListCacheKey);
            return Ok(role);

            //if (role == null)
            //    return BadRequest();

            //_context.Roles.Add(role);
            //_context.SaveChanges();
            //return Ok(role);
        }
    }
}

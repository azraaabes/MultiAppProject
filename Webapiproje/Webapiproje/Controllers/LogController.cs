using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using Webapiproje.DataDbContext;
using Webapiproje.Models;

namespace Webapiproje.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LogController : ControllerBase
    {
        private readonly IDistributedCache _cache;
        private readonly ApplicationDbContext _context;
        private const string LogListCacheKey = "logs_all";

        private readonly DistributedCacheEntryOptions cacheOptions =
           new DistributedCacheEntryOptions()
           .SetSlidingExpiration(TimeSpan.FromMinutes(2))
           .SetAbsoluteExpiration(TimeSpan.FromMinutes(10));
        public LogController(ApplicationDbContext context, IDistributedCache cache)
        {
            _context = context;
            _cache = cache;
        }

        //// GET: api/log
        //[HttpGet]
        //public async Task<IActionResult> GetLogs()
        //{

        //    var CachedLogString = await _cache.GetStringAsync(LogListCacheKey);
        //    if (!string.IsNullOrEmpty(CachedLogString))
        //    {
        //        Console.WriteLine("Cache'den geldi (GetLogs)");
        //        var cachedLogs = JsonSerializer.Deserialize<List<Log>>(CachedLogString);
        //        return Ok(cachedLogs);
        //    }

        //    Console.WriteLine("Db'den geldi (GEtLogs)");
        //    var logs = await _context.Logs.Include(l => l.Employee).AsNoTracking().ToListAsync();
        //    var logsString = JsonSerializer.Serialize(logs);
        //    await _cache.SetStringAsync(LogListCacheKey, logsString, cacheOptions);
        //    return Ok(logs);


        //    //var logs = _context.Logs.Include(e => e.Employee).ToList();
        //    //return Ok(logs);
        //}

        [HttpGet]
        public async Task<IActionResult> GetLogs(int page = 1, int pageSize = 10)
        {
            var cachedString = await _cache.GetStringAsync(LogListCacheKey);
            if (!string.IsNullOrEmpty(cachedString))
            {
                Console.WriteLine("Cache'den geldi (GetLogs)");
                var allLogs = JsonSerializer.Deserialize<List<Log>>(cachedString);

                var pagedLogs = allLogs
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                return Ok(new
                {
                    TotalCount = allLogs.Count,
                    Logs = pagedLogs
                });
            }

            Console.WriteLine("Db'den geldi (GetLogs)");

            var logsFromDb = await _context.Logs
                .Include(l => l.Employee)
                .AsNoTracking()
                .Select(l => new Log
                {
                    LogId = l.LogId,
                    Title = l.Title,
                    Message = l.Message,
                    Tarih = l.Tarih,
                    Employeeid = l.Employeeid,
                    State = l.State,
                    User = l.User
                })
                .ToListAsync();

            var serialized = JsonSerializer.Serialize(logsFromDb);
            await _cache.SetStringAsync(LogListCacheKey, serialized, cacheOptions);

            var pagedResult = logsFromDb
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return Ok(new
            {
                TotalCount = logsFromDb.Count,
                Logs = pagedResult
            });
        }
    }
}

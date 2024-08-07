using DynamoSoft_Task.Context;
using DynamoSoft_Task.Models;
using DynamoSoft_Task.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySqlX.XDevAPI;
using System.Collections.Concurrent;

namespace DynamoSoft_Task.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CryptoController : ControllerBase
    {
        private readonly PortfolioDataContext _context;
        private readonly ICryptoService _cryptoService;
        private readonly ILogger<CryptoController> _logger;

        public CryptoController(PortfolioDataContext context, ICryptoService cryptoService, ILogger<CryptoController> logger)
        {
            _context = context;
            _cryptoService = cryptoService;
            _logger = logger;
        }

        [HttpPost("intervalUpdate")]
        public IActionResult SetUpdateInterval([FromBody] int intervalMinutes)
        {
            if (intervalMinutes <= 0)
            {
                return BadRequest("Interval must be greater than zero.");
            }

            // Generate a new session ID
            var sessionId = Request.Cookies["SessionId"];

            if (string.IsNullOrEmpty(sessionId))
            {
                return BadRequest("Session ID is required.");
            }

            var interval = TimeSpan.FromMinutes(intervalMinutes);
            _cryptoService.SetUpdateInterval(interval, sessionId);

            Response.Cookies.Append("SessionId", sessionId, new CookieOptions
            {
                HttpOnly = true,
                SameSite = SameSiteMode.Lax,
                Secure = false, // Use 'true' if using HTTPS
            });

            return Ok($"Update interval set to {intervalMinutes} minutes.");
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadPortfolio([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("File is empty");

            // Generate a new session ID
            var sessionId = Request.Cookies["SessionId"];

            if (string.IsNullOrEmpty(sessionId))
            {
                return BadRequest("Session ID is required.");
            }

            using (var stream = new StreamReader(file.OpenReadStream()))
            {
                string line;
                var entries = new List<PortfolioEntry>();

                while ((line = await stream.ReadLineAsync()) != null)
                {
                    var parts = line.Split('|');
                    if (parts.Length != 3) continue;

                    var entry = new PortfolioEntry
                    {
                        Quantity = decimal.Parse(parts[0]),
                        Symbol = parts[1],
                        InitialBuyPrice = decimal.Parse(parts[2]),
                        SessionId = sessionId
                    };

                    entries.Add(entry);
                }

                _context.PortfolioEntries.AddRange(entries);
                await _context.SaveChangesAsync();
            }

            //Set the session ID in a cookie
            Response.Cookies.Append("SessionId", sessionId, new CookieOptions
            {
                HttpOnly = true,
                SameSite = SameSiteMode.Lax,
                Secure = false, // Use 'true' if using HTTPS
                Domain = "localhost:5013",
                Path = "/"
            });

            return Ok("File uploaded successfully");
        }

        [HttpGet("value")]
        public async Task<IActionResult> GetPortfolioValue()
        {
            var sessionId = Request.Cookies["SessionId"];

            if (string.IsNullOrEmpty(sessionId))
            {
                return BadRequest("Session ID is required.");
            }

            var entries = await _context.PortfolioEntries
                            .Where(e => e.SessionId == sessionId)
                            .ToListAsync();

            if (entries == null || !entries.Any())
            {
                return NotFound("No portfolio entries found for the given session ID.");
            }

            decimal initialTotalValue = 0;
            decimal currentTotalValue = 0;

            // Fetch symbols from PortfolioEntries and get their corresponding IDs from SymbolData
            var symbols = entries.Select(e => e.Symbol).ToList();
            var symbolDataList = _context.SymbolData.Where(s => symbols.Contains(s.Name)).ToList();
            var ids = symbolDataList.Select(s => s.Id).ToList();

            // Get crypto data for the needed IDs
            var cryptoData = await _cryptoService.GetCryptoDataAsync(ids);

            var portfolioDetails = _cryptoService.CalculatePortfolioValue(entries, cryptoData, symbolDataList);

            //Set the session ID in a cookie
            Response.Cookies.Append("SessionId", sessionId, new CookieOptions
            {
                HttpOnly = true,
                SameSite = SameSiteMode.Lax,
                Secure = false, // Use 'true' if using HTTPS
            });

            return Ok(portfolioDetails);
        }
    }
}

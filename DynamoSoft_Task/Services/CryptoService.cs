using DynamoSoft_Task.Context;
using DynamoSoft_Task.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace DynamoSoft_Task.Services
{
    public class CryptoService : ICryptoService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<CryptoService> _logger;
        private readonly IBackgroundTaskQueue _taskQueue;
        private readonly IHubContext<PortfolioHub> _hubContext;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private const int PageSize = 100;
        private readonly Dictionary<string, TimeSpan> _userIntervals = new Dictionary<string, TimeSpan>();


        public CryptoService(IHttpClientFactory httpClientFactory, ILogger<CryptoService> logger, IBackgroundTaskQueue taskQueue, IHubContext<PortfolioHub> hubContext, IServiceScopeFactory serviceScopeFactory)
        {
            _httpClientFactory = httpClientFactory;
            _hubContext = hubContext;
            _logger = logger;
            _taskQueue = taskQueue;
            _serviceScopeFactory = serviceScopeFactory;
        }

        private HttpClient CreateHttpClient() => _httpClientFactory.CreateClient();

        public async Task SeedCoinsAsync()
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var _context = scope.ServiceProvider.GetRequiredService<PortfolioDataContext>();
                var httpClient = CreateHttpClient();

                try
                {
                    int start = 0;
                    bool hasMore = true;

                    while (hasMore)
                    {
                        var response = await httpClient.GetStringAsync($"https://api.coinlore.net/api/tickers/?start={start}&limit={PageSize}");
                        var jsonDoc = JsonDocument.Parse(response);
                        var symbols = jsonDoc.RootElement.GetProperty("data").EnumerateArray();

                        if (!symbols.Any())
                        {
                            break;
                        }

                        foreach (var symbol in symbols)
                        {
                            var coinEntity = new SymbolData
                            {
                                Id = Int32.Parse(symbol.GetProperty("id").GetString()),
                                Name = symbol.GetProperty("symbol").GetString()
                            };

                            // Check if entity is already tracked or present in the database
                            var existingEntity = _context.SymbolData.Local.FirstOrDefault(c => c.Id == coinEntity.Id);
                            if (existingEntity != null)
                            {
                                _logger.LogWarning($"Skipping entity with duplicate ID {coinEntity.Id} and Name {coinEntity.Name}");
                            }
                            else if (!_context.SymbolData.Any(c => c.Id == coinEntity.Id))
                            {
                                _context.SymbolData.Add(coinEntity);
                            }
                            else
                            {
                                _logger.LogWarning($"Duplicate entity detected: ID {coinEntity.Id}, Name {coinEntity.Name}");
                            }

                        }

                        var info = jsonDoc.RootElement.GetProperty("info");
                        var totalCoins = info.GetProperty("coins_num").GetInt32();

                        start += PageSize;
                        hasMore = start < totalCoins;
                    }

                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Coins data seeded successfully.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to seed coin data.");
                }
            }
        }

        public async Task<Dictionary<string, CryptoData>> GetCryptoDataAsync(List<int> symbol_ids)
         {
            var cryptoData = new Dictionary<string, CryptoData>();
            var httpClient = CreateHttpClient();
            Random rnd = new Random();

            foreach (var id in symbol_ids)
            {
                var response = await httpClient.GetStringAsync($"https://api.coinlore.net/api/ticker/?id={id}");
                var jsonDoc = JsonDocument.Parse(response);
                var symbol = jsonDoc.RootElement[0];

                var data = new CryptoData
                {
                    Symbol = symbol.GetProperty("symbol").GetString(),
                    Price = Decimal.Parse(symbol.GetProperty("price_usd").GetString()),
                };
                cryptoData[data.Symbol] = data;
            }

            return cryptoData;
        }

        public List<object> CalculatePortfolioValue(List<PortfolioEntry> entries, Dictionary<string, CryptoData> cryptoData, List<SymbolData> symbolDataList)
        {
            var portfolioDetails = new List<object>();

            foreach (var symbol in entries.Select(e => e.Symbol).Distinct())
            {
                var symbolEntries = entries.Where(e => e.Symbol == symbol).ToList();
                var symbolData = symbolDataList.FirstOrDefault(s => s.Name == symbol);
                if (symbolData == null)
                {
                    _logger.LogWarning($"Symbol {symbol} not found in SymbolData.");
                    continue;
                }

                if (!cryptoData.TryGetValue(symbolData.Name, out var data))
                {
                    _logger.LogWarning($"Data for symbol {symbol} not found in crypto data.");
                    continue;
                }

                var initialBuyPrice = symbolEntries.First().InitialBuyPrice;
                var totalQuantity = symbolEntries.Sum(e => e.Quantity);
                var currentValue = totalQuantity * data.Price;
                var initialValue = totalQuantity * initialBuyPrice;

                var change = (initialValue == 0) ? 0 : ((currentValue - initialValue) / initialValue) * 100;

                portfolioDetails.Add(new
                {
                    Symbol = symbol,
                    InitialBuyPrice = initialBuyPrice,
                    Quantity = totalQuantity,
                    CurrentPrice = data.Price,
                    CurrentValue = currentValue,
                    Change = change
                });
            }

            return portfolioDetails;
        }

        public void SetUpdateInterval(TimeSpan interval, string sessionId)
        {
            _userIntervals[sessionId] = interval;

            var test = _taskQueue.IsTaskRunning(sessionId);
            // If the interval task for this session already exists, we don't need to start another one.
            if (!_taskQueue.IsTaskRunning(sessionId))
            {
                _taskQueue.QueueBackgroundWorkItem(async token => await PeriodicUpdate(token, sessionId), sessionId);
            }
        }

        private async Task PeriodicUpdate(CancellationToken token, string sessionId)
        {
            while (!token.IsCancellationRequested)
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var _context = scope.ServiceProvider.GetRequiredService<PortfolioDataContext>();
                    var httpClient = CreateHttpClient();

                    var entries = await _context.PortfolioEntries
                    .Where(e => e.SessionId == sessionId)
                    .ToListAsync();

                    var symbols = entries.Select(e => e.Symbol).Distinct().ToList();
                    var symbolDataList = await _context.SymbolData
                        .Where(s => symbols.Contains(s.Name))
                        .ToListAsync();

                    var ids = symbolDataList.Select(s => s.Id).ToList();
                    var cryptoData = await GetCryptoDataAsync(ids);

                    var updatedPortfolio = CalculatePortfolioValue(entries, cryptoData, symbolDataList);
                    await _hubContext.Clients.Group(sessionId).SendAsync("ReceivePortfolioUpdate", updatedPortfolio);

                    _logger.LogInformation("Portfolio updated successfully for session {SessionId}.");

                    try
                    {
                        var interval = _userIntervals.ContainsKey(sessionId) ? _userIntervals[sessionId] : TimeSpan.FromMinutes(5);
                        await Task.Delay(interval, token);
                    }
                    catch (TaskCanceledException)
                    {
                        break;
                    }
                }
            }
        }
    }
}

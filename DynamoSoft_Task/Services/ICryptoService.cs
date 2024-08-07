using DynamoSoft_Task.Models;

namespace DynamoSoft_Task.Services
{
    public interface ICryptoService
    {
        Task<Dictionary<string, CryptoData>> GetCryptoDataAsync(List<int> symbol_ids);
        List<object> CalculatePortfolioValue(List<PortfolioEntry> entries, Dictionary<string, CryptoData> cryptoData, List<SymbolData> symbolDataList);
        Task SeedCoinsAsync();
        void SetUpdateInterval(TimeSpan interval, string sessionId);
    }
}

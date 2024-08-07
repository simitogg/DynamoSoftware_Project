namespace DynamoSoft_Task.Models
{
    public class SymbolData
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class PortfolioEntry
    {
        public int Id {  get; set; }
        public decimal Quantity { get; set; }
        public string Symbol { get; set; }
        public decimal InitialBuyPrice { get; set; }
        public string SessionId { get; set; }
    }

    public class CryptoData
    {
        public string Symbol { get; set; }
        public decimal Price { get; set; }
    }

}

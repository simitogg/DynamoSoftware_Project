using Microsoft.AspNetCore.SignalR;

namespace DynamoSoft_Task.Services
{
    public class PortfolioHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var sessionId = Context.GetHttpContext().Request.Cookies["SessionId"].ToString();
            await Groups.AddToGroupAsync(Context.ConnectionId, sessionId);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var sessionId = Context.GetHttpContext().Request.Cookies["SessionId"].ToString();
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, sessionId);
            await base.OnDisconnectedAsync(exception);
        }
        public async Task SendUpdate(string sessionId, object updatedPortfolio)
        {
            await Clients.Group(sessionId).SendAsync("ReceivePortfolioUpdate", updatedPortfolio);
        }
    }

}

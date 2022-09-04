using Microsoft.AspNetCore.SignalR;

namespace SignalR.API.Hubs
{
    public class MyHub : Hub
    {
        public static List<string> Names { get; set; } = new List<string>();
        public async Task SendName(string message)
        {
            Names.Add(message);
            await Clients.All.SendAsync("ReceiveName", message);
        }
        public async Task GetNames()
        {
            await Clients.All.SendAsync("ReceiveNames", Names);
        }
    }
}

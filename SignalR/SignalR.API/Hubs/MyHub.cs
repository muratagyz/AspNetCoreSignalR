using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SignalR.API.Models;

namespace SignalR.API.Hubs
{
    public class MyHub : Hub
    {
        private readonly AppDbContext _context;
        public MyHub(AppDbContext context)
        {
            _context = context;
        }
        private static List<string> Names { get; set; } = new List<string>();
        private static int ClientCount { get; set; } = 0;
        public static int TeamCount { get; set; }


        public async Task SendName(string message)
        {
            if (Names.Count > TeamCount)
            {
                await Clients.Caller.SendAsync("Error", $"Takım en fazla {TeamCount} kişi olabilir");
            }
            else
            {
                Names.Add(message);
                await Clients.All.SendAsync("ReceiveName", message);
            }
        }
        public async Task GetNames()
        {
            await Clients.All.SendAsync("ReceiveNames", Names);
        }

        public async override Task OnConnectedAsync()
        {
            ClientCount++;
            await Clients.All.SendAsync("ReceiveClientCount", ClientCount);
            await base.OnConnectedAsync();
        }

        public async override Task OnDisconnectedAsync(Exception? exception)
        {
            ClientCount--;
            await Clients.All.SendAsync("ReceiveClientCount", ClientCount);
            await base.OnDisconnectedAsync(exception);
        }

        //Groups
        public async Task AddToGroup(string teamName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, teamName);
        }

        public async Task RemoveToGroup(string teamName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, teamName);
        }

        public async Task SendNameByGroup(string name, string teamName)
        {
            var team = await _context.Teams.Where(x => x.Name == teamName).FirstOrDefaultAsync();

            if (team != null)
            {
                team.Users.Add(new User()
                {
                    Name = name
                });
            }

            else
            {
                var newTeam = new Team()
                {
                    Name = teamName
                };

                newTeam.Users.Add(new User()
                {
                    Name = name
                });

                await _context.Teams.AddAsync(newTeam);
            }

            await _context.SaveChangesAsync();

            await Clients.Group(teamName).SendAsync("ReceiveMessageByGroup", name, team);
        }

        public async Task GetNamesByGroup()
        {
            var teams = _context.Teams.Include(x => x.Users).Select(x =>
                new
                {
                    teamName = x.Name,
                    Users = x.Users.ToList()
                }
            );

            await Clients.All.SendAsync("ReceiveNamesByGroup", teams);
        }
    }
}

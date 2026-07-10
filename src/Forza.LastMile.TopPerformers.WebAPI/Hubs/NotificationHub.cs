using Microsoft.AspNetCore.SignalR;

namespace Forza.LastMile.TopPerformers.WebAPI.Hubs;

public class NotificationHub : Hub
{
	public override async Task OnConnectedAsync()
	{
		await base.OnConnectedAsync();
	}

	public async Task SendEventToClients(object eventData)
	{
		await Clients.All.SendAsync("ReceiveEvent", eventData);
	}
}

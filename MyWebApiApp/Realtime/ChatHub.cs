using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace MyWebApiApp.Realtime;

public class ChatHub : Hub
{
    public async Task SendMessage(string userId, string message)
    
    {
        try
        {
            // Xử lý logic để gửi tin nhắn real-time
            await Clients.All.SendAsync("ReceiveMessage", userId, message);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}

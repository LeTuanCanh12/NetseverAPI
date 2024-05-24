using MyWebApiApp.Data;
using System;

namespace MyWebApiApp.Models
{
    public class Chat
    {
        public Guid SenderId { get; set; }
        public Guid ReceiverName { get; set; }
        public Guid ChatId { get; set; }

        //relationship
        public NguoiDung Sender { get; set; }
        public NguoiDung Receiver { get; set; }
        public ChatInfo ChatInfo { get; set; }
    } 
}

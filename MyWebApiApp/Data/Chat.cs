using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyWebApiApp.Data
{
    [Table("Chat")]
    public class Chat
    {
        public int SenderId { get; set; }
        public int ReceiverId { get; set; }
        public Guid ChatId { get; set; }

        public virtual NguoiDung Sender { get; set; }
        public virtual NguoiDung Receiver { get; set; }
        public virtual ChatInfo ChatInfo { get; set; }
    }
}

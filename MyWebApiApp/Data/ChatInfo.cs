using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyWebApiApp.Data
{

    [Table("ChatInfo")]
    public class ChatInfo
    {
        [Key]
        public Guid ChatId { get; set; }
        public DateTime SendDate { get; set; }
        public string Message { get; set; }
        public byte MessageType { get; set; }
        public ICollection<Chat> chats { get; set; }
        public ChatInfo()
        {
            chats = new List<Chat>();
        }
    }
}

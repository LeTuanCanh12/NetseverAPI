using System;

namespace MyWebApiApp.Models
{
    
    public class ChatInfoVM
    {
        public Guid ChatId { get; set; }
        public DateTime DateSend { get; set; }
        public string Message { get; set; }
        public byte MessageType { get; set; }
    }

    public class ChatInfoModel
    {
        public DateTime DateSend { get; set; }
        public string Message { get; set; }
        public byte MessageType { get; set; }
    }
}

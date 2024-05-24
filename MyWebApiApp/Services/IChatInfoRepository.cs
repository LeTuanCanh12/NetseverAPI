using MyWebApiApp.Models;
using System;
using System.Collections.Generic;

namespace MyWebApiApp.Services
{
    public class FullChatModel
    {
        public int SenderId { get; set; }
        public int ReceiverId { get; set; }
        public Guid ChatId { get; set; }
        public DateTime DateSend { get; set; }
        public string Message { get; set; }
        public byte MessageType { get; set; }

    }
    public interface IChatInfoRepository
    {
        List<FullChatModel> GetAll(int SenderId, int ReceiverId, int page);
        ChatInfoVM GetById(string id);
        ChatInfoVM Add(int SenderId, int ReceiverId, string message, byte messageType);
        void Update(Guid id, DateTime time, string Message, byte MessageType);
        void Delete(string id);
    }
}
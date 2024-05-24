using Microsoft.EntityFrameworkCore;
using MyWebApiApp.Data;
using MyWebApiApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyWebApiApp.Services
{
    public class ChatInfoRepository : IChatInfoRepository
    {
        private readonly MyDbContext _context;
        public static int PAGE_SIZE { get; set; } = 10;
        public ChatInfoRepository(MyDbContext context) 
        {
            _context = context;
        }
        ChatInfoVM IChatInfoRepository.Add(int SenderId, int ReceiverId, string message, byte messageType)
        {
            var chatInfo = new Data.ChatInfo 
            {
                ChatId = Guid.NewGuid(),
                SendDate = DateTime.Now,
                Message = message,
                MessageType = messageType
            };

            var chat = new Data.Chat
            {
                SenderId = SenderId,
                ReceiverId = ReceiverId,
                ChatId = chatInfo.ChatId,
            };


            _context.Add(chatInfo);
            _context.Add(chat);
            _context.SaveChanges();
            return new ChatInfoVM
            {
                ChatId = chatInfo.ChatId,
                DateSend = chatInfo.SendDate,
                Message = chatInfo.Message,
                MessageType =chatInfo.MessageType
            };

        }

        void IChatInfoRepository.Delete(string id)
        {
            var chat = _context.ChatInfos.SingleOrDefault(lo => lo.ChatId == Guid.Parse(id));
            if (chat != null)
            {
                _context.Remove(chat);
                _context.SaveChanges(true);
            }
        }

        public List<FullChatModel> GetAll(int senderId, int receiverId, int page = 1)
        {
            var allChats = _context.Chats.AsQueryable()
                .Join(_context.ChatInfos,
                    chat => chat.ChatId,
                    chatInfo => chatInfo.ChatId,
                    (chat, chatInfo) => new { Chat = chat, ChatInfo = chatInfo });

            #region Filtering
            allChats = allChats.Where(join => join.Chat.SenderId == senderId && join.Chat.ReceiverId == receiverId || join.Chat.SenderId == receiverId && join.Chat.ReceiverId == senderId);
            #endregion

            #region Sorting
            allChats = allChats.OrderByDescending(join => join.ChatInfo.SendDate);
            #endregion

            #region Paging
            var totalCount = allChats.Count();
            var result = allChats.Skip((page - 1) * PAGE_SIZE)
                         .Take(PAGE_SIZE)
                         .Select(join => new FullChatModel
                         {
                             SenderId = join.Chat.SenderId,
                             ReceiverId = join.Chat.ReceiverId,
                             ChatId = join.ChatInfo.ChatId,
                             DateSend = join.ChatInfo.SendDate,
                             Message = join.ChatInfo.Message,
                             MessageType = join.ChatInfo.MessageType
                         })
                         .ToList();
            #endregion

            return result;
        }

        ChatInfoVM IChatInfoRepository.GetById(string id)
        {
            var chat = _context.ChatInfos.SingleOrDefault(chat =>chat.ChatId == Guid.Parse(id));
            if (chat == null) return null;
            return new ChatInfoVM
            {
                ChatId = chat.ChatId,
                DateSend=chat.SendDate,
                Message=chat.Message,
                MessageType=chat.MessageType
            };
        }

        void IChatInfoRepository.Update(Guid id, DateTime time, string Message, byte MessageType)
        {
            var chat = _context.ChatInfos.SingleOrDefault(lo => lo.ChatId== id);
            if (chat == null) return;
            chat.SendDate = time;
            chat.Message = Message;
            chat.MessageType = MessageType;
            _context.SaveChanges();
        }

        
    }
}

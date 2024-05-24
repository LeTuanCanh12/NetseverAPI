using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyWebApiApp.Models;
using MyWebApiApp.Services;
using System;

namespace MyWebApiApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatInfoController : ControllerBase
    {
        private readonly IChatInfoRepository _chatInfoRepository;

        public ChatInfoController(IChatInfoRepository chatInfoRepository) 
        {
            _chatInfoRepository = chatInfoRepository;
        }
        [HttpGet]
        public IActionResult GetAll(int SenderId, int ReceiverId, int page) 
        {
            try 
            { 
                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "OK get data",
                    Data = _chatInfoRepository.GetAll(SenderId, ReceiverId, page)
                }); 
            }
            catch
            {
                return Ok(new ApiResponse
                {
                    Success = false,
                    Message = "Fail to get data"
                });
            }
        }

        //[HttpGet]
        /*public IActionResult GetChat(int userRoot, int userClient, int page)
        {
            try
            {
                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Sucess getting chats",
                    Data = _chatInfoRepository.GetChat(userRoot, userClient, page)
                });
            }
            catch 
            {
                return Ok(new ApiResponse
                {
                    Success = false,
                    Message = "Fail to get chats"
                });
            }
        }*/

        [HttpGet("{id}")]
        public IActionResult GetById(string id) 
        {
            try
            {
                var data = _chatInfoRepository.GetById(id);
                if (data == null)
                {
                    return Ok(new ApiResponse
                    {
                        Success = false,
                        Message = "Chat not found"
                    });
                }
                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Success get chat",
                    Data = data
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return Ok(new ApiResponse
                {
                    Success = false,
                    Message = "Fail to get data"
                });
            }
        }
        [HttpPost]
        public IActionResult CreateNew(int SenderId, int ReceiverId, string message, byte messageType)
        {
            try
            {
                var chat = _chatInfoRepository.Add(SenderId, ReceiverId, message, messageType);
                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Success create chat",
                    Data = chat
                });
            }
            catch
            {
                return Ok(new ApiResponse
                {
                    Success = false,
                    Message = "Fail to create chat"
                });
            }
        }
        [HttpDelete()]
        public IActionResult DeleteById(string id) 
        {

            try { 
                _chatInfoRepository.Delete(id);
                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Delete success"
                });
            }
            catch
            {
                return Ok(new ApiResponse
                {
                    Success = false,
                    Message = "Fail to delete"
                });
            }

        }
        [HttpPut("{id}")]
        public IActionResult Update(string id, Guid idUpdate, DateTime time, string Message, byte MessageType)
        {
            try 
            {
                if (Guid.Parse(id) != idUpdate)
                {
                    return Ok(new ApiResponse
                    {
                        Success = false,
                        Message = "Chat not found"
                    });
                }
                _chatInfoRepository.Update(idUpdate, time, Message, MessageType);
                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Success update",
                });
            }
            catch
            {
                return Ok(new ApiResponse
                {
                    Success = false,
                    Message = "Fail to update"
                });
            }
        }
    }
}

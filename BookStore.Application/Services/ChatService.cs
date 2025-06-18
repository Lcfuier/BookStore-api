using BookStore.Application.InterfacesRepository;
using BookStore.Domain.Constants;
using BookStore.Domain.DTOs;
using BookStore.Domain.Models;
using BookStore.Domain.Queries;
using BookStore.Domain.Result;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.Application.Services
{
    public class ChatService
    {
        private readonly IUnitOfWork _data;
        private readonly UserManager<ApplicationUser> _userManager;
        public ChatService(IUnitOfWork data, UserManager<ApplicationUser> userManager)
        {
            _data = data;
            _userManager = userManager;
        }
        public async Task<Result<Chat>> GetChatByUser(string userName)
        {
            var user =await _userManager.FindByNameAsync(userName);
            if (user == null)
            {
                return new Result<Chat>
                {
                    Success = false,
                    Data = null,
                    Message = "User not exist"
                };
            }
            var chat =await _data.Chat.GetAsync(new QueryOptions<Chat>
            {
                Where = c => c.UserId.Equals(user.Id),
            });
            if(chat == null)
            {
                chat = new Chat()
                {
                    CreatedTime = DateTime.UtcNow,
                    User= user,
                };
                _data.Chat.Add(chat);   
                await _data.SaveAsync();
            }
            return new Result<Chat>
            {
                Success = true,
                Data = chat
            };
        
        }
        
        public async Task<Result<PaginationResponse<Chat>>> GetAllChat(int page, int size,string userName)
        {
            var user = await _userManager.FindByNameAsync(userName);
            if (user == null)
            {
                return new Result<PaginationResponse<Chat>>
                {
                    Success = false,
                    Data = null,
                    Message = "Người dùng không tồn tại !"
                };
            }
            var roles = await _userManager.GetRolesAsync(user);
            if (roles.FirstOrDefault()?.ToLower() != Roles.Admin.ToLower() || roles.FirstOrDefault()?.ToLower() != Roles.Librarian.ToLower())
            {
                return new Result<PaginationResponse<Chat>>
                {
                    Success = false,
                    Data = null,
                    Message = "Không thể truy cập !"
                };
            }
            IEnumerable<Chat> data;
            QueryOptions<Chat> options = new QueryOptions<Chat>
            {
                OrderBy=c=>c.LastMessageAt,
                OrderByDirection="desc"
            };
            if (page < 1)
            {
                data = await _data.Chat.ListAllAsync(options);
            }
            else
            {
                options.PageNumber = page;
                options.PageSize = size;
                data = await _data.Chat.ListAllAsync(options);
            }

            PaginationResponse<Chat> paginationResponse = new PaginationResponse<Chat>

            {
                PageNumber = page,
                PageSize = size,
                // must be above the TotalRecords bc it has multiple Where clauses
                Items = data,
                TotalRecords = await _data.Chat.CountAsync()
            };
            return new Result<PaginationResponse<Chat>>
            {
                Data = paginationResponse,
                Message = "Successful",
                Success = true
            };
        }
        public async Task<Result<Message>> SendMessage(string userName, string content,Guid chatId)
        {
            var user = await _userManager.FindByNameAsync(userName);
            if (user == null)
            {
                return new Result<Message>
                {
                    Success = false,
                    Data = null,
                    Message = "User not exist"
                };
            }
            var chat = await _data.Chat.GetAsync(new QueryOptions<Chat>
            {
                Where = c => c.ChatId.Equals(chatId),
            });
            if (chat == null)
            {
                chat = new Chat()
                {
                    CreatedTime = DateTime.UtcNow,
                    User = user,
                };
                _data.Chat.Add(chat);
            }
            var message = new Message()
            {
                Chat = chat,
                Content = content,
                User = user,
                SendAt = DateTime.UtcNow,

            };
            chat.LastMessageAt = DateTime.UtcNow;
            chat.LastSenderId = user.Id;
            _data.Chat.Update(chat);
            _data.Message.Add(message);
            await _data.SaveAsync();
            return new Result<Message>
            {
                Success = true,
                Data = null
            };
        }
    }
}

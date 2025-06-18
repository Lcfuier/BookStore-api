using AutoMapper;
using BookStore.Application.Interface;
using BookStore.Domain.Constants;
using BookStore.Domain.DTOs;
using BookStore.Domain.Models;
using BookStore.Domain.Queries;
using BookStore.Domain.Result;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3.Data;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using Microsoft.Extensions.Configuration;
using File = Google.Apis.Drive.v3.Data.File;
using System.Management;
using Google.Apis.Upload;
using Google;
using Microsoft.AspNetCore.Hosting;
using BookStore.Application.InterfacesRepository;

namespace BookStore.Application.Services
{
    public class BookService : IBookService
    {
        private readonly IUnitOfWork _data;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IImageService _imageService;
        public BookService(IUnitOfWork data, UserManager<ApplicationUser> userManager, IMapper mapper, IImageService imageService)//IOptions<GoogleDriveSettings> driveSettings)
        {
            _data = data;
            _mapper = mapper;
            _userManager = userManager;
            _imageService=imageService;
        }
        public async Task<Result<PaginationResponse<Book>>> GetAllBooksAsync(GetAllBookReq req)
        {
            IEnumerable<Book> data;
            QueryOptions<Book> options = new QueryOptions<Book>
            {
                Includes = "Author, Categories, Publisher"
            };
            if (req.term != null)
            {
                options.Where = mi => mi.Title.ToLower().Contains(req.term.ToLower());
            }
            if (req.AuthorId.HasValue)
            {
                options.Where = c => c.authorId.Equals(req.AuthorId);
            }
            if (req.CategoryId.HasValue)
            {
                options.Where = c => c.Categories.Any(c => c.CategoryId.Equals(req.CategoryId));
            }
            if (req.PublisherId.HasValue)
            {
                options.Where = c => c.publisherID.Equals(req.PublisherId);
            }
            if (!string.IsNullOrEmpty(req.OrderBy))
            {
                if (req.OrderBy.Equals("createdTime"))
                {
                    options.OrderBy = c => c.CreatedTime;
                }
                else if (req.OrderBy.Equals("title"))
                {
                    options.OrderBy = c => c.Title;
                }
                if(req.IsAsc is false)
                {
                    options.OrderByDirection = "desc";
                }
            }
            if (req.Page < 1)
            {
                data = await _data.Book.ListAllAsync(options);
            }
            
            else
            {
                options.PageNumber = req.Page;
                options.PageSize = req.Size;
                data = await _data.Book.ListAllAsync(options);
            }

            PaginationResponse<Book> paginationResponse = new PaginationResponse<Book>
            {
                PageNumber = req.Page,
                PageSize = req.Size,
                // must be above the TotalRecords bc it has multiple Where clauses
                Items = data,
                TotalRecords = await _data.Book.CountAsync()
            };
            return new Result<PaginationResponse<Book>>
            {
                Data = paginationResponse,
                Success = true
            };
        }
        public async Task<Result<Book>> GetBookByIdAsync(Guid id)
        {
            QueryOptions<Book> options = new()
            {
                Where = a => a.BookId == id,
                Includes = "Author, Categories, Publisher"
            };
            var data = await _data.Book.GetAsync(options);
            //var data=await _data.Book.GetBookByIdAsync(id);
            if (data != null)
            {
                return new Result<Book>
                {
                    Success = true,
                    Data = data,
                };
            }
            else
            {
                return new Result<Book>
                {
                    Success = false,
                    Data = data,
                    Message = "Không tìm thấy sách!"
                };
            }

        }
        public async Task<Result<IEnumerable<BestSellerBookRes>>> GetBestSellerBookAsync()
        {
            var data = await _data.Book.GetBestSellerBook();
            //var data=await _data.Book.GetBookByIdAsync(id);
            if (data != null)
            {
                return new Result<IEnumerable<BestSellerBookRes>>
                {
                    Success = true,
                    Data = data,
                };
            }
            else
            {
                return new Result<IEnumerable<BestSellerBookRes>>
                {
                    Success = false,
                    Data = data,
                };
            }

        }
        public async Task<Result<Book>> RemoveBookAsync(Guid Id,string userName)
        {
            Result<Book> result = new Result<Book>();
            var user = await _userManager.FindByNameAsync(userName);
            if (user == null)
            {
                return new Result<Book>
                {
                    Success = false,
                    Data = null,
                    Message = "Người dùng không tồn tại !"
                };
            }
            var roles = await _userManager.GetRolesAsync(user);
            if (roles.FirstOrDefault()?.ToLower() != Roles.Admin.ToLower() && roles.FirstOrDefault()?.ToLower() != Roles.Librarian.ToLower())
            {
                return new Result<Book>
                {
                    Success = false,
                    Data = null,
                    Message = "Không thể truy cập!"
                };
            }
            var book = await _data.Book.GetAsync(new QueryOptions<Book>
            {
                Where = c => c.BookId.Equals(Id)
            });
            if (book == null)
            {
                result.Success = false;
                result.Message = "Sách không tồn tại";
                result.Data = null;
                return result;
            }
            if (!string.IsNullOrEmpty(book.ImageURL))
            {
                await _imageService.DeleteImageAsync(book.ImageURL);
            }
            _data.Book.Remove(book);
            await _data.SaveAsync();
            result.Success = true;
            result.Message = "Xóa sách thành công";
            return result;
        }
        public async Task<Result<Book>> AddBookAsync(AddBookReq bookDto, IFormFile image, string userName)
        {
            Result<Book> result = new Result<Book>();
            var user = await _userManager.FindByNameAsync(userName);
            if (user == null)
            {
                return new Result<Book>
                {
                    Success = false,
                    Data = null,
                    Message = "Người dùng không tồn tại !"
                };
            }
            var roles = await _userManager.GetRolesAsync(user);
            if (roles.FirstOrDefault()?.ToLower() != Roles.Admin.ToLower() && roles.FirstOrDefault()?.ToLower() != Roles.Librarian.ToLower())
            {
                return new Result<Book>
                {
                    Success = false,
                    Data = null,
                    Message = "Không thể truy cập!"
                };
            }
            var book = new Book();
            book = _mapper.Map<Book>(bookDto);
            var url = await _imageService.SaveImageAsync(image);
            book.ImageURL = url;
            await _data.Book.AddNewCategoryAsync(book, bookDto.CategoryIds, _data.Category);
            book.CreatedBy = userName;
            book.CreatedTime = DateTime.UtcNow;
            book.PublicationDate=bookDto.PublicationDate.ToUniversalTime();
            _data.Book.Add(book);
            await _data.SaveAsync();
            result.Success = true;
            result.Message = "Thêm sách thành công";
            result.Data = book;
            return result;
        }
        public async Task<Result<Book>> UpdateBookAsync(UpdateBookReq bookDto, IFormFile? newImage, string userName,Guid Id)
        {
            Result<Book> result = new Result<Book>();
            var user = await _userManager.FindByNameAsync(userName);
            if (user == null)
            {
                return new Result<Book>
                {
                    Success = false,
                    Data = null,
                    Message = "Người dùng không tồn tại !"
                };
            }
            var roles = await _userManager.GetRolesAsync(user);
            if (roles.FirstOrDefault()?.ToLower() != Roles.Admin.ToLower() && roles.FirstOrDefault()?.ToLower() != Roles.Librarian.ToLower())
            {
                return new Result<Book>
                {
                    Success = false,
                    Data = null,
                    Message = "Không thể truy cập!"
                };
            }
            QueryOptions<Book> options = new()
            {
                Where = a => a.BookId == Id,
                Includes = "Author, Categories, Publisher"
            };
            var book = await _data.Book.GetAsync(options);
            if (book is null)
            {
                result.Success = false;
                result.Message = "Book not exist";
                result.Data = null;
                return result;
            }
            string url = book.ImageURL;
            book.Title=bookDto.Title;
            book.PublicationDate=bookDto.PublicationDate.ToUniversalTime(); 
            book.Description=bookDto.Description;
            book.Isbn13 =bookDto.Isbn13;
            book.DiscountPercent=bookDto.DiscountPercent;
            book.Price=bookDto.Price;
            book.Inventory=bookDto.Inventory;
            book.NumberOfPage=bookDto.NumberOfPage;
            book.authorId=bookDto.authorId;
            book.publisherID=bookDto.publisherID;
            if (newImage != null && newImage.Length > 0)
            {
                if (!string.IsNullOrEmpty(url))
                {
                    await _imageService.DeleteImageAsync(url);
                }
                book.ImageURL = await _imageService.SaveImageAsync(newImage);
            }
            else
            {
                book.ImageURL = url;
            }
            await _data.Book.AddNewCategoryAsync(book, bookDto.CategoryIds, _data.Category);
            book.ModifiedBy = userName;
            book.ModifiedTime = DateTime.UtcNow;
            _data.Book.Update(book);
            await _data.SaveAsync();
            result.Success = true;
            result.Message = "Cập nhật sách thành công!";
            result.Data = book;
            return result;
        }
    }

}

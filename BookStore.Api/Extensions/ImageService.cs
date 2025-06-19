using BookStore.Application.Interface;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace BookStore.Api.Extensions
{
    public class ImageService : IImageService
    {
        private readonly IWebHostEnvironment _env;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly Cloudinary _cloudinary;

        public ImageService(IWebHostEnvironment env, IHttpContextAccessor httpContextAccessor, IConfiguration config)
        {
            _env = env;
            _httpContextAccessor = httpContextAccessor;
            var account = new Account(
           config["CloudinarySettings:CloudName"],
           config["CloudinarySettings:ApiKey"],
           config["CloudinarySettings:ApiSecret"]
            );

            _cloudinary = new Cloudinary(account);
        }

        /*public async Task<string> SaveImageAsync(IFormFile file)
        {
            if (_env.WebRootPath == null)
                throw new Exception("WebRootPath is null!");

            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(uploadsFolder, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            var request = _httpContextAccessor.HttpContext.Request;
            return $"{request.Scheme}://{request.Host}/uploads/{fileName}";
        }

        public async Task DeleteImageAsync(string imageUrl)
        {
            var fileName = Path.GetFileName(new Uri(imageUrl).LocalPath);
            var filePath = Path.Combine(_env.WebRootPath, "uploads", fileName);

            if (File.Exists(filePath))
                File.Delete(filePath);

            await Task.CompletedTask;
        }*/
        public async Task<string> SaveImageAsync(IFormFile file)
        {
            if (file == null || file.Length == 0) return null;

            await using var stream = file.OpenReadStream();
            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = "bookstore" // tạo folder bookstore trong Cloudinary
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);
            return uploadResult.SecureUrl?.ToString();
        }

        public async Task DeleteImageAsync(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl)) return;

            var uri = new Uri(imageUrl);
            var publicId = $"bookstore/{Path.GetFileNameWithoutExtension(uri.LocalPath)}";

            var deletionParams = new DeletionParams(publicId);
            await _cloudinary.DestroyAsync(deletionParams);
        }
    }
}

using BookStore.Application.Interface;

namespace BookStore.Api.Extensions
{
    public class ImageService : IImageService
    {
        private readonly IWebHostEnvironment _env;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ImageService(IWebHostEnvironment env, IHttpContextAccessor httpContextAccessor)
        {
            _env = env;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<string> SaveImageAsync(IFormFile file)
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
        }
    }
}

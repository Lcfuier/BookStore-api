using Microsoft.AspNetCore.Mvc;

namespace BookStore.Api.Controllers
{
    public class ChatController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

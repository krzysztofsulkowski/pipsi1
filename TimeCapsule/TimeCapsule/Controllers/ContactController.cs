using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TimeCapsule.Controllers
{
    [Authorize]
    public class ContactController : TimeCapsuleBaseController
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

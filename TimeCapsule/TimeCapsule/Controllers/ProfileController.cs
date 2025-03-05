using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TimeCapsule.Controllers
{
    [Authorize]
    public class ProfileController : TimeCapsuleBaseController
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

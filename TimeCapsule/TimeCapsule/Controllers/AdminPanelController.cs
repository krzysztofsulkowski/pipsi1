using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TimeCapsule.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminPanelController : TimeCapsuleBaseController
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

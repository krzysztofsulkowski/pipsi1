using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TimeCapsule.Controllers.Admin
{
    public class CapsuleManagementController : TimeCapsuleBaseController
    {
        [Authorize(Roles = "Admin")]
        [Route("AdminPanel/Capsules")]

        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = true)]
        public IActionResult GetCapsules()
        {
            return View();
        }
    }
}

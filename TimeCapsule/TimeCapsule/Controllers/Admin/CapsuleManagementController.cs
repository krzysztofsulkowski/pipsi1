using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimeCapsule.Controllers;
using TimeCapsule.Models.DatabaseModels;
using TimeCapsule.Models.Dto;
using TimeCapsule.Models;
using Microsoft.EntityFrameworkCore;


[Authorize(Roles = "Admin")]
[Route("AdminPanel/Capsules")]
public class CapsuleManagementController : TimeCapsuleBaseController
{
    private readonly TimeCapsuleContext _context;

    public CapsuleManagementController(TimeCapsuleContext context)
    {
        _context = context;
    }

    [HttpGet("")]
    public IActionResult GetCapsules()
    {
        var listaKapsul = _context.Capsules
            .Include(c => c.CreatedByUser)
            .Where(c => c.Status != Status.Deleted)
            .Select(c => new AdminCapsuleDto
            {
                CapsuleId = c.Id,
                Title = c.Title,
                UserName = c.CreatedByUser.Email,
                IsOpened = c.Status == Status.Opened,
                OpenDate = c.OpeningDate
            })
            .ToList();

        return View("~/Views/CapsuleManagement/CapsulesManagementView.cshtml", listaKapsul);
    }

    [HttpGet("Edit/{id}")]
    public IActionResult EditCapsuleDate(int id)
    {
        var capsule = _context.Capsules.FirstOrDefault(c => c.Id == id);

        if (capsule == null)
            return NotFound();

        var model = new AdminCapsuleDto
        {
            CapsuleId = capsule.Id,
            OpenDate = capsule.OpeningDate
        };

        return View("~/Views/CapsuleManagement/EditCapsuleDate.cshtml", model);
    }

    [HttpPost("Edit")]
    public IActionResult EditCapsuleDate(AdminCapsuleDto model)
    {
        var capsule = _context.Capsules.FirstOrDefault(c => c.Id == model.CapsuleId);

        if (capsule == null)
            return NotFound();

        capsule.OpeningDate = model.OpenDate;
        _context.SaveChanges();

        return RedirectToAction(nameof(GetCapsules));
    }

    [HttpPost("Delete/{id}")]
    public IActionResult DeleteCapsule(int id)
    {
        var capsule = _context.Capsules.FirstOrDefault(c => c.Id == id);

        if (capsule == null)
        {
            return NotFound();
        }

        capsule.Status = Status.Deleted;
        _context.SaveChanges();

        return RedirectToAction(nameof(GetCapsules));
    }
}

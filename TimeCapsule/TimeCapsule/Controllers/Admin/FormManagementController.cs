using Microsoft.AspNetCore.Mvc;
using TimeCapsule.Models.Dto;
using TimeCapsule.Services.Results;
using Microsoft.AspNetCore.Authorization;
using TimeCapsule.Interfaces;

namespace TimeCapsule.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    [Route("AdminPanel/Forms")]
    public class FormManagementController : TimeCapsuleBaseController
    {
        private readonly IFormManagementService _formManagementService;

        public FormManagementController(IFormManagementService formManagementService)
        {
            _formManagementService = formManagementService;
        }

        [HttpGet]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> GetForms()
        {
            var sectionsResult = await _formManagementService.GetFormSectionsWithQuestions();

            if (!sectionsResult.IsSuccess)
            {
                return View("~/Views/AdminPanel/Forms/FormsManagementView.cshtml", new List<CapsuleSectionDto>());
            }

            return View("~/Views/AdminPanel/Forms/FormsManagementView.cshtml", sectionsResult.Data);
        }

        
        [HttpPost("AddSection")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> AddSection([FromForm] CreateSectionDto model)
        {
            var result = await _formManagementService.AddSection(model);

            return HandleFormResult(result, model, "Sekcja została stworzona pomyślnie.");
        }

        [HttpGet("GetSectionById/{sectionId}")]
        public async Task<IActionResult> GetSectionById(int sectionId)
        {
            if (sectionId <= 0)
            {
                return BadRequest(ServiceResult.Failure("Nieprawidłowy parametr: ID sekcji musi być większe od zera"));
            }
            var result = await _formManagementService.GetSectionById(sectionId);
            return HandleStatusCodeServiceResult(result);
        }

        [HttpPost("UpdateSection")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> UpdateSection([FromForm] UpdateSectionDto model)
        {
            var result = await _formManagementService.UpdateSection(model);
            return HandleFormResult(result, model, "Pytanie zostało zaktualizowane pomyślnie.");
        }


        [HttpPost("AddQuestion")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> AddQuestion([FromForm] CreateQuestionDto model)
        {
            var result = await _formManagementService.AddQuestion(model);
            return HandleFormResult(result, model, "Pytanie zostało stworzone pomyślnie.");
        }

        [HttpGet("GetQuestionById/{questionId}")]
        public async Task<IActionResult> GetQuestionById(int questionId)
        {
            if (questionId <= 0)
            {
                return BadRequest(ServiceResult.Failure("Nieprawidłowy parametr: ID pytania musi być większe od zera"));
            }

            var result = await _formManagementService.GetQuestionById(questionId);
            return HandleStatusCodeServiceResult(result);
        }

        [HttpPost("UpdateQuestion")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> UpdateQuestion([FromForm] UpdateQuestionDto model)
        {
            var result = await _formManagementService.UpdateQuestion(model);
            return HandleFormResult(result, model, "Pytanie zostało zaktualizowane pomyślnie.");
        }

        [HttpPost("DeleteQuestion/{questionId}")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> DeleteQuestion(int questionId)
        {
            if (questionId <= 0)
            {
                var tempModel = new { Id = questionId };
                return HandleFormResult(ServiceResult.Failure("Nieprawidłowy identyfikator pytania."),
                                        tempModel, "", "Nieprawidłowy identyfikator: ");
            }

            var result = await _formManagementService.DeleteQuestion(questionId);

            return HandleFormResult(result, new { Id = questionId }, "Pytanie zostało usunięte pomyślnie.");
        }

        [HttpPost("DeleteSection/{sectionId}")]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<IActionResult> DeleteSection(int sectionId)
        {
            if (sectionId <= 0)
            {
                var tempModel = new { Id = sectionId };
                return HandleFormResult(ServiceResult.Failure("Nieprawidłowy identyfikator sekcji."),
                                        tempModel, "", "Nieprawidłowy identyfikator: ");
            }

            var result = await _formManagementService.DeleteSection(sectionId);
            return HandleFormResult(result, new { Id = sectionId }, "Sekcja została usunięta pomyślnie.");
        }
    }
}

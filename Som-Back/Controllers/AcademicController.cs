using Microsoft.AspNetCore.Mvc;
using Som_Service.Interface;

namespace Som_Back.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AcademicController : ControllerBase
    {
        private readonly IAcademicService _academicService;

        public AcademicController(IAcademicService academicService)
        {
            _academicService = academicService;
        }

        [HttpGet("curriculums")]
        public async Task<IActionResult> GetCurriculums()
        {
            var list = await _academicService.GetCurriculumsAsync();
            return Ok(list);
        }

        [HttpGet("classes")]
        public async Task<IActionResult> GetClasses([FromQuery] string curriculumType = "General")
        {
            var list = await _academicService.GetClassesAsync(curriculumType);
            return Ok(list);
        }

        [HttpGet("subjects")]
        public async Task<IActionResult> GetSubjects([FromQuery] string curriculumType, [FromQuery] string className)
        {
            if (string.IsNullOrWhiteSpace(curriculumType) || string.IsNullOrWhiteSpace(className))
            {
                return BadRequest(new { message = "CurriculumType and ClassName are required." });
            }

            var list = await _academicService.GetSubjectsAsync(curriculumType, className);
            return Ok(list);
        }
    }
}

using LearningPortal.DataBaseTables;
using LearningPortal.Services;
using Microsoft.AspNetCore.Mvc;

namespace LearningPortal.Controllers
{
    [ApiController]
    [Route("api/check-solved-variants")]
    public class VariantCountController : ControllerBase
    {
        private readonly VariantCountService _variantCountService;

        public VariantCountController(VariantCountService variantCountService)
        {
            _variantCountService = variantCountService;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            User? user = HttpContext.Items["user"] as User;
            if (user == null)
            {
                return Unauthorized();
            }

            int variantsCount = await _variantCountService.CheckSolvedVariants(user);

            return Ok(variantsCount);
        }
    }
}

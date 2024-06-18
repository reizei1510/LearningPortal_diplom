using LearningPortal.DataBaseTables;
using LearningPortal.Models;
using LearningPortal.Services;
using Microsoft.AspNetCore.Mvc;

namespace LearningPortal.Controllers
{
    [ApiController]
    [Route("api/get-variant")]
    public class VariantGetController : ControllerBase
    {
        private readonly GetVariantService _getVariantService;

        public VariantGetController(GetVariantService getVariantService)
        {
            _getVariantService = getVariantService;
        }

        [HttpGet("{variantNum}")]
        public async Task<IActionResult> Get(int variantNum)
        {
            User? user = HttpContext.Items["user"] as User;
            if (user == null)
            {
                return Unauthorized();
            }

            VariantModel? variant = await _getVariantService.GetVariant(user, variantNum);

            return Ok(variant);
        }
    }
}

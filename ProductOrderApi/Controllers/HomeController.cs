using Microsoft.AspNetCore.Mvc;

namespace ProductOrderApi.Controllers
{
    public class HomeController : Controller
    {
        [HttpGet]
        public IActionResult Get()
        {
            var token = Request.Headers["Authorization"].ToString();

            return Ok(token);
        }
    }
}

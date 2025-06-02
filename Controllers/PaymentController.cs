using Microsoft.AspNetCore.Mvc;

namespace SmartCookFinal.Controllers
{
    public class PaymentController : Controller 
    {
        public IActionResult Premium()
        {
            return View(); 
        }
    }
}

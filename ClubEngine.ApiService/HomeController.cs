using Microsoft.AspNetCore.Mvc;

namespace ClubEngine.ApiService;

public class HomeController : Controller
{
    // GET
    public IActionResult Index()
    {
        return Json(new
        {
            Test = "Test"
        });
    }
}
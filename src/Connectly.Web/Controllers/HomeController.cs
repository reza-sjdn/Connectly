using Connectly.Infrastructure.Persistence;
using Connectly.Web.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Connectly.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;
        private readonly MyMethods myMethods;

        public HomeController(AppDbContext context)
        {
            _context = context;
            myMethods = new MyMethods(_context);
        }

        public IActionResult Index() => View();

        public JsonResult Ef()
        {
            //myMethods.Update_Basic();
            //myMethods.Update_UpdateMethod();
            //myMethods.Update_AttachPattern();
            //myMethods.TestOriginalCurrentValues();
            //myMethods.Update_ExecuteUpdateAsync_1();
            //myMethods.Update_ExecuteUpdateAsync_2();
            //myMethods.Update_ExecuteUpdateAsync_3();
            myMethods.Update_RelatedEntity();

            return Json("Ok");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


    }
}

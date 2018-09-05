using System;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace IGSLControlPanel.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Error(Exception e)
        {
            var feature = HttpContext?.Features.Get<IExceptionHandlerPathFeature>();
            var logger = log4net.LogManager.GetLogger(typeof(ProductsController));
            logger.Error($"Message: {feature?.Error.Message ?? "NO HTTPCONTEXT"}; Trace: {feature?.Error.StackTrace ?? ""}");
            return View();
        }
    }
}
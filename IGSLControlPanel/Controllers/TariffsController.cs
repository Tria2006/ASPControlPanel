using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IGSLControlPanel.Data;
using Microsoft.AspNetCore.Mvc;

namespace IGSLControlPanel.Controllers
{
    public class TariffsController : Controller
    {
        private readonly IGSLContext _context;

        public TariffsController(IGSLContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IGSLControlPanel.Data;
using IGSLControlPanel.Enums;
using IGSLControlPanel.Helpers;
using IGSLControlPanel.Models;
using Microsoft.Extensions.Logging;

namespace IGSLControlPanel.Controllers
{
    public class TariffsController : FoldersBaseController
    {
        private readonly IGSLContext _context;
        private readonly TariffsHelper _tariffsHelper;
        private readonly ILogger _logger;

        public TariffsController(IGSLContext context, FolderDataHelper helper, TariffsHelper tariffsHelper, ILogger<ProductsController> logger)
            : base(context, helper)
        {
            _context = context;
            _logger = logger;
            BuildFolderTree(ModelTypes.Tariffs);
            _tariffsHelper = tariffsHelper;
            _tariffsHelper.Initialize(_context, GetRootFolder());
        }

        public IActionResult Index(Guid id)
        {
            return View(GetFolderById(id));
        }

        public IActionResult Create(Guid folderId)
        {
            var tempTariff = new Tariff { FolderId = folderId };
            _tariffsHelper.CurrentTariff = tempTariff;
            _tariffsHelper.IsCreateInProgress = true;
            ViewData["ParentFolderId"] = folderId;
            return View(tempTariff);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Tariff tariff)
        {
            if (!ModelState.IsValid) return View(tariff);
            tariff.Id = Guid.NewGuid();
            _context.Add(tariff);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tariff = await _context.Tariffs.FindAsync(id);
            if (tariff == null)
            {
                return NotFound();
            }
            return View(tariff);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, Tariff tariff)
        {
            if (id != tariff.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid) return View(tariff);
            try
            {
                _context.Update(tariff);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TariffExists(tariff.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tariff = await _context.Tariffs
                .FirstOrDefaultAsync(m => m.Id == id);
            if (tariff == null)
            {
                return NotFound();
            }

            return View(tariff);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var tariff = await _context.Tariffs.FindAsync(id);
            _context.Tariffs.Remove(tariff);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TariffExists(Guid id)
        {
            return _context.Tariffs.Any(e => e.Id == id);
        }

        public bool ProductCheckBoxClick(Guid id)
        {
            _tariffsHelper.CheckTariff(id, _context);
            return _tariffsHelper.HasSelectedTariffs;
        }

        public IActionResult MoveSelectedItems()
        {
            MoveSelectedFolders();
            _tariffsHelper.MoveSelectedTariffs(_context, GetSelectedDestFolderId());
            BuildFolderTree(ModelTypes.Products);
            return RedirectToAction("Index", new { id = GetSelectedDestFolderId() });
        }
    }
}

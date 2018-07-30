using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IGSLControlPanel.Data;
using IGSLControlPanel.Enums;
using IGSLControlPanel.Helpers;
using IGSLControlPanel.Models;

namespace IGSLControlPanel.Controllers
{
    public class TariffsController : FoldersBaseController
    {
        private readonly IGSLContext _context;
        private readonly TariffsHelper _tariffsHelper;

        public TariffsController(IGSLContext context, FolderDataHelper helper, TariffsHelper tariffsHelper)
            : base(context, helper)
        {
            _context = context;
            BuildFolderTree(ModelTypes.Tariffs);
            _tariffsHelper = tariffsHelper;
        }

        public IActionResult Index(Guid id)
        {
            _tariffsHelper.Initialize(_context, GetFolderById(id));
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
            _context.Add(tariff);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index), GetFolderById(tariff.FolderId));
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
            return RedirectToAction(nameof(Index), GetFolderById(tariff.FolderId));
        }

        public async Task<IActionResult> Delete(Guid id)
        {
            if (_tariffsHelper.HasSelectedTariffs)
            {
                await _tariffsHelper.RemoveTariffs(_context, GetFolderById(id));
            }
            return RedirectToAction("Index", new { id });
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var tariff = await _context.Tariffs.FindAsync(id);
            tariff.IsDeleted = true;
            await _context.SaveChangesAsync();
            var parentFolder = GetFolderById(tariff.FolderId);
            _tariffsHelper.BuildTariffs(parentFolder);
            return RedirectToAction(nameof(Index), parentFolder);
        }

        private bool TariffExists(Guid id)
        {
            return _context.Tariffs.Any(e => e.Id == id);
        }

        public bool TariffCheckBoxClick(Guid id)
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

        public override async Task ClearFolderItems(List<FolderTreeEntry> foldersToClear)
        {
            foreach (var folder in foldersToClear)
            {
                var tariffs = _context.Tariffs.Where(x => x.FolderId == folder.Id);
                if(!tariffs.Any()) continue;
                await tariffs.ForEachAsync(x => x.FolderId = Guid.Empty);
            }
            await _context.SaveChangesAsync();
        }
    }
}

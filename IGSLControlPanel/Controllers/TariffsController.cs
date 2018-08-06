﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DBModels.Models;
using DBModels.Models.ManyToManyLinks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IGSLControlPanel.Data;
using IGSLControlPanel.Enums;
using IGSLControlPanel.Helpers;

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

        public IActionResult Index(Guid parentid)
        {
            _tariffsHelper.Initialize(_context, GetFolderById(parentid));
            return View(GetFolderById(parentid));
        }

        public IActionResult Create(Guid folderId)
        {
            var tempTariff = new Tariff { FolderId = folderId };
            if (_tariffsHelper.IsTariffCreateInProgress)
            {
                tempTariff = _tariffsHelper.CurrentTariff;
            }
            else
            {
                _tariffsHelper.CurrentTariff = tempTariff;
                _tariffsHelper.IsTariffCreateInProgress = true;
            }
            ViewData["ParentFolderId"] = folderId;
            ViewData["InsRulesList"] = _context.InsuranceRules.Except(tempTariff.InsRuleTariffLink.Select(x => x.InsRule));
            return View(tempTariff);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Tariff tariff)
        {
            if (!ModelState.IsValid) return View(tariff);
            _context.Add(tariff);
            await _context.SaveChangesAsync();
            if (_tariffsHelper.IsInsRuleCreateInProgress)
            {
                tariff.InsRuleTariffLink = _tariffsHelper.CurrentTariff.InsRuleTariffLink;
                tariff.InsRuleTariffLink.ForEach(p =>
                {
                    p.TariffId = tariff.Id;
                });
                _tariffsHelper.IsInsRuleCreateInProgress = false;
                await _context.SaveChangesAsync();
            }
            if (_tariffsHelper.CurrentTariff.InsRuleTariffLink.Count > 0)
            {
                foreach (var link in _tariffsHelper.CurrentTariff.InsRuleTariffLink)
                {
                    tariff.InsRuleTariffLink.Add(new InsRuleTariffLink
                    {
                        InsRuleId = link.InsRuleId,
                        TariffId = tariff.Id
                    });
                    await _context.SaveChangesAsync();
                }
            }
            _tariffsHelper.IsTariffCreateInProgress = false;
            return RedirectToAction(nameof(Index), GetFolderById(tariff.FolderId));
        }

        public IActionResult Edit(Guid id)
        {
            var tariff = _context.Tariffs.Include(x => x.InsRuleTariffLink).ThenInclude(x => x.InsRule).SingleOrDefault(x => x.Id == id);
            if (tariff == null)
            {
                return NotFound();
            }
            ViewData["InsRulesList"] = _context.InsuranceRules.Except(tariff.InsRuleTariffLink.Select(x => x.InsRule));
            _tariffsHelper.CurrentTariff = tariff;
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

        // Нужно сохранить значения полей продукта если он еще не был сохранен, иначе при возвращении обратно 
        // на экран создания нового продукта все данные очистятся
        public void SaveTempData(Guid folderId, string name, DateTime? dateFrom, DateTime? dateTo)
        {
            _tariffsHelper.CurrentTariff.Name = name;
            _tariffsHelper.CurrentTariff.FolderId = folderId;
            _tariffsHelper.CurrentTariff.ValidFrom = dateFrom;
            _tariffsHelper.CurrentTariff.ValidTo = dateTo;
        }
    }
}

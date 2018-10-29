using DBModels.Models;
using IGSLControlPanel.Data;
using IGSLControlPanel.Enums;
using IGSLControlPanel.Helpers;
using log4net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IGSLControlPanel.Controllers
{
    public class TariffsController : FoldersBaseController
    {
        private readonly IGSLContext _context;
        private readonly TariffsHelper _tariffsHelper;
        private readonly IHttpContextAccessor _httpAccessor;
        private readonly ILog _logger;
        private readonly FilesHelper _filesHelper;

        public TariffsController(IGSLContext context, FolderDataHelper helper, TariffsHelper tariffsHelper, IHttpContextAccessor accessor, FilesHelper filesHelper)
            : base(context, helper)
        {
            _context = context;
            _httpAccessor = accessor;
            _logger = LogManager.GetLogger(typeof(TariffsController));
            BuildFolderTree(ModelTypes.Tariffs);
            _tariffsHelper = tariffsHelper;
            _filesHelper = filesHelper;
        }

        public IActionResult Index(Guid parentid)
        {
            _tariffsHelper.Initialize(_context, GetFolderById(parentid));
            return View(GetFolderById(parentid));
        }

        public IActionResult Create(Guid folderId)
        {
            var tempTariff = new Tariff { FolderId = folderId, ValidFrom = DateTime.Today, ValidTo = new DateTime(2100, 1, 1) };
            _tariffsHelper.CurrentTariff = tempTariff;
            ViewData["ParentFolderId"] = folderId;
            return View(tempTariff);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Tariff tariff, string create, string createAndExit)
        {
            if (!ModelState.IsValid) return View(tariff);
            _context.Add(tariff);
            await _context.SaveChangesAsync();
            _logger.Info($"{_httpAccessor.HttpContext.Connection.RemoteIpAddress} created Tariff (id={tariff.Id})");
            if (!string.IsNullOrEmpty(createAndExit))
                return RedirectToAction(nameof(Index), GetFolderById(tariff.FolderId));
            return RedirectToAction("Edit", new { tariff.Id });
        }

        public IActionResult Edit(Guid id)
        {
            var tariff = _context.Tariffs
                .Include(x => x.RiskFactorsTariffLinks)
                .ThenInclude(x => x.RiskFactor)
                .Include(x => x.InsRuleTariffLink)
                .ThenInclude(x => x.InsRule)
                .ThenInclude(x => x.LinksToRisks)
                .ThenInclude(x => x.Risk)
                .SingleOrDefault(x => x.Id == id);
            if (tariff == null)
            {
                return NotFound();
            }
            _tariffsHelper.CurrentTariff = tariff;
            return View(tariff);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, Tariff tariff, string save, string saveAndExit)
        {
            if (id != tariff.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid) return View(_tariffsHelper.CurrentTariff);
            try
            {
                _context.Update(tariff);
                await _context.SaveChangesAsync();
                _logger.Info($"{_httpAccessor.HttpContext.Connection.RemoteIpAddress} updated Tariff (id={tariff.Id})");
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
            if (!string.IsNullOrEmpty(saveAndExit))
                return RedirectToAction(nameof(Index), GetFolderById(tariff.FolderId));
            return RedirectToAction("Edit", new { id });
        }

        public async Task<IActionResult> Delete(Guid id)
        {
            if (_tariffsHelper.HasSelectedTariffs)
            {
                await _tariffsHelper.RemoveTariffs(_context, GetFolderById(id), _logger, _httpAccessor);
            }
            return RedirectToAction("Index", new { id });
        }

        public IActionResult Details(Guid tariffId)
        {
            var tariff = _context.Tariffs
                .Include(x => x.RiskFactorsTariffLinks)
                .ThenInclude(x => x.RiskFactor)
                .Include(x => x.InsRuleTariffLink)
                .ThenInclude(x => x.InsRule)
                .ThenInclude(x => x.LinksToRisks)
                .ThenInclude(x => x.Risk)
                .SingleOrDefault(x => x.Id == tariffId);
            if (tariff == null)
            {
                return NotFound();
            }
            return View(tariff);
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
                if (!tariffs.Any()) continue;
                await tariffs.ForEachAsync(x => x.FolderId = Guid.Empty);
            }
            await _context.SaveChangesAsync();
        }

        public bool GetFolderOrTariffSelected()
        {
            return _tariffsHelper.HasSelectedTariffs || HasSelectedFolders;
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

        public IActionResult CreateExcelFile()
        {
            var path = _filesHelper.CreateExcel(_tariffsHelper.CurrentTariff, _context);
            return File(System.IO.File.ReadAllBytes(path), "application/octet-stream", "tempFile.xlsx");
        }

        public IActionResult GoToUploadPage()
        {
            return View("UploadFileView");
        }

        public IActionResult ReturnFromUploadForm()
        {
            return RedirectToAction("Edit", new { _tariffsHelper.CurrentTariff.Id });
        }

        [HttpPost]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            await _filesHelper.UploadFile(file, _tariffsHelper.CurrentTariff, _context);
            return RedirectToAction("Edit", new { _tariffsHelper.CurrentTariff.Id });
        }

        public async Task SelectTariff(Guid id)
        {
            await _tariffsHelper.SelectTariffForProduct(id, _context);
            ViewData["TariffId"] = id;
        }

        public IActionResult AttachTariffToProduct(Guid productId)
        {
            var product = _context.Products.Find(productId);
            if (product == null) return NotFound();

            var attachedTariff = _context.Tariffs.Include(x => x.LinkedProducts).SingleOrDefault(x => x.Id == product.TariffId);

            if (attachedTariff != null)
            {
                attachedTariff.LinkedProducts.RemoveAll(x => x.Id != productId);
                _tariffsHelper.SelectedTariffForProduct.LinkedProducts.Add(product);
                product.TariffId = _tariffsHelper.SelectedTariffForProduct.Id;
            }
            else
            {
                _tariffsHelper.SelectedTariffForProduct.LinkedProducts.Add(product);
                product.TariffId = _tariffsHelper.SelectedTariffForProduct.Id;
            }
            _context.SaveChanges();
            return RedirectToAction("Edit", "Products", product);
        }

        public async Task<IActionResult> DetachTariff(Guid productId)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null) return NotFound();

            var attachedTariff = await _context.Tariffs.Include(x => x.LinkedProducts).SingleOrDefaultAsync(x => x.Id == product.TariffId);

            attachedTariff.LinkedProducts.RemoveAll(x => x.Id == productId);
            product.TariffId = null;
            await _context.SaveChangesAsync();

            return RedirectToAction("Edit", "Products", product);
        }
    }
}

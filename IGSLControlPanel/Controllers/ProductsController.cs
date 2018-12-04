using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DBModels.Models;
using IGSLControlPanel.Data;
using IGSLControlPanel.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IGSLControlPanel.Helpers;
using log4net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace IGSLControlPanel.Controllers
{
    public class ProductsController : FoldersBaseController
    {
        private readonly IGSLContext _context;
        private readonly ProductsHelper _productsHelper;
        private readonly IHttpContextAccessor _httpAccessor;
        private readonly ProductParamRiskFactorLinkHelper _linkHelper;
        private readonly ILog _logger;

        private IQueryable<ParameterGroup> GroupList { get; }

        public ProductsController(IGSLContext context, FolderDataHelper helper, ProductsHelper productsHelper, IHttpContextAccessor accessor,
            ProductParamRiskFactorLinkHelper linkHelper) 
            : base(context, helper)
        {
            _context = context;
            _httpAccessor = accessor;
            _logger = LogManager.GetLogger(typeof(ProductsController));
            BuildFolderTree(ModelTypes.Products);
            _productsHelper = productsHelper;
            _linkHelper = linkHelper;
            GroupList = _context.ParameterGroups.Where(x => !x.IsDeleted);
        }

        public IActionResult Index(Guid id)
        {
            _productsHelper.Initialize(_context, GetFolderById(id));
            return View(GetFolderById(id));
        }

        public IActionResult CreateProduct(Guid folderId)
        {
            var tempProduct = new Product {FolderId = folderId, ValidFrom = DateTime.Today, ValidTo = new DateTime(2100, 1, 1)};
                _productsHelper.CurrentProduct = tempProduct;
            ViewData["ParamGroups"] = new SelectList(GroupList, "Id", "Name");
            ViewData["ParentFolderId"] = folderId;
            return View(tempProduct);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProduct(Product product, string create, string createAndExit)
        {
            await _productsHelper.AddProduct(product, _context);
            _logger.Info($"{_httpAccessor.HttpContext.Connection.RemoteIpAddress} created Product (id={product.Id})");
            if (!string.IsNullOrEmpty(createAndExit))
                return RedirectToAction("Index", new { id = product.FolderId });
            return RedirectToAction("Edit", new { product.Id });
        }

        public async Task<IActionResult> Edit(Guid id)
        {
            ViewData["ParamGroups"] = new SelectList(GroupList, "Id", "Name");
            var product = await _context.Products.Include(x => x.LinkToProductParameters).ThenInclude(s => s.Parameter).SingleOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }
            var attachedGroups = product.LinkToProductParameters.Select(x => x.Parameter.GroupId).Distinct().ToList();
            ViewData["GlobalGroups"] = _context.ParameterGroups.Where(x => x.IsGlobal && !x.IsDeleted && !attachedGroups.Contains(x.Id)).ToList();
            ViewData["Tariffs"] = _context.Tariffs.Where(x => !x.IsDeleted);
            ViewData["TariffId"] = product.TariffId;
            product.Tariff = await _context.Tariffs.FindAsync(product.TariffId);
            _productsHelper.LoadProductLimits(product, _context);
            _productsHelper.CurrentProduct = product;
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Product product, string save, string saveAndExit)
        {
            if (string.IsNullOrEmpty(save) && string.IsNullOrEmpty(saveAndExit))
            {
                var id = product.Id;
                product = await _context.Products.Include(x => x.LinkToProductParameters).ThenInclude(s => s.Parameter).SingleOrDefaultAsync(m => m.Id == id);
                if (product == null)
                {
                    return NotFound();
                }
            }
            await _productsHelper.UpdateProduct(product, GetFolderById(product.FolderId), _context);
            _logger.Info($"{_httpAccessor.HttpContext.Connection.RemoteIpAddress} updated Product (id={product.Id})");
            if (!string.IsNullOrEmpty(saveAndExit))
                return RedirectToAction("Index", new { id = product.FolderId });
            return RedirectToAction("Edit", new { product.Id });
        }

        public async Task<IActionResult> DeleteProduct(Guid folderId)
        {
            if (_productsHelper.HasSelectedProducts)
            {
                await _productsHelper.RemoveProducts(_context, GetFolderById(folderId), _logger, _httpAccessor);
            }
            return RedirectToAction("Index", new { folderId });
        }

        public bool ProductCheckBoxClick(Guid id)
        {
            _productsHelper.CheckProduct(id, _context);
            return _productsHelper.HasSelectedProducts;
        }

        public bool GetFolderOrProductSelected()
        {
            return _productsHelper.HasSelectedProducts || HasSelectedFolders;
        }

        public IActionResult MoveSelectedItems()
        {
            MoveSelectedFolders();
            _productsHelper.MoveSelectedProducts(_context, GetSelectedDestFolderId());
            BuildFolderTree(ModelTypes.Products);
            return RedirectToAction("Index", new { id = GetSelectedDestFolderId() });
        }

        public void ProductParameterClick(Guid id)
        {
            _productsHelper.SelectUnselectParameter(id, _context);
        }

        public override async Task ClearFolderItems(List<FolderTreeEntry> foldersToClear)
        {
            foreach (var folder in foldersToClear)
            {
                var products = _context.Products.Where(x => x.FolderId == folder.Id);
                if (!products.Any()) continue;
                await products.ForEachAsync(x => x.FolderId = Guid.Empty);
            }
            await _context.SaveChangesAsync();
        }

        // Нужно сохранить значения полей продукта если он еще не был сохранен, иначе при возвращении обратно 
        // на экран создания нового продукта все данные очистятся
        public void SaveTempData(Guid folderId, string name, DateTime? dateFrom, DateTime? dateTo)
        {
            _productsHelper.CurrentProduct.Name = name;
            _productsHelper.CurrentProduct.FolderId = folderId;
            _productsHelper.CurrentProduct.ValidFrom = dateFrom;
            _productsHelper.CurrentProduct.ValidTo = dateTo;
        }

        public async Task<IActionResult> LinkSettings(Guid productId)
        {
            var product = await _context.Products
                .Include(x => x.LinkToProductParameters)
                .ThenInclude(x => x.Parameter).SingleOrDefaultAsync(x => x.Id == productId);
            if (product == null) return NotFound();
            product.Tariff = await _context.Tariffs
                .Include(x => x.RiskFactorsTariffLinks)
                .ThenInclude(x => x.RiskFactor).SingleOrDefaultAsync(x => x.Id == product.TariffId);

            return View(product);
        }

        public void AddLink(Guid productId, Guid tariffId, Guid paramId, Guid factorId)
        {
            _linkHelper.AddParamToFactorLink(productId, tariffId, paramId, factorId);
        }

        [HttpPost]
        public async Task<IActionResult> SaveLinks(Guid productId, string save, string saveAndExit)
        {
            await _linkHelper.SaveLinks(_context);

            if (!string.IsNullOrEmpty(saveAndExit))
                return RedirectToAction("Edit", new { id = productId });
            return RedirectToAction("LinkSettings", new { productId });
        }

        public IActionResult GetProductById(Guid id)
        {
            ViewData["ParamGroups"] = new SelectList(GroupList, "Id", "Name");
            ViewData["IsSelectParamView"] = true;
            return PartialView("_ParameterGroupsBlock", 
                _context.Products
                    .Include(x => x.LinkToProductParameters)
                    .ThenInclude(x => x.Parameter).SingleOrDefault(x => x.Id == id));
        }
    }
}

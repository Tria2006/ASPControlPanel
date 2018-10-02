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
        private readonly ILog logger;

        public ProductsController(IGSLContext context, FolderDataHelper helper, ProductsHelper productsHelper, IHttpContextAccessor accessor) 
            : base(context, helper)
        {
            _context = context;
            _httpAccessor = accessor;
            logger = LogManager.GetLogger(typeof(ProductsController));
            BuildFolderTree(ModelTypes.Products);
            _productsHelper = productsHelper;
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
            var groups = _context.ParameterGroups.Where(x => !x.IsDeleted);
            ViewData["ParamGroups"] = new SelectList(groups, "Id", "Name");
            ViewData["ParentFolderId"] = folderId;
            return View(tempProduct);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProduct(Product product, string create, string createAndExit)
        {
            await _productsHelper.AddProduct(product, _context);
            logger.Info($"{_httpAccessor.HttpContext.Connection.RemoteIpAddress} created Product (id={product.Id})");
            if (!string.IsNullOrEmpty(createAndExit))
                return RedirectToAction("Index", new { id = product.FolderId });
            return RedirectToAction("Edit", new { product.Id });
        }

        public async Task<IActionResult> Edit(Guid id)
        {
            var groups = _context.ParameterGroups.Where(x => !x.IsDeleted);
            ViewData["ParamGroups"] = new SelectList(groups, "Id", "Name");
            var product = await _context.Products.Include(x => x.LinkToProductParameters).ThenInclude(s => s.Parameter).SingleOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }
            _productsHelper.LoadProductLimits(product, _context);
            _productsHelper.CurrentProduct = product;
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Product product, string save, string saveAndExit)
        {
            await _productsHelper.UpdateProduct(product, GetFolderById(product.FolderId), _context);
            logger.Info($"{_httpAccessor.HttpContext.Connection.RemoteIpAddress} updated Product (id={product.Id})");
            if (!string.IsNullOrEmpty(saveAndExit))
                return RedirectToAction("Index", new { id = product.FolderId });
            return RedirectToAction("Edit", new { product.Id });
        }

        public async Task<IActionResult> DeleteProduct(Guid folderId)
        {
            if (_productsHelper.HasSelectedProducts)
            {
                await _productsHelper.RemoveProducts(_context, GetFolderById(folderId), logger, _httpAccessor);
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
    }
}

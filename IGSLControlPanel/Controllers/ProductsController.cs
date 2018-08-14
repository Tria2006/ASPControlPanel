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
        private readonly EntityStateHelper _stateHelper;
        private readonly IHttpContextAccessor _httpAccessor;
        private readonly ILog logger;

        public ProductsController(IGSLContext context, FolderDataHelper helper, ProductsHelper productsHelper, EntityStateHelper stateHelper, IHttpContextAccessor accessor) 
            : base(context, helper)
        {
            _context = context;
            _httpAccessor = accessor;
            logger = LogManager.GetLogger(typeof(ProductsController));
            BuildFolderTree(ModelTypes.Products);
            _productsHelper = productsHelper;
            _stateHelper = stateHelper;
        }

        public IActionResult Index(Guid id)
        {
            _productsHelper.Initialize(_context, GetFolderById(id));
            return View(GetFolderById(id));
        }

        public IActionResult CreateProduct(Guid folderId)
        {
            var tempProduct = new Product {FolderId = folderId};
            // такое может быть если возвращаемся с экрана создания нового параметра
            if (_stateHelper.IsProductCreateInProgress)
            {
                // возвращаемся к заполнению нового продукта
                tempProduct = _productsHelper.CurrentProduct;
            }
            else
            {
                _productsHelper.CurrentProduct = tempProduct;
                _stateHelper.IsProductCreateInProgress = true;
            }
            var groups = _context.ParameterGroups.Where(x => !x.IsDeleted);
            ViewData["ParamGroups"] = new SelectList(groups, "Id", "Name");
            ViewData["ParentFolderId"] = folderId;
            return View(tempProduct);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProduct(Product product)
        {
            await _productsHelper.AddProduct(product, _context);
            _stateHelper.IsProductCreateInProgress = false;
            // такое может быть если создавали новый продукт и сразу добавляли туда параметры
            if (_stateHelper.IsParameterCreateInProgress)
            {
                product.LinkToProductParameters = _productsHelper.CurrentProduct.LinkToProductParameters;
                // нужно проставить ProductId в линки созданных параметров
                product.LinkToProductParameters.ForEach(p =>
                {
                    p.ProductId = product.Id;
                });
                _stateHelper.IsParameterCreateInProgress = false;
                await _productsHelper.UpdateProduct(product, GetFolderById(product.FolderId), _context);
            }
            logger.Info($"{_httpAccessor.HttpContext.Connection.RemoteIpAddress} created Product (id={product.Id})");
            return RedirectToAction("Index", new { id = product.FolderId });
        }

        public async Task<IActionResult> Edit(Guid id)
        {
            var groups = _context.ParameterGroups.Where(x => !x.IsDeleted);
            ViewData["ParamGroups"] = new SelectList(groups, "Id", "Name");
            var product = await _context.Products.Include(x => x.LinkToProductParameters).ThenInclude(s => s.Parameter).SingleOrDefaultAsync(m => m.Id == id);
            _productsHelper.CurrentProduct = product;
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Product product)
        {
            await _productsHelper.UpdateProduct(product, GetFolderById(product.FolderId), _context);
            logger.Info($"{_httpAccessor.HttpContext.Connection.RemoteIpAddress} updated Product (id={product.Id})");
            return RedirectToAction("Index", new { id = product.FolderId });
        }

        public async Task<IActionResult> DeleteProduct(Guid id)
        {
            if (_productsHelper.HasSelectedProducts)
            {
                await _productsHelper.RemoveProducts(_context, GetFolderById(id), logger, _httpAccessor);
            }
            return RedirectToAction("Index", new { id });
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
            _productsHelper.SelectUnselectParameter(id);
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

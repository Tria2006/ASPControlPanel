using System;
using System.Linq;
using System.Threading.Tasks;
using IGSLControlPanel.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IGSLControlPanel.Helpers;
using IGSLControlPanel.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;

namespace IGSLControlPanel.Controllers
{
    public class ProductsController : FoldersBaseController
    {
        private readonly IGSLContext _context;
        private readonly ProductsHelper _productsHelper;
        private readonly ILogger _logger;

        public ProductsController(IGSLContext context, FolderDataHelper helper, ProductsHelper productsHelper, ILogger<ProductsController> logger) 
            : base(context, helper)
        {
            _context = context;
            _logger = logger;
            _productsHelper = productsHelper;
        }

        public IActionResult Index(Guid id)
        {
            return View(GetFolderById(id));
        }

        public IActionResult CreateProduct(Guid folderId)
        {
            var tempProduct = new Product {FolderId = folderId};
            _productsHelper.CurrentProduct = tempProduct;
            _productsHelper.IsProductCreateInProgress = true;
            var groups = _context.ParameterGroups.Where(x => !x.IsDeleted);
            ViewData["ParamGroups"] = new SelectList(groups, "Id", "Name");
            ViewData["ParentFolderId"] = folderId;
            return View(tempProduct);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProduct(Product product)
        {
            var helper = new ProductsHelper();
            await helper.AddProduct(product, _context);
            _productsHelper.IsProductCreateInProgress = false;
            return RedirectToAction("Index", new { id = product.FolderId });
        }

        public async Task<IActionResult> Edit(Guid id)
        {
            var groups = _context.ParameterGroups.Where(x => !x.IsDeleted);
            ViewData["ParamGroups"] = new SelectList(groups, "Id", "Name");
            var product = await _context.Products.SingleOrDefaultAsync(m => m.Id == id);
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
            await _productsHelper.UpdateProduct(product, GetFolderById(product.FolderId ?? Guid.Empty), _context);
            return RedirectToAction("Index", new { id = product.FolderId });
        }

        public async Task<IActionResult> DeleteProduct(Guid id)
        {
            if (_productsHelper.HasSelectedProducts)
            {
                await _productsHelper.RemoveProducts(_context, GetFolderById(id));
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
            RebuildFolderTree();
            return RedirectToAction("Index", new { id = GetSelectedDestFolderId() });
        }

        public void ProductParameterClick(Guid id)
        {
            _productsHelper.SelectUnselectParameter(id);
        }
    }
}

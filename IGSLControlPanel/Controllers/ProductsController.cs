﻿using System;
using System.Threading.Tasks;
using IGSLControlPanel.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IGSLControlPanel.Helpers;
using IGSLControlPanel.Models;
using Microsoft.Extensions.Logging;

namespace IGSLControlPanel.Controllers
{
    public class ProductsController : Controller
    {
        private readonly IGSLContext _context;
        private readonly FolderDataHelper _folderDataHelper;
        private readonly ProductsHelper _productsHelper;
        private readonly ILogger _logger;

        public ProductsController(IGSLContext context, FolderDataHelper helper, ProductsHelper productsHelper, ILogger<ProductsController> logger)
        {
            _context = context;
            _logger = logger;
            _folderDataHelper = helper;
            _productsHelper = productsHelper;
            _folderDataHelper.Initialize(_context);
        }

        public IActionResult Index(Guid id)
        {
            var folder = _folderDataHelper.GetFolderById(id, _folderDataHelper.FoldersTree);
            return View(folder);
        }

        public IActionResult OneLevelUp(Guid destFolderId, bool returnPartial = false)
        {
            if (!returnPartial) return RedirectToAction("Index", new {id = destFolderId});
            var folder = _folderDataHelper.GetFolderById(destFolderId, _folderDataHelper.FoldersTree);
            _folderDataHelper.SelectedDestFolderId = destFolderId;
            return PartialView("FolderSelectView", folder);
        }

        public IActionResult CreateProduct(Guid folderId)
        {
            var tempProduct = new Product {FolderId = folderId};
            _productsHelper.CurrentProduct = tempProduct;
            _productsHelper.IsProductCreateInProgress = true;
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

        public async Task<IActionResult> CreateFolder(string name, Guid parentFolderId)
        {
            var folder = await _context.FolderTreeEntries.SingleOrDefaultAsync(m => m.Id == parentFolderId);
            await _folderDataHelper.AddFolder(name, _context, folder);
            return RedirectToAction("Index", new{ id = parentFolderId });
        }

        public async Task<IActionResult> Edit(Guid id)
        {
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
            await _folderDataHelper.UpdateProduct(product, _context);
            return RedirectToAction("Index", new { id = product.FolderId });
        }

        public async Task<IActionResult> DeleteFolder(Guid id)
        {
            if(_folderDataHelper.HasSelectedFolders)
                await _folderDataHelper.RemoveFolders(_context, id);
            return RedirectToAction("Index", new { id });
        }

        public async Task<IActionResult> DeleteProduct(Guid id)
        {
            if (_folderDataHelper.HasSelectedProducts)
                await _folderDataHelper.RemoveProducts(_context, id);
            return RedirectToAction("Index", new { id });
        }

        public bool FolderCheckBoxClick(Guid id)
        {
            _folderDataHelper.CheckFolder(id, _context);
            return _folderDataHelper.HasSelectedFolders;
        }

        public bool ProductCheckBoxClick(Guid id)
        {
            _folderDataHelper.CheckProduct(id, _context);
            return _folderDataHelper.HasSelectedProducts;
        }

        public bool GetFolderOrProductSelected()
        {
            return _folderDataHelper.HasSelectedProducts || _folderDataHelper.HasSelectedFolders;
        }

        public IActionResult FolderClick(Guid id)
        {
            var folder = _folderDataHelper.GetFolderById(id, _folderDataHelper.FoldersTree);
            _folderDataHelper.SelectedDestFolderId = id;
            return PartialView("FolderSelectView", folder);
        }

        public IActionResult MoveSelectedItems()
        {
            _folderDataHelper.MoveSelectedItems(_context);
            return RedirectToAction("Index", new { id = _folderDataHelper.SelectedDestFolderId });
        }

        public void ProductParameterClick(Guid id)
        {
            _productsHelper.SelectUnselectParameter(id);
        }
    }
}

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
        private readonly ILogger _logger;

        public ProductsController(IGSLContext context, FolderDataHelper helper, ILogger<ProductsController> logger)
        {
            _context = context;
            _logger = logger;
            _folderDataHelper = helper;
            _folderDataHelper.Initialize(_context);
        }

        public IActionResult Index(Guid id)
        {
            var folder = _folderDataHelper.GetFolderById(id, _folderDataHelper.FoldersTree);
            return View(folder);
        }

        public IActionResult OneLevelUp(Guid destFolderId, bool returnPartial = false)
        {
            if (returnPartial)
            {
                var folder = _folderDataHelper.GetFolderById(destFolderId, _folderDataHelper.FoldersTree);
                return PartialView("FolderSelectView", folder);
            }
            return RedirectToAction("Index", new { id = destFolderId });
        }

        public IActionResult CreateProduct(Guid folderId)
        {
            return View(new Product{FolderId = folderId});
        }

        [HttpPost]
        public IActionResult CreateProduct(Product product)
        {
            var helper = new ProductsHelper();
            helper.AddProduct(product, _context);
            return RedirectToAction("Index", new { id = product.FolderId });
        }

        public async Task<IActionResult> CreateFolder(string name, Guid parentFolderId)
        {
            var folder = await _context.FolderTreeEntries.SingleOrDefaultAsync(m => m.Id == parentFolderId);
            _folderDataHelper.AddFolder(name, _context, folder);
            return RedirectToAction("Index", new{ id = parentFolderId });
        }

        public async Task<IActionResult> Edit(Guid id)
        {
            var product = await _context.Products.SingleOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        [HttpPost]
        public IActionResult Edit(Product product)
        {
            _folderDataHelper.UpdateProduct(product, _context);
            return View(product);
        }

        public IActionResult DeleteFolder(Guid id)
        {
            if(_folderDataHelper.HasSelectedFolders)
                _folderDataHelper.RemoveFolders(_context, id);
            return RedirectToAction("Index", new { id });
        }

        public IActionResult DeleteProduct(Guid id)
        {
            if (_folderDataHelper.HasSelectedProducts)
                _folderDataHelper.RemoveProducts(_context, id);
            return RedirectToAction("Index", new { id });
        }

        public void FolderCheckBoxClick(Guid? id)
        {
            if(id == null) return;
            _folderDataHelper.CheckFolder((Guid)id, _context);
        }

        public void ProductCheckBoxClick(Guid? id)
        {
            if(id == null) return;
            _folderDataHelper.CheckProduct((Guid)id, _context);
        }

        public IActionResult FolderClick(Guid id)
        {
            var folder = _folderDataHelper.GetFolderById(id, _folderDataHelper.FoldersTree);
            return PartialView("FolderSelectView", folder);
        }
    }
}

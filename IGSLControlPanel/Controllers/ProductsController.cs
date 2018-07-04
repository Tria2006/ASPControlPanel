using System;
using System.Threading.Tasks;
using IGSLControlPanel.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IGSLControlPanel.Helpers;
using IGSLControlPanel.Models;

namespace IGSLControlPanel.Controllers
{
    public class ProductsController : Controller
    {
        private readonly IGSLContext _context;
        private readonly FolderDataHelper _folderDataHelper;

        public ProductsController(IGSLContext context, FolderDataHelper helper)
        {
            _context = context;
            _folderDataHelper = helper;
            _folderDataHelper.Initialize(_context);
        }

        public IActionResult Index(Guid id)
        {
            var folder = _folderDataHelper.GetFolderById(id, _folderDataHelper.FoldersTree);
            return View(folder);
        }

        public IActionResult CreateProduct(Guid folderId)
        {
            return View(new Product{FolderId = folderId});
        }

        public IActionResult CreateProductSubmit(string name, Guid folderId)
        {
            _folderDataHelper.AddProduct(name, folderId, _context);
            return RedirectToAction("Index", new { id = folderId });
        }

        public async Task<IActionResult> CreateFolder(string name, Guid parentFolderId)
        {
            var folder = await _context.FolderTreeEntries.SingleOrDefaultAsync(m => m.Id == parentFolderId);
            _folderDataHelper.AddFolder(name, _context, folder);
            return RedirectToAction("Index", new{ id = parentFolderId });
        }

        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products.SingleOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        public IActionResult DeleteFolder(Guid id)
        {
            _folderDataHelper.RemoveFolders(_context, id);
            return RedirectToAction("Index", new { id });
        }

        public IActionResult DeleteProduct(Guid? id)
        {
            if (id == null) return NoContent();
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
    }
}

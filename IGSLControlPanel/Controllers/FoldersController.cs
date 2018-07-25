using System;
using System.Threading.Tasks;
using IGSLControlPanel.Data;
using IGSLControlPanel.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IGSLControlPanel.Controllers
{
    public class FoldersController : Controller
    {
        private readonly IGSLContext _context;
        private readonly FolderDataHelper _folderDataHelper;

        public FoldersController(IGSLContext context, FolderDataHelper helper)
        {
            _context = context;
            _folderDataHelper = helper;
            if(!_folderDataHelper.IsInitialized)_folderDataHelper.Initialize(_context);
        }

        public async Task<IActionResult> CreateFolder(string name, Guid parentFolderId)
        {
            var folder = await _context.FolderTreeEntries.SingleOrDefaultAsync(m => m.Id == parentFolderId);
            await _folderDataHelper.AddFolder(name, _context, folder);
            return RedirectToAction("Index", "Products", new { id = parentFolderId });
        }

        public async Task<IActionResult> DeleteFolder(Guid id)
        {
            if (_folderDataHelper.HasSelectedFolders)
                await _folderDataHelper.RemoveFolders(_context, id);
            return RedirectToAction("Index", "Products", new { id });
        }

        public bool FolderCheckBoxClick(Guid id)
        {
            _folderDataHelper.CheckFolder(id, _context);
            return _folderDataHelper.HasSelectedFolders;
        }

        public IActionResult FolderClick(Guid id)
        {
            var folder = _folderDataHelper.GetFolderById(id, _folderDataHelper.FoldersTree);
            _folderDataHelper.SelectedDestFolderId = id;
            return PartialView("FolderSelectView", folder);
        }

        public IActionResult OneLevelUp(Guid destFolderId, bool returnPartial = false)
        {
            if (!returnPartial) return RedirectToAction("Index", "Products", new { id = destFolderId });
            var folder = _folderDataHelper.GetFolderById(destFolderId, _folderDataHelper.FoldersTree);
            _folderDataHelper.SelectedDestFolderId = destFolderId;
            return PartialView("FolderSelectView", folder);
        }
    }
}
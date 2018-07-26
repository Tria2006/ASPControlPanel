using System;
using System.Linq;
using System.Threading.Tasks;
using IGSLControlPanel.Data;
using IGSLControlPanel.Enums;
using IGSLControlPanel.Helpers;
using IGSLControlPanel.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IGSLControlPanel.Controllers
{
    public class FoldersBaseController : Controller
    {
        private readonly IGSLContext _context;
        private readonly FolderDataHelper _folderDataHelper;
        public bool HasSelectedFolders => _folderDataHelper._checkedFolders.Any();

        protected FoldersBaseController(IGSLContext context, FolderDataHelper helper)
        {
            _context = context;
            _folderDataHelper = helper;
            _folderDataHelper.Initialize(_context);
        }

        public async Task<IActionResult> CreateFolder(string name, Guid parentFolderId, ModelTypes modelType)
        {
            var folder = await _context.FolderTreeEntries.SingleOrDefaultAsync(m => m.Id == parentFolderId);
            await _folderDataHelper.AddFolder(name, (int)modelType, _context, folder);
            return RedirectToAction("Index", modelType.ToString(), new { id = parentFolderId });
        }

        public async Task<IActionResult> DeleteFolder(Guid id, string controllerName)
        {
            if (HasSelectedFolders)
                await _folderDataHelper.RemoveFolders(_context, id);
            return RedirectToAction("Index", controllerName, new { id });
        }

        public bool FolderCheckBoxClick(Guid id)
        {
            _folderDataHelper.CheckFolder(id, _context);
            return HasSelectedFolders;
        }

        public IActionResult FolderClick(Guid id)
        {
            var folder = _folderDataHelper.GetFolderById(id, _folderDataHelper.FoldersTree);
            _folderDataHelper.SelectedDestFolderId = id;
            return PartialView("FolderSelectView", folder);
        }

        public IActionResult OneLevelUp(Guid destFolderId, string controllerName, bool returnPartial = false)
        {
            if (!returnPartial) return RedirectToAction("Index", controllerName, new { id = destFolderId });
            var folder = _folderDataHelper.GetFolderById(destFolderId, _folderDataHelper.FoldersTree);
            _folderDataHelper.SelectedDestFolderId = destFolderId;
            return PartialView("FolderSelectView", folder);
        }

        public FolderTreeEntry GetFolderById(Guid id)
        {
            return _folderDataHelper.GetFolderById(id, _folderDataHelper.FoldersTree);
        }

        public void MoveSelectedFolders()
        {
            _folderDataHelper.MoveSelectedFolders(_context);
        }

        public Guid GetSelectedDestFolderId()
        {
            return _folderDataHelper.SelectedDestFolderId;
        }

        public void BuildFolderTree(ModelTypes modelType)
        {
            _folderDataHelper.BuildFolderTree(modelType);
        }

        public FolderTreeEntry GetRootFolder()
        {
            return _folderDataHelper.FoldersTree;
        }
    }
}
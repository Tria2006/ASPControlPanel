﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DBModels.Models;
using IGSLControlPanel.Data;
using IGSLControlPanel.Enums;

namespace IGSLControlPanel.Helpers
{
    public class FolderDataHelper
    {
        public FolderTreeEntry FoldersTree { get; set; }
        private List<FolderTreeEntry> _folders { get; set; }
        public List<FolderTreeEntry> _checkedFolders { get; set; } = new List<FolderTreeEntry>();
        public Guid SelectedDestFolderId { get; set; }

        public void Initialize(IGSLContext _context)
        {
            // создается root папка с Id = Guid.Empty
            FoldersTree = new FolderTreeEntry();
            // папки из БД
            _folders = _context.FolderTreeEntries.ToList();
        }

        public FolderTreeEntry BuildFolderTree(ModelTypes modelType)
        {
            // первый уровень папок
            var firstLevelFolders = _folders.Where(s => (s.ParentFolderId == null || s.ParentFolderId == Guid.Empty) && !s.IsDeleted && s.ModelTypeId == (int)modelType);
            foreach (var folder in firstLevelFolders)
            {
                // заполняем ChildFolders
                BuildChildFolders(folder);

                FoldersTree.ChildFolders.Add(folder);
            }
            return FoldersTree;
        }

        private void BuildChildFolders(FolderTreeEntry parent)
        {
            // получаем родительскую папку по ID
            var folders = _folders.Where(x => x.ParentFolderId != null && x.ParentFolderId == parent.Id && !x.IsDeleted);
            foreach (var folder in folders)
            {
                // рекурсия для заполнения вложенных ChildFolders
                BuildChildFolders(folder);

                if (!parent.ChildFolders.Contains(folder))
                    parent.ChildFolders.Add(folder);
            }
        }

        public async Task<FolderTreeEntry> AddFolder(string name, int modelTypeId, IGSLContext _context, FolderTreeEntry parent = null)
        {
            // новая папка с введенным именем
            var newFolder = new FolderTreeEntry
            {
                Name = name,
                ModelTypeId = modelTypeId
            };

            // если parent не пустой, то помещаем новую папку в ChildFolders parent'a
            if (parent != null)
            {
                newFolder.ParentFolderId = parent.Id;
                parent.ChildFolders.Add(newFolder);
            }
            _context.FolderTreeEntries.Add(newFolder);
            await _context.SaveChangesAsync();
            return parent;
        }

        public async Task RemoveFolders(IGSLContext _context, Guid parentId)
        {
            // идем по списку выбранных папок
            foreach (var f in _checkedFolders)
            {
                // получаем папку из контекста и далее работаем с ней
                var contextFolder = _context.FolderTreeEntries.SingleOrDefault(x => x.Id == f.Id);
                if (contextFolder == null) continue;
                RemoveChildFoldersAndProducts(contextFolder);
                f.IsDeleted = true;
            }
            var parentFolder = GetFolderById(parentId, FoldersTree);
            parentFolder?.ChildFolders.RemoveAll(x => x.IsDeleted);
            await _context.SaveChangesAsync();
        }

        private void RemoveChildFoldersAndProducts(FolderTreeEntry parent)
        {
            // отвязывавем продукты от удаляемой папки
            parent.Products.ForEach(x => {x.FolderId = Guid.Empty;});
            parent.Tariffs.ForEach(x => {x.FolderId = Guid.Empty;});
            foreach (var childFolder in parent.ChildFolders)
            {
                // рекурсивно удаляем ChildFolders
                RemoveChildFoldersAndProducts(childFolder);
            }
            parent.IsDeleted = true;
        }

        public void CheckFolder(Guid id, IGSLContext _context)
        {
            // получаем папку из контекста
            var folder = _context.FolderTreeEntries.SingleOrDefault(x => x.Id == id);
            if (folder == null) return;
            // добавляем или удаляем папку из списка _checkedFolders
            if (_checkedFolders.Any(f => f.Id == folder.Id))
                _checkedFolders.RemoveAll(f => f.Id == folder.Id);
            else
                _checkedFolders.Add(folder);
        }

        public FolderTreeEntry GetFolderById(Guid id, FolderTreeEntry folder)
        {
            // рекурсивный поиск по Id папок по дереву
            if (folder.Id == id) return folder;
            var childs1 = folder.ChildFolders.Where(x => !x.IsDeleted);
            foreach (var f in childs1)
            {
                if (f.Id == id) return f;
                var childs2 = f.ChildFolders.Where(x => !x.IsDeleted);
                foreach (var childFolder in childs2)
                {
                    var result = GetFolderById(id, childFolder);
                    if (result != null) return result;
                }
            }
            return null;
        }

        public void MoveSelectedFolders(IGSLContext context)
        {
            foreach (var folder in _checkedFolders)
            {
                var contextFolder = context.FolderTreeEntries.SingleOrDefault(f => f.Id == folder.Id);
                if(contextFolder == null) continue;
                contextFolder.ParentFolderId = SelectedDestFolderId;
            }
            _checkedFolders.Clear();
        }
    }
}

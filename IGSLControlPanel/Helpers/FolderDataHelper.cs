using System;
using System.Collections.Generic;
using System.Linq;
using IGSLControlPanel.Data;
using IGSLControlPanel.Models;

namespace IGSLControlPanel.Helpers
{
    public class FolderDataHelper
    {
        public FolderTreeEntry FoldersTree { get; set; }
        private List<FolderTreeEntry> _folders { get; set; }
        private List<FolderTreeEntry> _checkedFolders { get; set; }
        private List<Product> _checkedProducts { get; }
        private readonly ProductsHelper _productsHelper;

        public bool HasSelectedFolders => _checkedFolders.Any();
        public bool HasSelectedProducts => _checkedProducts.Any();

        public FolderDataHelper()
        {
            _checkedFolders = new List<FolderTreeEntry>();
            _checkedProducts = new List<Product>();
            _productsHelper = new ProductsHelper();
        }

        public void Initialize(IGSLContext _context)
        {
            // создается root папка с Id = Guid.Empty
            FoldersTree = new FolderTreeEntry();
            
            // папки из БД
            _folders = _context.FolderTreeEntries.ToList();

            _productsHelper.Initialize(_context);

            // формирование дерева папок
            BuildFolderTree();
        }

        public FolderTreeEntry BuildFolderTree()
        {
            // первый уровень папок
            var firstLevelFolders = _folders.Where(s => (s.ParentFolderId == null || s.ParentFolderId == Guid.Empty) && !s.IsDeleted);
            foreach (var folder in firstLevelFolders)
            {
                // заполняем ChildFolders
                BuildChildFolders(folder);

                // заполняем Products
                _productsHelper.BuildProducts(folder);

                FoldersTree.ChildFolders.Add(folder);
            }
            FoldersTree.Products.AddRange(_productsHelper.RootProducts);
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

                // заполняем продукты
                _productsHelper.BuildProducts(folder);

                if (!parent.ChildFolders.Contains(folder))
                    parent.ChildFolders.Add(folder);
            }
        }

        public FolderTreeEntry AddFolder(string name, IGSLContext _context, FolderTreeEntry parent = null)
        {
            // новая папка с введенным именем
            var newFolder = new FolderTreeEntry
            {
                Name = name
            };

            // если parent не пустой, то помещаем новую папку в ChildFolders parent'a
            if (parent != null)
            {
                newFolder.ParentFolderId = parent.Id;
                parent.ChildFolders.Add(newFolder);
            }
            _context.FolderTreeEntries.Add(newFolder);
            _context.SaveChanges();
            return parent;
        }

        public void RemoveFolders(IGSLContext _context, Guid parentId)
        {
            // идем по списку выбранных папок
            foreach (var f in _checkedFolders)
            {
                // получаем папку из контекста и далее работаем с ней
                var contextFolder = _context.FolderTreeEntries.SingleOrDefault(x => x.Id == f.Id);
                if (contextFolder == null) continue;
                RemoveChildFoldersAndProducts(contextFolder, _context);
                f.IsDeleted = true;
            }
            var parentFolder = GetFolderById(parentId, FoldersTree);
            parentFolder?.ChildFolders.RemoveAll(x => x.IsDeleted);
            _context.SaveChanges();
            _checkedFolders = new List<FolderTreeEntry>();
        }

        private void RemoveChildFoldersAndProducts(FolderTreeEntry parent, IGSLContext _context)
        {
            // отвязывавем продукты от удаляемой папки
            parent.Products.ForEach(x => _productsHelper.RemoveFolderId(x, _context));
            foreach (var childFolder in parent.ChildFolders)
            {
                // рекурсивно удаляем ChildFolders
                RemoveChildFoldersAndProducts(childFolder, _context);
            }
            parent.IsDeleted = true;
        }

        public void CheckFolder(Guid id, IGSLContext _context)
        {
            // получаем папку из контекста
            var folder = _context.FolderTreeEntries.SingleOrDefault(x => x.Id == id);
            if (folder == null) return;
            // добавляем или удаляем папку из списка _checkedFolders
            if (_checkedFolders.Contains(folder))
                _checkedFolders.Remove(folder);
            else
                _checkedFolders.Add(folder);
        }

        public void CheckProduct(Guid id, IGSLContext _context)
        {
            // получаем продукт из контекста
            var product = _context.Products.SingleOrDefault(x => x.Id == id);
            if (product == null) return;
            // добавляем или удаляем продукт из списка _checkedFolders
            if (_checkedProducts.Contains(product))
                _checkedProducts.Remove(product);
            else
                _checkedProducts.Add(product);
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

        public void UpdateProduct(Product product, IGSLContext context)
        {
            var parentFolder = GetFolderById(product.FolderId ?? Guid.Empty, FoldersTree);
            _productsHelper.UpdateProduct(product, parentFolder, context);
        }

        public void RemoveProducts(IGSLContext context, Guid? parentId)
        {
            var parentFolder = GetFolderById(parentId ?? Guid.Empty, FoldersTree);
            _productsHelper.RemoveProducts(context, _checkedProducts, parentFolder);
        }
    }
}

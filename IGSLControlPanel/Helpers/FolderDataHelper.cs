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
        private List<Product> _products { get; set; }
        private List<FolderTreeEntry> _folders { get; set; }
        private List<Product> _productsWOFolder { get; set; }
        private List<FolderTreeEntry> _checkedFolders { get; set; } = new List<FolderTreeEntry>();
        private List<Product> _checkedProducts { get; set; } = new List<Product>();

        public bool HasSelectedFolders => _checkedFolders.Any();
        public bool HasSelectedProducts => _checkedProducts.Any();

        public void Initialize(IGSLContext _context)
        {
            if (_checkedFolders == null) _checkedFolders = new List<FolderTreeEntry>();
            if (_checkedProducts == null) _checkedProducts = new List<Product>();
            FoldersTree = new FolderTreeEntry();
            _folders = _context.FolderTreeEntries.ToList();
            _products = _context.Products.ToList();
            _productsWOFolder = _products.Where(x => (x.FolderId == null || x.FolderId == Guid.Empty) && !x.IsDeleted).ToList();
            BuildFolderTree();
        }

        public FolderTreeEntry BuildFolderTree()
        {
            var firstLevelFolders = _folders.Where(s => s.ParentFolderId == null && !s.IsDeleted);
            foreach (var folder in firstLevelFolders)
            {
                BuildChildFolders(folder);
                BuildProducts(folder);
                FoldersTree.ChildFolders.Add(folder);
            }
            FoldersTree.Products.AddRange(_productsWOFolder);
            return FoldersTree;
        }

        private void BuildChildFolders(FolderTreeEntry parent)
        {
            var folders = _folders.Where(x => x.ParentFolderId != null && x.ParentFolderId == parent.Id && !x.IsDeleted);
            foreach (var folder in folders)
            {
                BuildChildFolders(folder);
                BuildProducts(folder);
                if (!parent.ChildFolders.Contains(folder))
                    parent.ChildFolders.Add(folder);
            }
        }

        private void BuildProducts(FolderTreeEntry parent)
        {
            var products = _products.Where(x => x.FolderId != null && x.FolderId == parent.Id);
            foreach (var product in products)
            {
                if (!parent.Products.Contains(product))
                    parent.Products.Add(product);
            }
        }

        public FolderTreeEntry AddFolder(string name, IGSLContext _context, FolderTreeEntry parent = null)
        {
            var newFolder = new FolderTreeEntry
            {
                Name = name
            };
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
            foreach (var f in _checkedFolders)
            {
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
            parent.Products.ForEach(x => RemoveFolderId(x, _context));
            foreach (var childFolder in parent.ChildFolders)
            {
                RemoveChildFoldersAndProducts(childFolder, _context);
            }
            parent.IsDeleted = true;
        }

        public void RemoveProducts(IGSLContext _context, Guid? parentId = null)
        {
            foreach (var f in _checkedProducts)
            {
                var contextFolder = _context.Products.SingleOrDefault(x => x.Id == f.Id);
                if (contextFolder == null) continue;
                contextFolder.IsDeleted = true;
            }

            if (parentId != null)
            {
                var parentFolder = GetFolderById((Guid)parentId, FoldersTree);
                parentFolder?.Products.RemoveAll(x => x.IsDeleted);
            }
            _productsWOFolder.RemoveAll(x => x.IsDeleted);
            _context.SaveChanges();
            _checkedProducts = new List<Product>();
        }

        public void CheckFolder(Guid id, IGSLContext _context)
        {
            var folder = _context.FolderTreeEntries.SingleOrDefault(x => x.Id == id);
            if (folder == null) return;
            if (_checkedFolders.Contains(folder))
                _checkedFolders.Remove(folder);
            else
                _checkedFolders.Add(folder);
        }

        public void CheckProduct(Guid id, IGSLContext _context)
        {
            var product = _context.Products.SingleOrDefault(x => x.Id == id);
            if (product == null) return;
            if (_checkedProducts.Contains(product))
                _checkedProducts.Remove(product);
            else
                _checkedProducts.Add(product);
        }

        public void RemoveFolderId(Product p, IGSLContext _context)
        {
            var product = _context.Products.FirstOrDefault(x => x.Id == p.Id);
            if (product == null) return;
            product.FolderId = Guid.Empty;
            _context.SaveChanges();
        }

        public void AddProduct(string name, Guid parentId, IGSLContext _context)
        {
            var newProduct = new Product
            {
                Name = name,
                FolderId = parentId
            };
            _context.Products.Add(newProduct);
            _context.SaveChanges();
        }

        public FolderTreeEntry GetFolderById(Guid id, FolderTreeEntry folder)
        {
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
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using IGSLControlPanel.Data;
using IGSLControlPanel.Models;

namespace IGSLControlPanel.Helpers
{
    public class FolderDataHelper
    {
        public FolderTreeEntry FoldersTree { get; set; } = new FolderTreeEntry();
        private List<Product> _products { get; set; }
        private List<FolderTreeEntry> _folders { get; set; }
        private IEnumerable<Product> _productsWOFolder { get; set; }
        private List<FolderTreeEntry> _checkedFolders { get; set; } = new List<FolderTreeEntry>();
        
        public void Initialize(IGSLContext _context)
        {
            if(_checkedFolders == null) _checkedFolders = new List<FolderTreeEntry>();
            _folders = _context.FolderTreeEntries.ToList();
            _products = _context.Products.ToList();
            _productsWOFolder = _products.Where(x => x.FolderId == null);
            BuildFolderTree();
        }

        public void RenewFolders(IGSLContext _context)
        {
            _folders = new List<FolderTreeEntry>();
            _products = new List<Product>();
            _folders = _context.FolderTreeEntries.ToList();
            _products = _context.Products.ToList();
            _productsWOFolder = _products.Where(x => x.FolderId == null);
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

        public void RemoveFolders(IGSLContext _context, Guid? parentId = null)
        {
            foreach (var f in _checkedFolders)
            {
                //f.Products.ForEach(x => RemoveFolderId(x, _context));
                //foreach (var childFolder in f.ChildFolders)
                //{
                //    RemoveFolders(_context, childFolder.Id);
                //}
                //f.IsDeleted = true;
                //_context.SaveChanges();
            }
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

        public void RemoveFolderId(Product p, IGSLContext _context)
        {
            var product = _context.Products.FirstOrDefault(x => x.Id == p.Id);
            if (product == null) return;
            product.FolderId = Guid.Empty;
            _context.SaveChanges();
        }

        public Guid AddProduct(string name, FolderTreeEntry parent, IGSLContext _context)
        {
            var newProduct = new Product
            {
                Name = name,
                FolderId = parent.Id
            };

            _context.Products.Add(newProduct);
            _context.SaveChanges();
            return newProduct.Id;
        }

        public void RemoveProduct(Product p, IGSLContext _context)
        {
            var product = _context.Products.FirstOrDefault(x => x.Id == p.Id);
            if (product == null) return;
            _context.Products.Remove(product);
            _context.SaveChanges();
        }

        public FolderTreeEntry GetFolderById(Guid id, FolderTreeEntry folder)
        {
            if (folder.Id == id) return folder;
            foreach (var f in folder.ChildFolders.Where(x => !x.IsDeleted))
            {
                if (f.Id == id) return f;
                foreach (var childFolder in f.ChildFolders.Where(x => !x.IsDeleted))
                {
                    var result = GetFolderById(id, childFolder);
                    if (result != null) return result;
                }
            }
            return null;
        }
    }
}

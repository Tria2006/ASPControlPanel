using System;
using System.Collections.Generic;
using System.Linq;
using IGSLControlPanel.Data;
using IGSLControlPanel.Models;

namespace IGSLControlPanel.Helpers
{
    public class FolderDataHelper
    {
        private readonly IGSLContext _context;
        public FolderTreeEntry FoldersTree { get; set; } = new FolderTreeEntry();
        private List<Product> _products { get; set; }
        private List<FolderTreeEntry> _folders { get; set; }
        private IEnumerable<Product> _productsWOFolder { get; set; }

        public FolderDataHelper(IGSLContext context)
        {
            _context = context;
            _folders = _context.FolderTreeEntries.ToList();
            _products = _context.Products.ToList();
            _productsWOFolder = _products.Where(x => x.FolderId == null);
            BuildFolderTree();
        }

        public void RenewFolders()
        {
            _folders = new List<FolderTreeEntry>();
            _products = new List<Product>();
            _folders = _context.FolderTreeEntries.ToList();
            _products = _context.Products.ToList();
            _productsWOFolder = _products.Where(x => x.FolderId == null);
        }

        public FolderTreeEntry BuildFolderTree()
        {
            var firstLevelFolders = _folders.Where(s => s.ParentFolderId == null);
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
            var folders = _folders.Where(x => x.ParentFolderId != null && x.ParentFolderId == parent.Id);
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

        public FolderTreeEntry AddFolder(string name, FolderTreeEntry parent = null)
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

        public void RemoveFolder(FolderTreeEntry folder)
        {
            folder.Products.ForEach(RemoveFolderId);
            folder.ChildFolders.ForEach(RemoveFolder);
            var contextFolder = _context.FolderTreeEntries.FirstOrDefault(x => x.Id == folder.Id);
            if (contextFolder == null) return;
            _context.FolderTreeEntries.Remove(contextFolder);
            _context.SaveChanges();
        }

        public void RemoveFolderId(Product p)
        {
            var product = _context.Products.FirstOrDefault(x => x.Id == p.Id);
            if (product == null) return;
            product.FolderId = null;
            _context.SaveChanges();
        }

        public Guid AddProduct(string name, FolderTreeEntry parent)
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

        public void RemoveProduct(Product p)
        {
            var product = _context.Products.FirstOrDefault(x => x.Id == p.Id);
            if (product == null) return;
            _context.Products.Remove(product);
            _context.SaveChanges();
        }

        public FolderTreeEntry GetFolderById(Guid id, FolderTreeEntry folder)
        {
            if (folder.Id == id) return folder;
            foreach (var f in folder.ChildFolders)
            {
                if (f.Id == id) return f;
                foreach (var childFolder in f.ChildFolders)
                {
                    var result = GetFolderById(id, childFolder);
                    if (result != null) return result;
                }
            }
            return null;
        }
    }
}

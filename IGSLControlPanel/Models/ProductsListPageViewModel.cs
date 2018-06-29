using System;
using System.Collections.Generic;
using System.Linq;
using IGSLControlPanel.Data;

namespace IGSLControlPanel.Models
{
    public class ProductsListPageViewModel
    {
        private readonly IGSLContext _context;
        public List<FolderTreeEntry> FoldersTree { get; set; } = new List<FolderTreeEntry>();
        private List<Product> _products { get; set; } = new List<Product>();
        private List<FolderTreeEntry> _folders { get; set; } = new List<FolderTreeEntry>();
        public IEnumerable<Product> ProductsWOFolder { get; private set; }

        public ProductsListPageViewModel(IGSLContext context)
        {
            _context = context;
            _folders = _context.FolderTreeEntries.ToList();
            _products = _context.Products.ToList();
            ProductsWOFolder = _products.Where(x => x.FolderId == null);
            BuildFolderTree();
        }

        public void RenewFolders()
        {
            _folders = FoldersTree = new List<FolderTreeEntry>();
            _products = new List<Product>();
            _folders = _context.FolderTreeEntries.ToList();
            _products = _context.Products.ToList();
            ProductsWOFolder = _products.Where(x => x.FolderId == null);
        }

        public List<FolderTreeEntry> BuildFolderTree()
        {
            var firstLevelFolders = _folders.Where(s => s.ParentFolderId == null);
            foreach (var folder in firstLevelFolders)
            {
                BuildChildFolders(folder);
                BuildProducts(folder);
                FoldersTree.Add(folder);
            }
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

        public Guid AddFolder(string name, FolderTreeEntry parent = null)
        {
            var newFolder = new FolderTreeEntry
            {
                Name = name
            };
            if (parent != null) newFolder.ParentFolderId = parent.Id;
            _context.FolderTreeEntries.Add(newFolder);
            _context.SaveChanges();
            return newFolder.Id;
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

        private void CreateTestProducts()
        {
            for (int i = 1; i < 30; i++)
            {
                _products.Add(new Product
                {
                    Name = $"Product {i}"
                });
            }
            _context.Products.AddRange(_products);
            _context.SaveChanges();
        }

        private void CreateFolderTestData()
        {
            for (int i = 1; i < 5; i++)
            {
                _folders.Add(new FolderTreeEntry
                {
                    Name = $"Folder {i}",
                    ParentFolderId = Guid.Parse("A09FCCB3-2676-E811-ABDD-5404A6BFD6F2")
                });
            }
            _context.FolderTreeEntries.AddRange(_folders);
            _context.SaveChanges();
        }
    }
}

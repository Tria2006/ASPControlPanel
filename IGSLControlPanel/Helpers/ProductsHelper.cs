using System;
using System.Collections.Generic;
using System.Linq;
using IGSLControlPanel.Data;
using IGSLControlPanel.Models;
using Microsoft.EntityFrameworkCore;

namespace IGSLControlPanel.Helpers
{
    public class ProductsHelper
    {
        private List<Product> _products { get; set; }
        public List<Product> RootProducts { get; set; }
        public Product CurrentProduct { get; set; }
        public ProductParameter CurrentParameter { get;
            set; }
        public bool IsProductCreateInProgress { get; set; }
        public bool IsParameterCreateInProgress { get; set; }

        public void Initialize(IGSLContext _context)
        {
            // продукты получаем вместе со связанными параметрами 
            _products = _context.Products.Include(x => x.LinkToProductParameters).ThenInclude(p => p.Parameter).Where(s => !s.IsDeleted).ToList();

            // загружаем лимиты для параметров
            _products.ForEach(p =>
            {
                foreach (var link in p.LinkToProductParameters)
                {
                    link.Parameter.Limit =
                        _context.ValueLimits.FirstOrDefault(
                            x => x.ProductId == p.Id && x.ParameterId == link.ProductParameterId && !x.IsDeleted);
                }
            });
            // продукты, не привязанные ни к какой папке
            RootProducts = _products.Where(x => (x.FolderId == null || x.FolderId == Guid.Empty) && !x.IsDeleted).ToList();
        }

        public void BuildProducts(FolderTreeEntry parent)
        {
            var products = _products.Where(x => (x.FolderId != null && x.FolderId == parent.Id) && !x.IsDeleted);
            foreach (var product in products)
            {
                if (parent.Products.Contains(product)) continue;
                parent.Products.Add(product);
            }
        }

        public void RemoveProducts(IGSLContext _context, List<Product> _checkedProducts, FolderTreeEntry parentFolder)
        {
            // двигаемся по списку выбранных продуктов
            foreach (var f in _checkedProducts)
            {
                // получаем продукт из контекста и далее работавем с ним
                var contextProduct = _context.Products.SingleOrDefault(x => x.Id == f.Id);
                if (contextProduct == null) continue;
                contextProduct.IsDeleted = true;
            }

            // удаляются продукты только из FoldersTree
            // в контексте БД они остаются
            parentFolder?.Products.RemoveAll(x => x.IsDeleted);

            // удалить продукты нужно и из _productsWOFolder
            RootProducts.RemoveAll(x => x.IsDeleted);
            _context.SaveChanges();

            // очищаем список выбранных продуктов
            _checkedProducts.Clear();
            BuildProducts(parentFolder);
        }

        public void RemoveFolderId(Product p, IGSLContext _context)
        {
            // отвязываем продукт от папки
            var product = _context.Products.FirstOrDefault(x => x.Id == p.Id);
            if (product == null) return;
            product.FolderId = Guid.Empty;
            _context.SaveChanges();
        }

        public void UpdateProductFields(Product oldProduct, Product newProduct)
        {
            oldProduct.Name = newProduct.Name;
            oldProduct.ValidFrom = newProduct.ValidFrom;
            oldProduct.ValidTo = newProduct.ValidTo;
        }

        public void AddProduct(Product product,IGSLContext _context)
        {
            _context.Products.Add(product);
            _context.SaveChanges();
        }

        public void UpdateProduct(Product product, FolderTreeEntry parentFolder, IGSLContext _context)
        {
            var contextProduct = _context.Products.SingleOrDefault(x => x.Id == product.Id);
            if (contextProduct == null) return;
            contextProduct.ValidFrom = product.ValidFrom;
            contextProduct.ValidTo = product.ValidTo;
            _context.SaveChanges();
            BuildProducts(parentFolder);
        }

        public void SelectUnselectParameter(Guid parameterId)
        {
            CurrentParameter = CurrentProduct.LinkToProductParameters
                .SingleOrDefault(s => s.ProductParameterId == parameterId)
                ?.Parameter;
        }
    }
}

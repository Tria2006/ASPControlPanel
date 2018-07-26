﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IGSLControlPanel.Data;
using IGSLControlPanel.Models;
using Microsoft.EntityFrameworkCore;

namespace IGSLControlPanel.Helpers
{
    public class ProductsHelper
    {
        private List<Product> _products { get; set; }
        //public List<Product> RootProducts { get; set; }
        public Product CurrentProduct { get; set; }
        public ProductParameter CurrentParameter { get; set; }
        public bool IsProductCreateInProgress { get; set; }
        public bool IsParameterCreateInProgress { get; set; }
        private List<Product> _checkedProducts { get; } = new List<Product>();
        public bool HasSelectedProducts => _checkedProducts.Any();

        public void Initialize(IGSLContext _context, FolderTreeEntry rootFolder)
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
            //RootProducts = _products.Where(x => (x.FolderId == null || x.FolderId == Guid.Empty) && !x.IsDeleted).ToList();

            BuildProducts(rootFolder);
        }

        private void BuildProducts(FolderTreeEntry parent)
        {
            var products = _products.Where(x => x.FolderId == parent.Id && !x.IsDeleted);
            foreach (var product in products)
            {
                if (parent.Products.Contains(product)) continue;
                parent.Products.Add(product);
            }

            foreach (var childFolder in parent.ChildFolders)
            {
                BuildProducts(childFolder);
            }
        }

        public async Task RemoveProducts(IGSLContext _context, FolderTreeEntry parentFolder)
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
            //RootProducts.RemoveAll(x => x.IsDeleted);
            await _context.SaveChangesAsync();

            // очищаем список выбранных продуктов
            _checkedProducts.Clear();
            BuildProducts(parentFolder);
        }

        public async Task RemoveFolderId(Product p, IGSLContext _context)
        {
            // отвязываем продукт от папки
            var product = _context.Products.FirstOrDefault(x => x.Id == p.Id);
            if (product == null) return;
            product.FolderId = Guid.Empty;
            await _context.SaveChangesAsync();
        }

        public async Task AddProduct(Product product,IGSLContext _context)
        {
            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateProduct(Product product, FolderTreeEntry parentFolder, IGSLContext _context)
        {
            var contextProduct = _context.Products.SingleOrDefault(x => x.Id == product.Id);
            if (contextProduct == null) return;
            contextProduct.ValidFrom = product.ValidFrom;
            contextProduct.ValidTo = product.ValidTo;
            await _context.SaveChangesAsync();
            BuildProducts(parentFolder);
        }

        public void SelectUnselectParameter(Guid parameterId)
        {
            CurrentParameter = CurrentProduct.LinkToProductParameters
                .SingleOrDefault(s => s.ProductParameterId == parameterId)
                ?.Parameter;
        }

        public void CheckProduct(Guid id, IGSLContext _context)
        {
            // получаем продукт из контекста
            var product = _context.Products.SingleOrDefault(x => x.Id == id);
            if (product == null) return;
            // добавляем или удаляем продукт из списка _checkedFolders
            if (_checkedProducts.Any(p => p.Id == product.Id))
                _checkedProducts.RemoveAll(p => p.Id == product.Id);
            else
                _checkedProducts.Add(product);
        }

        public void MoveSelectedProducts(IGSLContext context, Guid selectedDestFolderId)
        {
            foreach (var product in _checkedProducts)
            {
                var contextProduct = context.Products.SingleOrDefault(p => p.Id == product.Id);
                if (contextProduct == null) continue;
                contextProduct.FolderId = selectedDestFolderId;
            }
            _checkedProducts.Clear();
            context.SaveChanges();
        }
    }
}

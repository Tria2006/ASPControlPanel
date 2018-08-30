using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DBModels.Models;
using IGSLControlPanel.Data;
using log4net;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace IGSLControlPanel.Helpers
{
    public class ProductsHelper
    {
        private List<Product> _products { get; set; }
        public Product CurrentProduct { get; set; }
        public ProductParameter CurrentParameter { get; set; }
        private List<Product> _checkedProducts { get; } = new List<Product>();
        public ValueLimit LimitWOChanges { get; set; }
        public bool HasSelectedProducts => _checkedProducts.Any();

        public void Initialize(IGSLContext _context, FolderTreeEntry rootFolder)
        {
            // продукты получаем вместе со связанными параметрами 
            _products = _context.Products
                .Include(x => x.LinkToProductParameters)
                .ThenInclude(p => p.Parameter)
                .Where(s => !s.IsDeleted).ToList();

            // загружаем лимиты для параметров
            _products.ForEach(p => LoadProductLimits(p, _context));

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

        public async Task RemoveProducts(IGSLContext _context, FolderTreeEntry parentFolder, ILog logger, IHttpContextAccessor _httpAccessor)
        {
            // двигаемся по списку выбранных продуктов
            foreach (var product in _checkedProducts)
            {
                // получаем продукт из контекста и далее работавем с ним
                var contextProduct = _context.Products.Include(x => x.LinkToProductParameters).ThenInclude(x => x.Parameter).SingleOrDefault(x => x.Id == product.Id);
                if (contextProduct == null) continue;
                // проставляем IsDeleted всем связанным параметрам
                contextProduct.LinkToProductParameters.ForEach(l =>
                {
                    l.Parameter.IsDeleted = true;
                });
                // удаляем связи
                contextProduct.LinkToProductParameters.Clear();
                contextProduct.IsDeleted = true;
                logger.Info($"{_httpAccessor.HttpContext.Connection.RemoteIpAddress} deleted(set IsDeleted=true) Product (id={product.Id})");
            }

            // удаляются продукты только из FoldersTree
            // в контексте БД они остаются
            parentFolder?.Products.RemoveAll(x => x.IsDeleted);

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
            contextProduct.LinkToProductParameters = product.LinkToProductParameters;
            contextProduct.Name = product.Name;
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

        public void LoadProductLimits(Product product, IGSLContext context)
        {
            foreach (var link in product.LinkToProductParameters)
            {
                link.Parameter.Limit =
                    context.ValueLimits.Include(x => x.LimitListItems).FirstOrDefault(
                        x => x.ProductId == product.Id && x.ParameterId == link.ProductParameterId && !x.IsDeleted);
            }
        }
    }
}

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
        private List<Product> Products { get; set; }
        public Product CurrentProduct { get; set; }
        public ProductParameter CurrentParameter { get; set; }
        private List<Product> CheckedProducts { get; } = new List<Product>();
        public ValueLimit LimitWOChanges { get; set; }
        public bool HasSelectedProducts => CheckedProducts.Any();

        public void Initialize(IGSLContext context, FolderTreeEntry rootFolder)
        {
            // продукты получаем вместе со связанными параметрами 
            Products = context.Products
                .Include(x => x.LinkToProductParameters)
                .ThenInclude(p => p.Parameter)
                .Where(s => !s.IsDeleted).ToList();

            // загружаем лимиты для параметров
            Products.ForEach(p => LoadProductLimits(p, context));

            BuildProducts(rootFolder);
        }

        private void BuildProducts(FolderTreeEntry parent)
        {
            var products = Products.Where(x => x.FolderId == parent.Id && !x.IsDeleted);
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

        public async Task RemoveProducts(IGSLContext context, FolderTreeEntry parentFolder, ILog logger, IHttpContextAccessor httpAccessor)
        {
            // двигаемся по списку выбранных продуктов
            foreach (var product in CheckedProducts)
            {
                // получаем продукт из контекста и далее работавем с ним
                var contextProduct = context.Products.Include(x => x.LinkToProductParameters).ThenInclude(x => x.Parameter).SingleOrDefault(x => x.Id == product.Id);
                if (contextProduct == null) continue;
                // проставляем IsDeleted всем связанным параметрам
                contextProduct.LinkToProductParameters.ForEach(l =>
                {
                    l.Parameter.IsDeleted = true;
                });
                // удаляем связи
                contextProduct.LinkToProductParameters.Clear();
                contextProduct.IsDeleted = true;
                logger.Info($"{httpAccessor.HttpContext.Connection.RemoteIpAddress} deleted(set IsDeleted=true) Product (id={product.Id})");
            }

            // удаляются продукты только из FoldersTree
            // в контексте БД они остаются
            parentFolder?.Products.RemoveAll(x => x.IsDeleted);

            await context.SaveChangesAsync();

            // очищаем список выбранных продуктов
            CheckedProducts.Clear();
            BuildProducts(parentFolder);
        }

        public async Task RemoveFolderId(Product p, IGSLContext context)
        {
            // отвязываем продукт от папки
            var product = context.Products.FirstOrDefault(x => x.Id == p.Id);
            if (product == null) return;
            product.FolderId = Guid.Empty;
            await context.SaveChangesAsync();
        }

        public async Task AddProduct(Product product,IGSLContext context)
        {
            await context.Products.AddAsync(product);
            await context.SaveChangesAsync();
        }

        public async Task UpdateProduct(Product product, FolderTreeEntry parentFolder, IGSLContext context)
        {
            var contextProduct = context.Products.SingleOrDefault(x => x.Id == product.Id);
            if (contextProduct == null) return;
            contextProduct.ValidFrom = product.ValidFrom;
            contextProduct.ValidTo = product.ValidTo;
            contextProduct.LinkToProductParameters = product.LinkToProductParameters;
            contextProduct.Name = product.Name;
            await context.SaveChangesAsync();
            BuildProducts(parentFolder);
        }

        public void SelectUnselectParameter(Guid parameterId, IGSLContext context)
        {
            CurrentParameter = context.ProductParameters.Find(parameterId);
        }

        public void CheckProduct(Guid id, IGSLContext context)
        {
            // получаем продукт из контекста
            var product = context.Products.SingleOrDefault(x => x.Id == id);
            if (product == null) return;
            // добавляем или удаляем продукт из списка _checkedFolders
            if (CheckedProducts.Any(p => p.Id == product.Id))
                CheckedProducts.RemoveAll(p => p.Id == product.Id);
            else
                CheckedProducts.Add(product);
        }

        public void MoveSelectedProducts(IGSLContext context, Guid selectedDestFolderId)
        {
            foreach (var product in CheckedProducts)
            {
                var contextProduct = context.Products.SingleOrDefault(p => p.Id == product.Id);
                if (contextProduct == null) continue;
                contextProduct.FolderId = selectedDestFolderId;
            }
            CheckedProducts.Clear();
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IGSLControlPanel.Data;
using IGSLControlPanel.Helpers;
using IGSLControlPanel.Models;
using IGSLControlPanel.Models.ManyToManyLinks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace IGSLControlPanel.Controllers
{
    public class ProductParametersController : Controller
    {
        private readonly IGSLContext _context;
        private readonly ProductsHelper _productsHelper;

        public ProductParametersController(IGSLContext context, ProductsHelper productsHelper)
        {
            _context = context;
            _productsHelper = productsHelper;
        }

        public IActionResult Create()
        {
            ViewData["ParamGroups"] = new SelectList(_context.ParameterGroups, "Id", "Name");
            _productsHelper.IsParameterCreateInProgress = true;
            var tempParam = new ProductParameter
            {
                LinkToProduct = new List<ProductLinkToProductParameter>
                {
                    new ProductLinkToProductParameter
                    {
                        Product = _productsHelper.CurrentProduct,
                        ProductId = _productsHelper.CurrentProduct.Id
                    }
                }
            };
            _productsHelper.CurrentParameter = tempParam;
            return View(tempParam);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,IsDeleted,IsRequiredForCalc,IsRequiredForSave,DataType,Order,Limit")] ProductParameter productParameter)
        {
            if (!ModelState.IsValid) return View(productParameter);
            _context.Add(productParameter);
            _context.SaveChanges();
            productParameter.LinkToProduct = new List<ProductLinkToProductParameter>{ new ProductLinkToProductParameter
            {
                ProductId = _productsHelper.CurrentProduct.Id,
                ProductParameterId = productParameter.Id
            }};
            await _context.SaveChangesAsync();
            _productsHelper.IsParameterCreateInProgress = false;
            await CheckParameterOrders(productParameter);
            _productsHelper.CurrentParameter = null;
            return RedirectToAction(_productsHelper.IsProductCreateInProgress ? "CreateProduct" : "Edit", "Products", _productsHelper.CurrentProduct);
        }

        public IActionResult Edit(Guid id)
        {
            ViewData["ParamGroups"] = new SelectList(_context.ParameterGroups, "Id", "Name");
            var productParameter =
                _productsHelper.CurrentProduct.LinkToProductParameters.SingleOrDefault(x => x.ProductParameterId == id);
            if (productParameter == null)
            {
                return NotFound();
            }
            _productsHelper.CurrentParameter = productParameter.Parameter;
            return View(productParameter.Parameter);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, ProductParameter productParameter)
        {
            if (id != productParameter.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid) return View(productParameter);
            try
            {
                _context.Update(productParameter);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductParameterExists(productParameter.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            await CheckParameterOrders(productParameter);
            _productsHelper.CurrentParameter = null;
            return RedirectToAction(_productsHelper.IsProductCreateInProgress ? "CreateProduct" : "Edit", "Products", _productsHelper.CurrentProduct);
        }

        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var productParameter = await _context.ProductParameters
                .FirstOrDefaultAsync(m => m.Id == id);
            if (productParameter == null)
            {
                return NotFound();
            }

            return View(productParameter);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var productParameter = _context.ProductParameters.Include(x => x.LinkToProduct).ThenInclude(p => p.Product).SingleOrDefault(x => x.Id == id);
            if (productParameter == null) return RedirectToAction("Edit", "Products", _productsHelper.CurrentProduct);
            productParameter.IsDeleted = true;
            var link = productParameter.LinkToProduct.SingleOrDefault(
                p => p.ProductParameterId == id && p.ProductId == _productsHelper.CurrentProduct.Id);
            if (link != null)
            {
                productParameter.LinkToProduct.Remove(link);
            }
            await _context.SaveChangesAsync();
            return RedirectToAction("Edit", "Products", _productsHelper.CurrentProduct);
        }

        private bool ProductParameterExists(Guid id)
        {
            return _context.ProductParameters.Any(e => e.Id == id);
        }

        public IActionResult ParameterUp()
        {
            MoveParam();
            return PartialView("_ProductParametersBlock", _productsHelper.CurrentProduct);
        }

        public IActionResult ParameterDown()
        {
            MoveParam(false);
            return PartialView("_ProductParametersBlock", _productsHelper.CurrentProduct);
        }

        private void MoveParam(bool up = true)
        {
            if(_productsHelper.CurrentParameter == null) return;
            // выбранный параметр из контекста
            var contextParameter = _context.ProductParameters.SingleOrDefault(x => x.Id == _productsHelper.CurrentParameter.Id);
            if(contextParameter == null) return;

            // нужно переместить параметр вверх
            if (up)
            {
                // предыдущий параметр по Order(выбираем ближайший order, который меньше, чем в текущем параметре)
                var prevParam =
                    _productsHelper.CurrentProduct.LinkToProductParameters.Where(
                        x => x.Parameter.Order < contextParameter.Order).OrderByDescending(x => x.Parameter.Order).FirstOrDefault();
                if(prevParam == null) return;

                // Order куда надо переместить
                var destOrder = prevParam.Parameter.Order;

                // получаем предыдущий параметр из контекста
                var contextPrevParam =
                    _context.ProductParameters.SingleOrDefault(s => s.Id == prevParam.ProductParameterId);

                // перемещаем предыдущий параметр на место выбранного
                prevParam.Parameter.Order = contextParameter.Order;
                if (contextPrevParam != null) contextPrevParam.Order = contextParameter.Order;
                
                // перемещаем текущий переметр на место предыдущего
                contextParameter.Order = _productsHelper.CurrentParameter.Order = destOrder;
                _context.SaveChanges();
            }
            else
            {
                // следующий параметр по Order(выбираем ближайший order, который больше, чем в текущем параметре)
                var nextParam =
                    _productsHelper.CurrentProduct.LinkToProductParameters.Where(
                        x => x.Parameter.Order > contextParameter.Order).OrderBy(x => x.Parameter.Order).FirstOrDefault();
                if (nextParam == null) return;

                // Order куда надо переместить
                var destOrder = nextParam.Parameter.Order;

                // получаем следующий параметр из контекста
                var contextNextParam =
                    _context.ProductParameters.SingleOrDefault(s => s.Id == nextParam.ProductParameterId);

                // перемещаем следующий параметр на место выбранного
                nextParam.Parameter.Order = contextParameter.Order;
                if (contextNextParam != null) contextNextParam.Order = contextParameter.Order;

                // перемещаем текущий переметр на место предыдущего
                contextParameter.Order = _productsHelper.CurrentParameter.Order = destOrder;
                _context.SaveChanges();
            }
        }

        private async Task CheckParameterOrders(ProductParameter parameter)
        {
            // получаем запись LinkToProductParameters с нужным параметром
            var currentLink =
                _productsHelper.CurrentProduct.LinkToProductParameters.SingleOrDefault(
                    x => x.ProductParameterId == parameter.Id);

            // удаляем ее из списка чтобы вставить по нужному индексу
            _productsHelper.CurrentProduct.LinkToProductParameters.Remove(currentLink);

            // получаем индекс, на который надо поставить параметр
            var toIndex = parameter.Order > _productsHelper.CurrentProduct.LinkToProductParameters.Count
                ? _productsHelper.CurrentProduct.LinkToProductParameters.Count
                : parameter.Order;

            _productsHelper.CurrentProduct.LinkToProductParameters.Insert(toIndex, currentLink);

            // перестраиваем значение поля Order у параметров в соответствие с индексами
            _productsHelper.CurrentProduct.LinkToProductParameters.ForEach(l =>
            {
                // это влияет на текущее отображение
                l.Parameter.Order = _productsHelper.CurrentProduct.LinkToProductParameters.IndexOf(l);

                // это нужно, чтоб в БД сохранилось значение
                var contextParam = _context.ProductParameters.SingleOrDefault(x => x.Id == l.ProductParameterId);
                if(contextParam != null)
                    contextParam.Order = l.Parameter.Order;
            });
            await _context.SaveChangesAsync();
        }
    }
}

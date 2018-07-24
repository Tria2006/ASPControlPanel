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

        public IActionResult Create(Guid? groupId)
        {
            var groups = new List<ParameterGroup>
            {
                new ParameterGroup{Id = Guid.Empty, Name = "Не выбрано"}
            };
            groups.AddRange(_context.ParameterGroups.Where(x => !x.IsDeleted));
            ViewData["ParamGroups"] = new SelectList(groups, "Id", "Name", groupId ?? groups.First().Id);
            _productsHelper.IsParameterCreateInProgress = true;
            var tempParam = new ProductParameter
            {
                GroupId = groupId,
                LinkToProduct = new List<ProductLinkToProductParameter>
                {
                    new ProductLinkToProductParameter
                    {
                        Product = _productsHelper.CurrentProduct,
                        ProductId = _productsHelper.CurrentProduct.Id,
                    }
                }
            };
            _productsHelper.CurrentParameter = tempParam;
            return View(tempParam);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductParameter productParameter)
        {
            if (!ModelState.IsValid) return View(productParameter);
            if (productParameter.GroupId == Guid.Empty) productParameter.GroupId = null;
            _context.Add(productParameter);
            _context.SaveChanges();
            productParameter.LinkToProduct = new List<ProductLinkToProductParameter>{ new ProductLinkToProductParameter
            {
                ProductId = _productsHelper.CurrentProduct.Id,
                ProductParameterId = productParameter.Id
            }};
            await _context.SaveChangesAsync();
            var productId = _productsHelper.CurrentProduct.Id;
            _productsHelper.CurrentProduct = _context.Products.Include(x => x.LinkToProductParameters).ThenInclude(p => p.Parameter).SingleOrDefault(x => x.Id == productId);
            _productsHelper.IsParameterCreateInProgress = false;
            await CheckParameterOrders(productParameter);
            _productsHelper.CurrentParameter = null;
            return RedirectToAction(_productsHelper.IsProductCreateInProgress ? "CreateProduct" : "Edit", "Products", _productsHelper.CurrentProduct);
        }

        public IActionResult Edit(Guid id)
        {
            var productParameter =
                _productsHelper.CurrentProduct.LinkToProductParameters.SingleOrDefault(x => x.ProductParameterId == id);
            if (productParameter == null)
            {
                return NotFound();
            }
            var groups = new List<ParameterGroup>
            {
                new ParameterGroup{Id = Guid.Empty, Name = "Не выбрано"}
            };
            groups.AddRange(_context.ParameterGroups.Where(x => !x.IsDeleted));
            ViewData["ParamGroups"] = new SelectList(groups, "Id", "Name", groups.SingleOrDefault(x => x.Id == productParameter.Parameter.GroupId));
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
                _productsHelper.CurrentParameter = productParameter;
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

        public IActionResult ParameterUp(Guid? groupId)
        {
            MoveParam(groupId);
            return PartialView("_ProductParametersBlock", _productsHelper.CurrentProduct.LinkToProductParameters.Where(x => x.Parameter.GroupId == groupId).Select(s => s.Parameter).ToList());
        }

        public IActionResult ParameterDown(Guid? groupId)
        {
            MoveParam(groupId, false);
            return PartialView("_ProductParametersBlock", _productsHelper.CurrentProduct.LinkToProductParameters.Where(x => x.Parameter.GroupId == groupId).Select(s => s.Parameter).ToList());
        }

        private void MoveParam(Guid? groupId, bool up = true)
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
                        x => x.Parameter.GroupId == groupId && x.Parameter.Order < contextParameter.Order).OrderByDescending(x => x.Parameter.Order).FirstOrDefault();
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
                        x => x.Parameter.GroupId == groupId && x.Parameter.Order > contextParameter.Order).OrderBy(x => x.Parameter.Order).FirstOrDefault();
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
            List<ProductLinkToProductParameter> resultList;
            var paramsList =
                _productsHelper.CurrentProduct.LinkToProductParameters.Where(
                    x => x.Parameter.GroupId == parameter.GroupId).OrderBy(x => x.Parameter.Order).ToList();

            // получаем запись LinkToProductParameters с нужным параметром
            var currentLink =
                _productsHelper.CurrentProduct.LinkToProductParameters.SingleOrDefault(
                    x => x.ProductParameterId == parameter.Id);

            if(currentLink == null) return;

            // если добавляем в начало
            if (parameter.Order == 0)
            {
                paramsList.Remove(currentLink);
                resultList = new List<ProductLinkToProductParameter>{currentLink};
                resultList.AddRange(paramsList);

            } // если Order больше, чем максимальный, то добавляем в конец
            else if (parameter.Order >= paramsList.Max(x => x.Parameter.Order))
            {
                paramsList.Remove(currentLink);
                resultList = new List<ProductLinkToProductParameter>(paramsList) {currentLink};
            }
            else
            {
                var lower = paramsList.Where(x => x.Parameter.Order < parameter.Order); // Order меньше нужного
                var upper = paramsList.Where(x => x.Parameter.Order >= parameter.Order); // Order больше нужного

                // собираем результат: 1. все, кто с меньшим Order; 2. текущий параметр; 3. все, кто с бОльшим Order
                resultList = new List<ProductLinkToProductParameter>(lower) {currentLink};
                resultList.AddRange(upper);

            }

            // задаем Order каждому в списке
            var i = 0;
            foreach (var link in resultList)
            {
                var contextParam = _context.ProductParameters.SingleOrDefault(x => x.Id == link.ProductParameterId);
                link.Parameter.Order = i;
                if (contextParam != null) contextParam.Order = i;
                i++;
            }

            // удаляем все записи параметров для текущей группы
            _productsHelper.CurrentProduct.LinkToProductParameters.RemoveAll(
                x => x.Parameter.GroupId == parameter.GroupId);

            // добавляем resultList
            _productsHelper.CurrentProduct.LinkToProductParameters.AddRange(resultList);
            await _context.SaveChangesAsync();
        }
    }
}

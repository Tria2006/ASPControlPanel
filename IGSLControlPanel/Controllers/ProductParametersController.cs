﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DBModels.Models;
using DBModels.Models.ManyToManyLinks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IGSLControlPanel.Data;
using IGSLControlPanel.Helpers;
using log4net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace IGSLControlPanel.Controllers
{
    public class ProductParametersController : Controller
    {
        private readonly IGSLContext _context;
        private readonly ProductsHelper _productsHelper;
        private readonly EntityStateHelper _stateHelper;
        private readonly IHttpContextAccessor _httpAccessor;
        private readonly ILog logger;

        public ProductParametersController(IGSLContext context, ProductsHelper productsHelper, EntityStateHelper stateHelper, IHttpContextAccessor accessor)
        {
            _context = context;
            _httpAccessor = accessor;
            logger = LogManager.GetLogger(typeof(ProductsController));
            _productsHelper = productsHelper;
            _stateHelper = stateHelper;
        }

        public IActionResult Create(Guid? groupId)
        {
            var groups = new List<ParameterGroup>
            {
                new ParameterGroup{Id = Guid.Empty, Name = "Не выбрано"}
            };
            groups.AddRange(_context.ParameterGroups.Where(x => !x.IsDeleted));
            ViewData["ParamGroups"] = new SelectList(groups, "Id", "Name", groupId ?? groups.First().Id);
            var tempParam = new ProductParameter
            {
                GroupId = groupId,
                ValidFrom = DateTime.Today,
                ValidTo = new DateTime(2100, 1, 1)
            };
            if (_stateHelper.IsParameterCreateInProgress)
            {
                tempParam = _productsHelper.CurrentParameter;
            }
            else
            {
                _productsHelper.CurrentParameter = tempParam;
                _stateHelper.IsParameterCreateInProgress = true;
            }
            return View(tempParam);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductParameter productParameter)
        {
            if (!ModelState.IsValid) return View(productParameter);
            if (productParameter.GroupId == Guid.Empty) productParameter.GroupId = null;

            if (!_stateHelper.IsProductCreateInProgress)
            {
                _context.Add(productParameter);
                _context.SaveChanges();
                var link = new ProductLinkToProductParameter
                {
                    ProductId = _productsHelper.CurrentProduct.Id,
                    ProductParameterId = productParameter.Id,
                    Parameter = productParameter
                };
                productParameter.LinkToProduct = new List<ProductLinkToProductParameter> { link };
                await _context.SaveChangesAsync();
                var productId = _productsHelper.CurrentProduct.Id;
                _productsHelper.CurrentProduct = _context.Products.Include(x => x.LinkToProductParameters)
                    .ThenInclude(p => p.Parameter)
                    .SingleOrDefault(x => x.Id == productId);
                await CheckParameterOrders(productParameter);
                _productsHelper.CurrentParameter = null;
                _stateHelper.IsParameterCreateInProgress = false;
                logger.Info($"{_httpAccessor.HttpContext.Connection.RemoteIpAddress} created ProductParameter (id={productParameter.Id})");
            }
            else
            {
                // если попадаем сюда, то продукт еще не был сохранен и считаем, что добавление параметра не завершено
                _productsHelper.CurrentProduct.LinkToProductParameters.Add(new ProductLinkToProductParameter
                {
                    Product = _productsHelper.CurrentProduct,
                    Parameter = productParameter
                });
            }
            return RedirectToAction(_stateHelper.IsProductCreateInProgress ? "CreateProduct" : "Edit", "Products", _productsHelper.CurrentProduct);
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
                logger.Info($"{_httpAccessor.HttpContext.Connection.RemoteIpAddress} updated ProductParameter (id={productParameter.Id})");
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
            return RedirectToAction(_stateHelper.IsProductCreateInProgress ? "CreateProduct" : "Edit", "Products", _productsHelper.CurrentProduct);
        }

        public async Task<IActionResult> Delete(Guid id)
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
            var contextParameter = await _context.ProductParameters
                .FirstOrDefaultAsync(m => m.Id == id);
            if (contextParameter == null)
            {
                return NotFound();
            }

            return View(productParameter.Parameter);
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
            logger.Info($"{_httpAccessor.HttpContext.Connection.RemoteIpAddress} deleted(set IsDeleted=true) ProductParameter (id={id})");
            return RedirectToAction("Edit", "Products", _productsHelper.CurrentProduct);
        }

        private bool ProductParameterExists(Guid id)
        {
            return _context.ProductParameters.Any(e => e.Id == id);
        }

        public IActionResult ParameterUp(Guid? groupId)
        {
            MoveParam(groupId ?? Guid.Empty);
            return PartialView("_ProductParametersBlock", _productsHelper.CurrentProduct.LinkToProductParameters.Where(x => groupId != null ? x.Parameter.GroupId == groupId : x.Parameter.GroupId == Guid.Empty).Select(s => s.Parameter).ToList());
        }

        public IActionResult ParameterDown(Guid? groupId)
        {
            MoveParam(groupId ?? Guid.Empty, false);
            return PartialView("_ProductParametersBlock", _productsHelper.CurrentProduct.LinkToProductParameters.Where(x => groupId != null ? x.Parameter.GroupId == groupId : x.Parameter.GroupId == Guid.Empty).Select(s => s.Parameter).ToList());
        }

        private void MoveParam(Guid groupId, bool up = true)
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
            if (parameter.Order == 1)
            {
                resultList = new List<ProductLinkToProductParameter>{currentLink};
                resultList.AddRange(paramsList.Where(x => x.ProductParameterId != parameter.Id));

            } // если Order больше, чем максимальный, то добавляем в конец
            else if (parameter.Order >= paramsList.Max(x => x.Parameter.Order))
            {
                resultList = new List<ProductLinkToProductParameter>(paramsList.Where(x => x.ProductParameterId != parameter.Id)) {currentLink};
            }
            else
            {
                resultList = paramsList.Where(x => x.ProductParameterId != parameter.Id).ToList();
                resultList.Insert(parameter.Order - 1, currentLink);
            }

            // задаем Order каждому в списке
            var i = 1;
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

        public bool ProductCheckBoxClick(Guid id)
        {
            _productsHelper.CheckProduct(id, _context);
            return _productsHelper.HasSelectedProducts;
        }

        // Нужно сохранить значения полей параметра если он еще не был сохранен, иначе при возвращении обратно 
        // на экран создания нового параметра все данные очистятся
        public void SaveTempData(int dataType, string name, 
            DateTime? dateFrom, DateTime? dateTo, 
            bool requiredForSave, bool requiredForCalc,
            Guid? groupId, int order)
        {
            _productsHelper.CurrentParameter.Name = name;
            _productsHelper.CurrentParameter.ValidFrom = dateFrom;
            _productsHelper.CurrentParameter.ValidTo = dateTo;
            _productsHelper.CurrentParameter.DataType = dataType;
            _productsHelper.CurrentParameter.IsRequiredForCalc = requiredForCalc;
            _productsHelper.CurrentParameter.IsRequiredForSave = requiredForSave;
            _productsHelper.CurrentParameter.GroupId = groupId;
            _productsHelper.CurrentParameter.Order = order;
        }

        public IActionResult GoBack()
        {
            return RedirectToAction(_stateHelper.IsProductCreateInProgress ? "Create" : "Edit", "Products", _productsHelper.CurrentProduct);
        }
    }
}

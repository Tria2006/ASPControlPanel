using DBModels.Models;
using DBModels.Models.ManyToManyLinks;
using IGSLControlPanel.Data;
using IGSLControlPanel.Helpers;
using log4net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IGSLControlPanel.Controllers
{
    public class ProductParametersController : Controller
    {
        private readonly IGSLContext _context;
        private readonly ProductsHelper _productsHelper;
        private readonly IHttpContextAccessor _httpAccessor;
        private readonly ILog _logger;

        public ProductParametersController(IGSLContext context, ProductsHelper productsHelper, IHttpContextAccessor accessor)
        {
            _context = context;
            _httpAccessor = accessor;
            _logger = LogManager.GetLogger(typeof(ProductParametersController));
            _productsHelper = productsHelper;
        }

        public async Task<IActionResult> Create(Guid? groupId)
        {
            var groups = new List<ParameterGroup>
            {
                new ParameterGroup{Id = Guid.Empty, Name = "Не выбрано"}
            };
            groups.AddRange(_context.ParameterGroups.Where(x => !x.IsDeleted));
            ViewData["ParamGroups"] = new SelectList(groups, "Id", "Name", groupId ?? groups.First().Id);
            // пытаемся получить данные группы
            var group = await GetGroupById(groupId);
            var tempParam = new ProductParameter
            {
                GroupId = groupId,
                ValidFrom = DateTime.Today,
                ValidTo = new DateTime(2100, 1, 1)
            };
            ViewData["IsSelectGroupDisabled"] = group?.IsGlobal ?? false;
            _productsHelper.CurrentParameter = tempParam;
            return View(tempParam);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductParameter productParameter, string create, string createAndExit)
        {
            // пытаемся получить данные группы
            var group = await GetGroupById(productParameter.GroupId);

            if (!ModelState.IsValid)
            {
                // если данные некорректны, то при возвращении надо выставить выбранную группу
                var groups = new List<ParameterGroup>
                {
                    new ParameterGroup{Id = Guid.Empty, Name = "Не выбрано"}
                };
                groups.AddRange(_context.ParameterGroups.Where(x => !x.IsDeleted));
                ViewData["ParamGroups"] = new SelectList(groups, "Id", "Name", group?.Id ?? groups.First().Id);
                ViewData["IsSelectGroupDisabled"] = group?.IsGlobal ?? false;
                _productsHelper.CurrentParameter = productParameter;
                return View(productParameter);
            }
            if (productParameter.GroupId == Guid.Empty) productParameter.GroupId = null;

            _context.Add(productParameter);
            _context.SaveChanges();
            productParameter.Limit = _productsHelper.CurrentParameter.Limit;
            if (productParameter.Limit != null)
            {
                productParameter.Limit.ParameterId = productParameter.Id;
                productParameter.Limit.ProductId = _productsHelper.CurrentProduct.Id;
            }

            if (group == null || !group.IsGlobal)
            {
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
                _productsHelper.LoadProductLimits(_productsHelper.CurrentProduct, _context);
            }
            else
            {
                productParameter.LinkToProduct = new List<ProductLinkToProductParameter> ();
                await _context.SaveChangesAsync();
            }
            _productsHelper.CurrentParameter = null;
            _logger.Info($"{_httpAccessor.HttpContext.Connection.RemoteIpAddress} created ProductParameter (id={productParameter.Id})");

            if (group != null && group.IsGlobal)
            {
                if (!string.IsNullOrEmpty(createAndExit))
                    return RedirectToAction("Index", "ParameterGroups");
                return RedirectToAction("Edit", new { productParameter.Id });
            }

            if (!string.IsNullOrEmpty(createAndExit))
                return RedirectToAction("Edit", "Products", _productsHelper.CurrentProduct);
            return RedirectToAction("Edit", new { productParameter.Id });
        }

        public async Task<IActionResult> Edit(Guid id)
        {
            var productParameter = _context.ProductParameters.Find(id);
            if (productParameter == null)
            {
                return NotFound();
            }
            // пытаемся получить данные группы
            var group = await GetGroupById(productParameter.GroupId);
            var groups = new List<ParameterGroup>
            {
                new ParameterGroup{Id = Guid.Empty, Name = "Не выбрано"}
            };
            groups.AddRange(_context.ParameterGroups.Where(x => !x.IsDeleted));
            ViewData["ParamGroups"] = new SelectList(groups, "Id", "Name", groups.SingleOrDefault(x => x.Id == productParameter.GroupId));
            _productsHelper.CurrentParameter = productParameter;
            ViewData["IsSelectGroupDisabled"] = group?.IsGlobal ?? false;
            return View(productParameter);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, ProductParameter productParameter, string save, string saveAndExit)
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
                _logger.Info($"{_httpAccessor.HttpContext.Connection.RemoteIpAddress} updated ProductParameter (id={productParameter.Id})");
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
            // пытаемся получить данные группы
            var group = await GetGroupById(productParameter.GroupId);
            if (group != null && !group.IsGlobal)
            {
                await CheckParameterOrders(productParameter);
            }
            _productsHelper.CurrentParameter = null;

            if (group != null && group.IsGlobal)
            {
                if (!string.IsNullOrEmpty(saveAndExit))
                    return RedirectToAction("Index", "ParameterGroups");
                return RedirectToAction("Edit", new { productParameter.Id });
            }

            if (!string.IsNullOrEmpty(saveAndExit))
                return RedirectToAction("Edit", "Products",
                    _productsHelper.CurrentProduct);
            return RedirectToAction("Edit", new { id });
        }

        public async Task<IActionResult> Delete(Guid id)
        {
            var productParameter = await _context.ProductParameters
                .FirstOrDefaultAsync(m => m.Id == id);
            if (productParameter == null)
            {
                return NotFound();
            }
            var groups = new List<ParameterGroup>
            {
                new ParameterGroup{Id = Guid.Empty, Name = "Не выбрано"}
            };
            groups.AddRange(_context.ParameterGroups.Where(x => !x.IsDeleted));
            ViewData["ParamGroups"] = new SelectList(groups, "Id", "Name", groups.SingleOrDefault(x => x.Id == productParameter.GroupId));
            _productsHelper.CurrentParameter = productParameter;

            return View(productParameter);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var productParameter = _context.ProductParameters.Include(x => x.LinkToProduct).ThenInclude(p => p.Product).SingleOrDefault(x => x.Id == id);
            // пытаемся получить данные группы
            var group = await GetGroupById(productParameter?.GroupId);
            var isGlobal = group != null && group.IsGlobal;

            if (productParameter == null)
            {
                if (!isGlobal)
                {
                    return RedirectToAction("Edit", "Products", _productsHelper.CurrentProduct);
                }

                return RedirectToAction("EditGlobal", "ParameterGroups", new { group.Id });
            }
            productParameter.IsDeleted = true;

            if (!isGlobal)
            {
                var link = productParameter.LinkToProduct.SingleOrDefault(
                    p => p.ProductParameterId == id && p.ProductId == _productsHelper.CurrentProduct.Id);
                if (link != null)
                {
                    productParameter.LinkToProduct.Remove(link);
                }
            }
            await _context.SaveChangesAsync();
            _logger.Info($"{_httpAccessor.HttpContext.Connection.RemoteIpAddress} deleted(set IsDeleted=true) ProductParameter (id={id})");

            if (!isGlobal) return RedirectToAction("Edit", "Products", _productsHelper.CurrentProduct);
            return RedirectToAction("EditGlobal", "ParameterGroups", new {group.Id});
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
            if (_productsHelper.CurrentParameter == null) return;
            // выбранный параметр из контекста
            var contextParameter = _context.ProductParameters.SingleOrDefault(x => x.Id == _productsHelper.CurrentParameter.Id);
            if (contextParameter == null) return;

            // нужно переместить параметр вверх
            if (up)
            {
                // предыдущий параметр по Order(выбираем ближайший order, который меньше, чем в текущем параметре)
                var prevParam =
                    _productsHelper.CurrentProduct.LinkToProductParameters.Where(
                        x => x.Parameter.GroupId == groupId && x.Parameter.Order < contextParameter.Order).OrderByDescending(x => x.Parameter.Order).FirstOrDefault();
                if (prevParam == null) return;

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

            if (currentLink == null) return;

            // если добавляем в начало
            if (parameter.Order == 1)
            {
                resultList = new List<ProductLinkToProductParameter> { currentLink };
                resultList.AddRange(paramsList.Where(x => x.ProductParameterId != parameter.Id));

            } // если Order больше, чем максимальный, то добавляем в конец
            else if (parameter.Order >= paramsList.Max(x => x.Parameter.Order))
            {
                resultList = new List<ProductLinkToProductParameter>(paramsList.Where(x => x.ProductParameterId != parameter.Id)) { currentLink };
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

        public async Task<IActionResult> GoBack(Guid parameterId)
        {
            var productParameter = _context.ProductParameters.Find(parameterId);
            if (productParameter == null)
            {
                return NotFound();
            }
            // пытаемся получить данные группы
            var group = await GetGroupById(productParameter.GroupId);
            if (group == null || !group.IsGlobal)
            {
                return RedirectToAction("Edit", "Products", _productsHelper.CurrentProduct);
            }

            return RedirectToAction("EditGlobal", "ParameterGroups", new {id = group.Id});
        }

        private async Task<ParameterGroup> GetGroupById(Guid? id)
        {
            if (id != null)
            {
                return await _context.ParameterGroups.FindAsync(id);
            }

            return null;
        }
    }
}

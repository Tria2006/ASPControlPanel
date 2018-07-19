using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IGSLControlPanel.Data;
using IGSLControlPanel.Helpers;
using IGSLControlPanel.Models;
using IGSLControlPanel.Models.ManyToManyLinks;

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
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,IsDeleted,IsRequiredForCalc,IsRequiredForSave,DataType")] ProductParameter productParameter)
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
            return RedirectToAction(_productsHelper.IsCreateInProgress ? "CreateProduct" : "Edit", "Products", _productsHelper.CurrentProduct);
        }

        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var productParameter = await _context.ProductParameters.FindAsync(id);
            if (productParameter == null)
            {
                return NotFound();
            }
            return View(productParameter);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,Name,IsDeleted,IsRequiredForCalc,IsRequiredForSave,DataType")] ProductParameter productParameter)
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
            return RedirectToAction(_productsHelper.IsCreateInProgress ? "CreateProduct" : "Edit", "Products", _productsHelper.CurrentProduct);
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
            if(_productsHelper.SelectedParameter == null) return;
            // выбранный параметр из контекста
            var contextParameter = _context.ProductParameters.SingleOrDefault(x => x.Id == _productsHelper.SelectedParameter.Id);
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
                contextParameter.Order = _productsHelper.SelectedParameter.Order = destOrder;
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
                contextParameter.Order = _productsHelper.SelectedParameter.Order = destOrder;
                _context.SaveChanges();
            }
        }
    }
}

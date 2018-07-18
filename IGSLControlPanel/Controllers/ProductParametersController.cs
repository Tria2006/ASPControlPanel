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
        public async Task<IActionResult> Create([Bind("Id,Name,IsDeleted,IsRequiredForCalc,IsRequiredForSave")] ProductParameter productParameter)
        {
            if (!ModelState.IsValid) return View(productParameter);
            _context.Add(productParameter);
            _context.SaveChanges();
            productParameter.LinkToProduct = new List<ProductLinkToProductParameter>{ new ProductLinkToProductParameter
            {
                ProductId = _productsHelper.TempProduct.Id,
                ProductParameterId = productParameter.Id
            }};
            await _context.SaveChangesAsync();
            return RedirectToAction(_productsHelper.IsCreateInProgress ? "CreateProduct" : "Edit", "Products", _productsHelper.TempProduct);
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
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,Name,IsDeleted,IsRequiredForCalc,IsRequiredForSave")] ProductParameter productParameter)
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
            return RedirectToAction(_productsHelper.IsCreateInProgress ? "CreateProduct" : "Edit", "Products", _productsHelper.TempProduct);
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
            var productParameter = await _context.ProductParameters.FindAsync(id);
            productParameter.IsDeleted = true;
            var link = productParameter.LinkToProduct.SingleOrDefault(
                p => p.ProductParameterId == id && p.ProductId == _productsHelper.TempProduct.Id);
            if (link != null)
            {
                productParameter.LinkToProduct.Remove(link);
            }
            await _context.SaveChangesAsync();
            return RedirectToAction("Edit", "Products", _productsHelper.TempProduct);
        }

        private bool ProductParameterExists(Guid id)
        {
            return _context.ProductParameters.Any(e => e.Id == id);
        }

        public IActionResult ReturnToEditProduct()
        {
            return RedirectToAction("Edit", "Products", _productsHelper.TempProduct);
        }
    }
}

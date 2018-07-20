using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IGSLControlPanel.Data;
using IGSLControlPanel.Helpers;
using IGSLControlPanel.Models;

namespace IGSLControlPanel.Controllers
{
    public class ValueLimitsController : Controller
    {
        private readonly IGSLContext _context;
        private readonly ProductsHelper _productsHelper;

        public ValueLimitsController(IGSLContext context, ProductsHelper productsHelper)
        {
            _context = context;
            _productsHelper = productsHelper;
        }

        public IActionResult Create()
        {
            var tempLimit = new ValueLimit
            {
                ProductId = _productsHelper.CurrentProduct.Id,
                ParameterId = _productsHelper.CurrentParameter.Id,
                ParameterDataType = _productsHelper.CurrentParameter.DataType
            };
            return View(tempLimit);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,ParameterDataType,IntValueFrom,IntValueTo,DateValueFrom,DateValueTo,IsDeleted,ParameterId,ProductId")] ValueLimit valueLimit)
        {
            if (!ModelState.IsValid) return View(valueLimit);
            valueLimit.ParameterDataType = _productsHelper.CurrentParameter.DataType;
            _productsHelper.CurrentParameter.Limit = valueLimit;
            _context.Add(valueLimit);
            await _context.SaveChangesAsync();
            return RedirectToAction(_productsHelper.IsParameterCreateInProgress ? "Create" : "Edit", "ProductParameters", _productsHelper.CurrentParameter);
        }

        public IActionResult Edit(Guid id)
        {
            return View(_productsHelper.CurrentParameter.Limit);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,Name,ParameterDataType,IntValueFrom,IntValueTo,DateValueFrom,DateValueTo,IsDeleted,ParameterId,ProductId")] ValueLimit valueLimit)
        {
            if (id != valueLimit.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid) return View(valueLimit);
            try
            {
                valueLimit.ParameterDataType = _productsHelper.CurrentParameter.DataType;
                _context.Update(valueLimit);
                _productsHelper.CurrentParameter.Limit = valueLimit;
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ValueLimitExists(valueLimit.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(_productsHelper.IsParameterCreateInProgress ? "Create" : "Edit", "ProductParameters", _productsHelper.CurrentParameter);
        }

        public IActionResult Delete(Guid id)
        {
            return View(_productsHelper.CurrentParameter.Limit);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var valueLimit = await _context.ValueLimits.FindAsync(id);
            valueLimit.IsDeleted = true;
            _productsHelper.CurrentParameter.Limit = null;
            await _context.SaveChangesAsync();
            return RedirectToAction(_productsHelper.IsParameterCreateInProgress ? "Create" : "Edit", "ProductParameters", _productsHelper.CurrentParameter);
        }

        private bool ValueLimitExists(Guid id)
        {
            return _context.ValueLimits.Any(e => e.Id == id);
        }
    }
}

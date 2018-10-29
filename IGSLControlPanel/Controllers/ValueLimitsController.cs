using System;
using System.Linq;
using System.Threading.Tasks;
using DBModels.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IGSLControlPanel.Data;
using IGSLControlPanel.Helpers;
using log4net;
using Microsoft.AspNetCore.Http;

namespace IGSLControlPanel.Controllers
{
    public class ValueLimitsController : Controller
    {
        private readonly IGSLContext _context;
        private readonly ProductsHelper _productsHelper;
        private readonly IHttpContextAccessor _httpAccessor;
        private readonly ILog logger;

        public ValueLimitsController(IGSLContext context, ProductsHelper productsHelper, IHttpContextAccessor accessor)
        {
            _context = context;
            _httpAccessor = accessor;
            logger = LogManager.GetLogger(typeof(ValueLimitsController));
            _productsHelper = productsHelper;
        }

        public IActionResult Create()
        {
            var limit = new ValueLimit
            {
                ProductId = _productsHelper.CurrentProduct.Id,
                ParameterId = _productsHelper.CurrentParameter.Id,
                ParameterDataType = _productsHelper.CurrentParameter.DataType,
                ValidFrom = DateTime.Today,
                ValidTo = new DateTime(2100, 1, 1)
            };
                _productsHelper.CurrentParameter.Limit = limit;
                _productsHelper.LimitWOChanges = null;

            return View(limit);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ValueLimit valueLimit, string create, string createAndExit)
        {
            if (!ModelState.IsValid) return View(valueLimit);
            valueLimit.ParameterDataType = _productsHelper.CurrentParameter.DataType;
            _productsHelper.CurrentParameter.Limit = valueLimit;
            var contextParam = await _context.ProductParameters.FindAsync(_productsHelper.CurrentParameter.Id);
            contextParam.Limit = valueLimit;

            _context.Add(valueLimit);
            await _context.SaveChangesAsync();
            logger.Info($"{_httpAccessor.HttpContext.Connection.RemoteIpAddress} created ValueLimit (id={valueLimit.Id})");
            if (!string.IsNullOrEmpty(createAndExit))
                return RedirectToAction("Edit", "ProductParameters", _productsHelper.CurrentParameter);
            return RedirectToAction("Edit", new { valueLimit.Id });
        }

        public IActionResult Edit(Guid id)
        {
            var contextLimit = _context.ValueLimits.Include(x => x.LimitListItems).SingleOrDefault(x => x.Id == id);
            return View(contextLimit);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, ValueLimit valueLimit, string save, string saveAndExit)
        {
            if (!ModelState.IsValid) return View(valueLimit);
            try
            {
                valueLimit.ParameterDataType = _productsHelper.CurrentParameter.DataType;
                valueLimit.LimitListItems = _productsHelper.CurrentParameter.Limit.LimitListItems;
                _context.Update(valueLimit);
                _productsHelper.CurrentParameter.Limit = valueLimit;
                await _context.SaveChangesAsync();
                logger.Info($"{_httpAccessor.HttpContext.Connection.RemoteIpAddress} updated ValueLimit (id={valueLimit.Id})");
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
            if (!string.IsNullOrEmpty(saveAndExit))
                return RedirectToAction("Edit", "ProductParameters", new{ _productsHelper.CurrentParameter.Id });
            return RedirectToAction("Edit", new { id });
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
            var contextParam = await _context.ProductParameters.FindAsync(_productsHelper.CurrentParameter.Id);
            contextParam.Limit = null;
            await _context.SaveChangesAsync();
            logger.Info($"{_httpAccessor.HttpContext.Connection.RemoteIpAddress} deleted(set IsDeleted=true) ValueLimit (id={id})");
            return RedirectToAction("Edit", "ProductParameters", _productsHelper.CurrentParameter);
        }

        private bool ValueLimitExists(Guid id)
        {
            return _context.ValueLimits.Any(e => e.Id == id);
        }

        public void SaveTempData(string name, int dataType, DateTime? dateMin, DateTime? dateMax, int? intMin, int? intMax, string strValue)
        {
            _productsHelper.CurrentParameter.Limit.Name = name;
            _productsHelper.CurrentParameter.Limit.ParameterDataType = dataType;
            _productsHelper.CurrentParameter.Limit.DateValueFrom = dateMin;
            _productsHelper.CurrentParameter.Limit.DateValueTo = dateMax;
            _productsHelper.CurrentParameter.Limit.IntValueFrom = intMin;
            _productsHelper.CurrentParameter.Limit.IntValueTo = intMax;
            _productsHelper.CurrentParameter.Limit.StringValue = strValue;
        }

        public IActionResult GoBack()
        {
            //_productsHelper.CurrentParameter.Limit = _productsHelper.LimitWOChanges;
            return RedirectToAction("Edit", "ProductParameters", _productsHelper.CurrentParameter);
        }
    }
}

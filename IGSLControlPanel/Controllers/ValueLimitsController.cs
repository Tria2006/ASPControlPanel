using DBModels.Models;
using IGSLControlPanel.Data;
using IGSLControlPanel.Helpers;
using log4net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace IGSLControlPanel.Controllers
{
    public class ValueLimitsController : Controller
    {
        private readonly IGSLContext _context;
        private readonly ProductsHelper _productsHelper;
        private readonly IHttpContextAccessor _httpAccessor;
        private readonly ILog _logger;

        public ValueLimitsController(IGSLContext context, ProductsHelper productsHelper, IHttpContextAccessor accessor)
        {
            _context = context;
            _httpAccessor = accessor;
            _logger = LogManager.GetLogger(typeof(ValueLimitsController));
            _productsHelper = productsHelper;
        }

        public IActionResult Create(Guid parameterId, bool returnToGroupEdit = false)
        {
            ViewData["ReturnToGroupEdit"] = returnToGroupEdit;
            var param = _context.ProductParameters.Find(parameterId);
            if (param == null) return NotFound();

            var limit = new ValueLimit
            {
                ParameterId = parameterId,
                ParameterDataType = param.DataType,
                ValidFrom = DateTime.Today,
                ValidTo = new DateTime(2100, 1, 1)
            };
            if (_productsHelper.CurrentProduct != null)
            {
                limit.ProductId = _productsHelper.CurrentProduct.Id;
            }

            if (_productsHelper.CurrentParameter != null)
            {
                _productsHelper.CurrentParameter.Limit = limit;
            }
            _productsHelper.LimitWOChanges = null;

            return View(limit);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ValueLimit valueLimit, string create, string createAndExit, bool returnToGroupEdit)
        {
            if (!ModelState.IsValid) return View(valueLimit);
            if (_productsHelper.CurrentParameter != null)
                _productsHelper.CurrentParameter.Limit = valueLimit;
            var contextParam = await _context.ProductParameters.FindAsync(valueLimit.ParameterId);
            contextParam.Limit = valueLimit;
            _context.Add(valueLimit);
            await _context.SaveChangesAsync();

            _logger.Info($"{_httpAccessor.HttpContext.Connection.RemoteIpAddress} created ValueLimit (id={valueLimit.Id})");

            if (!string.IsNullOrEmpty(createAndExit))
                return RedirectToAction("Edit", "ProductParameters", new{id = valueLimit.ParameterId, returnToGroupEdit});
            return RedirectToAction("Edit", new { valueLimit.Id });
        }

        public IActionResult Edit(Guid id, bool returnToGroupEdit = false)
        {
            ViewData["ReturnToGroupEdit"] = returnToGroupEdit;
            var contextLimit = _context.ValueLimits.Include(x => x.LimitListItems).SingleOrDefault(x => x.Id == id);
            return View(contextLimit);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, ValueLimit valueLimit, string save, string saveAndExit, bool returnToGroupEdit)
        {
            if (!ModelState.IsValid) return View(valueLimit);
            try
            {
                valueLimit.ParameterDataType = _productsHelper.CurrentParameter.DataType;
                valueLimit.LimitListItems = _productsHelper.CurrentParameter.Limit.LimitListItems;
                _context.Update(valueLimit);
                _productsHelper.CurrentParameter.Limit = valueLimit;
                await _context.SaveChangesAsync();
                _logger.Info($"{_httpAccessor.HttpContext.Connection.RemoteIpAddress} updated ValueLimit (id={valueLimit.Id})");
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
                return RedirectToAction("Edit", "ProductParameters", new { id = _productsHelper.CurrentParameter.Id, returnToGroupEdit });
            return RedirectToAction("Edit", new { id, returnToGroupEdit });
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
            _logger.Info($"{_httpAccessor.HttpContext.Connection.RemoteIpAddress} deleted(set IsDeleted=true) ValueLimit (id={id})");
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

        public IActionResult GoBack(Guid parameterId, bool returnToGroupEdit)
        {
            return RedirectToAction("Edit", "ProductParameters", new {id = parameterId, returnToGroupEdit});
        }
    }
}

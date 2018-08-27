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
        private readonly EntityStateHelper _stateHelper;
        private readonly IHttpContextAccessor _httpAccessor;
        private readonly ILog logger;

        public ValueLimitsController(IGSLContext context, ProductsHelper productsHelper, EntityStateHelper stateHelper, IHttpContextAccessor accessor)
        {
            _context = context;
            _httpAccessor = accessor;
            logger = LogManager.GetLogger(typeof(ValueLimitsController));
            _productsHelper = productsHelper;
            _stateHelper = stateHelper;
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
            if (_stateHelper.IsValueLimitCreateInProgress)
            {
                limit = _productsHelper.CurrentParameter.Limit;
            }
            else
            {
                _productsHelper.CurrentParameter.Limit = limit;
                _stateHelper.LimitWOChanges = null;
                _stateHelper.IsValueLimitCreateInProgress = true;
            }

            return View(limit);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ValueLimit valueLimit)
        {
            if (!ModelState.IsValid) return View(valueLimit);
            valueLimit.ParameterDataType = _productsHelper.CurrentParameter.DataType;
            if (_stateHelper.IsValueLimitCreateInProgress)
            {
                valueLimit.LimitListItems = _productsHelper.CurrentParameter.Limit.LimitListItems;
                foreach (var item in valueLimit.LimitListItems)
                {
                    item.ValueLimitId = valueLimit.Id;
                }
            }
            _productsHelper.CurrentParameter.Limit = valueLimit;

            if (!_stateHelper.IsParameterCreateInProgress)
            {
                _context.Add(valueLimit);
                await _context.SaveChangesAsync();
                _stateHelper.IsValueLimitCreateInProgress = false;
            }
            logger.Info($"{_httpAccessor.HttpContext.Connection.RemoteIpAddress} created ValueLimit (id={valueLimit.Id})");
            return RedirectToAction(_stateHelper.IsParameterCreateInProgress ? "Create" : "Edit", "ProductParameters", _productsHelper.CurrentParameter);
        }

        public IActionResult Edit(Guid id)
        {
            _stateHelper.LimitWOChanges = _productsHelper.CurrentParameter.Limit;
            return View(_productsHelper.CurrentParameter.Limit);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, ValueLimit valueLimit)
        {
            if (id != valueLimit.Id)
            {
                return NotFound();
            }

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
            return RedirectToAction(_stateHelper.IsParameterCreateInProgress ? "Create" : "Edit", "ProductParameters", _productsHelper.CurrentParameter);
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
            logger.Info($"{_httpAccessor.HttpContext.Connection.RemoteIpAddress} deleted(set IsDeleted=true) ValueLimit (id={id})");
            return RedirectToAction(_stateHelper.IsParameterCreateInProgress ? "Create" : "Edit", "ProductParameters", _productsHelper.CurrentParameter);
        }

        private bool ValueLimitExists(Guid id)
        {
            return _context.ValueLimits.Any(e => e.Id == id);
        }

        public void SaveTempData(string name, int dataType, DateTime? dateMin, DateTime? dateMax, int? intMin, int? intMax)
        {
            _productsHelper.CurrentParameter.Limit.Name = name;
            _productsHelper.CurrentParameter.Limit.ParameterDataType = dataType;
            _productsHelper.CurrentParameter.Limit.DateValueFrom = dateMin;
            _productsHelper.CurrentParameter.Limit.DateValueTo = dateMax;
            _productsHelper.CurrentParameter.Limit.IntValueFrom = intMin;
            _productsHelper.CurrentParameter.Limit.IntValueTo = intMax;
        }

        public IActionResult GoBack()
        {
            _productsHelper.CurrentParameter.Limit = _stateHelper.LimitWOChanges;
            return RedirectToAction(_stateHelper.IsParameterCreateInProgress ? "Create" : "Edit", "ProductParameters", _productsHelper.CurrentParameter);
        }
    }
}

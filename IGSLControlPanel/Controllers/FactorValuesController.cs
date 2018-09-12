using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DBModels.Models;
using IGSLControlPanel.Data;
using IGSLControlPanel.Helpers;
using log4net;
using Microsoft.AspNetCore.Http;

namespace IGSLControlPanel.Controllers
{
    public class FactorValuesController : Controller
    {
        private readonly IGSLContext _context;
        private readonly RiskFactorHelper _factorHelper;
        private readonly TariffsHelper _tariffsHelper;
        private readonly IHttpContextAccessor _httpAccessor;
        private readonly ILog _logger;

        public FactorValuesController(IGSLContext context, RiskFactorHelper factorHelper, 
            TariffsHelper tariffsHelper, IHttpContextAccessor accessor)
        {
            _context = context;
            _logger = LogManager.GetLogger(typeof(FactorValuesController));
            _factorHelper = factorHelper;
            _tariffsHelper = tariffsHelper;
            _httpAccessor = accessor;
        }

        public IActionResult Create()
        {
            var tempItem = new FactorValue
            {
                RiskFactorId = _factorHelper.CurrentFactor.Id,
                TariffId = _tariffsHelper.CurrentTariff.Id,
                ValidFrom = DateTime.Now,
                ValidTo = new DateTime(2100,1,1)
            };
            return View(tempItem);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FactorValue factorValue, string create, string createAndExit)
        {
            if (!ModelState.IsValid) return View(factorValue);
            _context.Add(factorValue);
            await _context.SaveChangesAsync();
            _logger.Info($"{_httpAccessor.HttpContext.Connection.RemoteIpAddress} created FactorValue (id={factorValue.Id})");
            if (!string.IsNullOrEmpty(createAndExit))
                return RedirectToAction("Edit", "RiskFactors", _factorHelper.CurrentFactor);
            return RedirectToAction("Edit", new { factorValue.Id });
        }

        public async Task<IActionResult> Edit(Guid id)
        {
            var factorValue = await _context.FactorValues.FindAsync(id);
            if (factorValue == null)
            {
                return NotFound();
            }
            return View(factorValue);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, FactorValue factorValue, string save, string saveAndExit)
        {
            if (id != factorValue.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid) return View(factorValue);
            try
            {
                _context.Update(factorValue);
                await _context.SaveChangesAsync();
                _logger.Info($"{_httpAccessor.HttpContext.Connection.RemoteIpAddress} updated FactorValue (id={factorValue.Id})");
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FactorValueExists(factorValue.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            if (!string.IsNullOrEmpty(saveAndExit))
                return RedirectToAction("Edit", "RiskFactors", _factorHelper.CurrentFactor);
            return RedirectToAction("Edit", new { factorValue.Id });
        }

        public async Task<IActionResult> Delete(Guid id)
        {
            var factorValue = await _context.FactorValues
                .FirstOrDefaultAsync(m => m.Id == id);
            if (factorValue == null)
            {
                return NotFound();
            }

            return View(factorValue);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var factorValue = await _context.FactorValues.FindAsync(id);
            factorValue.IsDeleted = true;
            await _context.SaveChangesAsync();
            return RedirectToAction("Edit", "RiskFactors", _factorHelper.CurrentFactor);
        }

        private bool FactorValueExists(Guid id)
        {
            return _context.FactorValues.Any(e => e.Id == id);
        }

        public IActionResult GoBack()
        {
            return RedirectToAction("Edit", "RiskFactors", _factorHelper.CurrentFactor);
        }
    }
}

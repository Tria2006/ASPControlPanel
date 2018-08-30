using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DBModels.Models;
using DBModels.Models.ManyToManyLinks;
using IGSLControlPanel.Data;
using IGSLControlPanel.Helpers;
using log4net;
using Microsoft.AspNetCore.Http;

namespace IGSLControlPanel.Controllers
{
    public class RiskFactorsController : Controller
    {
        private readonly IGSLContext _context;
        private readonly TariffsHelper _tariffsHelper;
        private readonly RiskFactorHelper _factorHelper;
        private readonly IHttpContextAccessor _httpAccessor;
        private readonly ILog logger;

        public RiskFactorsController(IGSLContext context, RiskFactorHelper factorHelper, TariffsHelper tariffsHelper,
            IHttpContextAccessor accessor)
        {
            _context = context;
            _httpAccessor = accessor;
            logger = LogManager.GetLogger(typeof(RiskFactorsController));
            _tariffsHelper = tariffsHelper;
            _factorHelper = factorHelper;
        }

        public IActionResult Create()
        {
            var tempFactor = new RiskFactor
            {
                ValidFrom = DateTime.Today,
                ValidTo = new DateTime(2100, 1, 1)
            };
            _factorHelper.CurrentFactor = tempFactor;
            return View(tempFactor);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RiskFactor riskFactor, string create, string createAndExit)
        {
            if (!ModelState.IsValid) return View(riskFactor);
                _context.Add(riskFactor);
                await _context.SaveChangesAsync();
                riskFactor.RiskFactorsTariffLinks.Add(new RiskFactorTariffLink
                {
                    TariffId = _tariffsHelper.CurrentTariff.Id,
                    RiskFactorId = riskFactor.Id,
                    RiskFactor = riskFactor
                });
                await _context.SaveChangesAsync();
                logger.Info($"{_httpAccessor.HttpContext.Connection.RemoteIpAddress} created RiskFactor (id={riskFactor.Id})");
            if (!string.IsNullOrEmpty(createAndExit))
                return RedirectToAction("Edit", "Tariffs", _tariffsHelper.CurrentTariff);
            return RedirectToAction("Edit", new { riskFactor.Id });
        }

        public IActionResult Edit(Guid id)
        {
            var riskFactor = _context.RiskFactors
                .Include(x => x.RiskFactorsTariffLinks)
                .Include(x => x.FactorValues)
                .SingleOrDefault(x => x.Id == id);
            if (riskFactor == null)
            {
                return NotFound();
            }
            var contextTariff = _context.Tariffs
                .Include(x => x.InsRuleTariffLink)
                .ThenInclude(x => x.InsRule)
                .ThenInclude(x => x.LinksToRisks)
                .ThenInclude(x => x.Risk)
                .SingleOrDefault(x => x.Id == _tariffsHelper.CurrentTariff.Id);
            if (contextTariff != null)
            {
                riskFactor.InsuranceRulesLinks = contextTariff.InsRuleTariffLink;
            }
            _factorHelper.CurrentFactor = riskFactor;
            _factorHelper.PrepareFactorData(_context);
            return View(riskFactor);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, RiskFactor riskFactor, string save, string saveAndExit)
        {
            if (id != riskFactor.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid) return View(riskFactor);
            try
            {
                riskFactor.FactorValues = _factorHelper.CurrentFactor.FactorValues;
                _context.Update(riskFactor);
                await _context.SaveChangesAsync();
                logger.Info($"{_httpAccessor.HttpContext.Connection.RemoteIpAddress} updated RiskFactor (id={riskFactor.Id})");
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RiskFactorExists(riskFactor.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            if (!string.IsNullOrEmpty(saveAndExit))
                return RedirectToAction("Edit", "Tariffs", _tariffsHelper.CurrentTariff);
            return RedirectToAction("Edit", new { id });
        }

        public IActionResult Delete(Guid id)
        {
            var riskFactor = _context.RiskFactors.Include(x => x.RiskFactorsTariffLinks)
                .SingleOrDefault(x => x.Id == id);
            if (riskFactor == null)
            {
                return NotFound();
            }

            return View(riskFactor);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var riskFactor = await _context.RiskFactors.FindAsync(id);

            if (riskFactor != null)
            {
                riskFactor.IsDeleted = true;
            }
            var contextTariff = _context.Tariffs.Include(x => x.RiskFactorsTariffLinks)
                .SingleOrDefault(x => x.Id == _tariffsHelper.CurrentTariff.Id);

            contextTariff?.RiskFactorsTariffLinks.RemoveAll(x => x.RiskFactorId == id);
            _tariffsHelper.CurrentTariff.RiskFactorsTariffLinks.RemoveAll(x => x.RiskFactorId == id);
            await _context.SaveChangesAsync();
            logger.Info($"{_httpAccessor.HttpContext.Connection.RemoteIpAddress} deleted(set IsDeleted=true) RiskFactor (id={id})");
            return RedirectToAction("Edit", "Tariffs", _tariffsHelper.CurrentTariff);
        }

        private bool RiskFactorExists(Guid id)
        {
            return _context.RiskFactors.Any(e => e.Id == id);
        }

        public async Task<IActionResult> AddFactorsToTariff(Guid? factorId = null)
        {
            var factors = new List<RiskFactorTariffLink>();
            if (factorId != null)
            {
                var factor = await _context.RiskFactors.FindAsync(factorId);
                if (factor == null) return RedirectToAction("Edit", "Tariffs", _tariffsHelper.CurrentTariff);
                factors.Add(new RiskFactorTariffLink
                {
                    RiskFactorId = factor.Id,
                    TariffId = _tariffsHelper.CurrentTariff.Id
                });
            }
            else
            {
                foreach (var selectedFactor in _factorHelper.SelectedFactors)
                {
                    factors.Add(new RiskFactorTariffLink
                    {
                        RiskFactorId = selectedFactor.Id,
                        TariffId = _tariffsHelper.CurrentTariff.Id
                    });
                }
            }

                var contextTariff = await _context.Tariffs.FindAsync(_tariffsHelper.CurrentTariff.Id);
                if (contextTariff != null)
                {
                    contextTariff.RiskFactorsTariffLinks.AddRange(factors);
                    await _context.SaveChangesAsync();
                    _factorHelper.SelectedFactors.Clear();
                }
            return RedirectToAction("Edit", "Tariffs", _tariffsHelper.CurrentTariff);
        }

        public async Task FactorCheckBoxClick(Guid id)
        {
            await _factorHelper.CheckFactor(id, _context);
        }

        public IActionResult GoBack()
        {
            var factor = _context.RiskFactors.Find(_factorHelper.CurrentFactor.Id);
            if (factor != null) _factorHelper.CurrentFactor = factor;
            return RedirectToAction("Edit", "Tariffs", _tariffsHelper.CurrentTariff);
        }

        public void UpdateFactorValue(Guid id, double factorValue)
        {
            var factor = _factorHelper.CurrentFactor.FactorValues.SingleOrDefault(x => x.Id == id && x.TariffId == _tariffsHelper.CurrentTariff.Id);
            if(factor == null) return;
            factor.Value = factorValue;
        }
    }
}

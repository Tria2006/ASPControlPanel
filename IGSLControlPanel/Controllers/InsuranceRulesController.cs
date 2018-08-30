using DBModels.Models;
using DBModels.Models.ManyToManyLinks;
using IGSLControlPanel.Data;
using IGSLControlPanel.Helpers;
using log4net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IGSLControlPanel.Controllers
{
    public class InsuranceRulesController : Controller
    {
        private readonly IGSLContext _context;
        private readonly TariffsHelper _tariffsHelper;
        private readonly InsuranceRulesHelper _insRulesHelper;
        private readonly IHttpContextAccessor _httpAccessor;
        private readonly ILog logger;

        public InsuranceRulesController(IGSLContext context, TariffsHelper tariffsHelper,
            InsuranceRulesHelper insRulesHelper, IHttpContextAccessor accessor)
        {
            _context = context;
            _httpAccessor = accessor;
            logger = LogManager.GetLogger(typeof(InsuranceRulesController));
            _tariffsHelper = tariffsHelper;
            _insRulesHelper = insRulesHelper;
        }

        public IActionResult Create()
        {
            ViewData["TariffId"] = _tariffsHelper.CurrentTariff.Id;
            var tempRule = new InsuranceRule { ValidFrom = DateTime.Today, ValidTo = new DateTime(2100, 1, 1) };
            _insRulesHelper.CurrentRule = tempRule;
            return View(tempRule);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(InsuranceRule insuranceRule, string create, string createAndExit)
        {
            if (!ModelState.IsValid) return View(insuranceRule);

            _context.Add(insuranceRule);
            insuranceRule.LinksToRisks = _insRulesHelper.CurrentRule.LinksToRisks;
            await _context.SaveChangesAsync();
            insuranceRule.LinksToTariff.Add(new InsRuleTariffLink
            {
                TariffId = _tariffsHelper.CurrentTariff.Id,
                InsRuleId = insuranceRule.Id,
                InsRule = insuranceRule
            });
            await _context.SaveChangesAsync();
            logger.Info($"{_httpAccessor.HttpContext.Connection.RemoteIpAddress} created InsuranceRule (id={insuranceRule.Id})");
            if (!string.IsNullOrEmpty(createAndExit))
                return RedirectToAction("Edit", "Tariffs", _tariffsHelper.CurrentTariff);
            return RedirectToAction("Edit", new { insuranceRule.Id });
        }

        public IActionResult Edit(Guid id)
        {
            var insuranceRule = _context.InsuranceRules
                .Include(x => x.LinksToRisks)
                .ThenInclude(x => x.Risk)
                .ThenInclude(x => x.Requirements).SingleOrDefault(x => x.Id == id) ?? _insRulesHelper.CurrentRule;
            ViewData["TariffId"] = _tariffsHelper.CurrentTariff?.Id;
            if (insuranceRule == null)
            {
                return NotFound();
            }
            _insRulesHelper.CurrentRule = insuranceRule;
            return View(insuranceRule);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, InsuranceRule insuranceRule, string save, string saveAndExit)
        {
            if (id != insuranceRule.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid) return View(insuranceRule);
            try
            {
                _context.Update(insuranceRule);
                await _context.SaveChangesAsync();
                logger.Info($"{_httpAccessor.HttpContext.Connection.RemoteIpAddress} updated InsuranceRule (id={insuranceRule.Id})");
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!InsuranceRuleExists(insuranceRule.Id))
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
            var insuranceRule = _context.InsuranceRules
                .Include(x => x.LinksToRisks)
                .ThenInclude(x => x.Risk)
                .ThenInclude(x => x.Requirements).SingleOrDefault(x => x.Id == id);
            if (insuranceRule == null)
            {
                return NotFound();
            }
            return View(insuranceRule);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var insuranceRule = await _context.InsuranceRules.FindAsync(id);

            if (insuranceRule != null)
            {
                insuranceRule.IsDeleted = true;
            }
            var contextTariff = _context.Tariffs.Include(x => x.InsRuleTariffLink)
                .SingleOrDefault(x => x.Id == _tariffsHelper.CurrentTariff.Id);

            contextTariff?.InsRuleTariffLink.RemoveAll(x => x.InsRuleId == id);
            _tariffsHelper.CurrentTariff.InsRuleTariffLink.RemoveAll(x => x.InsRuleId == id);
            await _context.SaveChangesAsync();
            logger.Info($"{_httpAccessor.HttpContext.Connection.RemoteIpAddress} deleted(set IsDeleted=true) InsuranceRule (id={id})");
            return RedirectToAction("Edit", "Tariffs", _tariffsHelper.CurrentTariff);
        }

        private bool InsuranceRuleExists(Guid id)
        {
            return _context.InsuranceRules.Any(e => e.Id == id);
        }

        public void InsRuleClick(Guid id)
        {
            _tariffsHelper.SelectUnselectRule(id);
        }

        public async Task InsRuleCheckBoxClick(Guid id)
        {
            await _insRulesHelper.CheckInsRule(id, _context);
        }

        public async Task<IActionResult> AddRulesToTariff(Guid? insRuleId = null)
        {
            var rules = new List<InsRuleTariffLink>();
            if (insRuleId != null)
            {
                var rule = await _context.InsuranceRules.FindAsync(insRuleId);
                if (rule == null) return RedirectToAction("Edit", "Tariffs", _tariffsHelper.CurrentTariff);
                rules.Add(new InsRuleTariffLink
                {
                    InsRuleId = rule.Id,
                    TariffId = _tariffsHelper.CurrentTariff.Id
                });
            }
            else
            {
                foreach (var selectedRule in _insRulesHelper.SelectedRules)
                {
                    rules.Add(new InsRuleTariffLink
                    {
                        InsRuleId = selectedRule.Id,
                        TariffId = _tariffsHelper.CurrentTariff.Id
                    });
                }
            }

            var contextTariff = await _context.Tariffs.FindAsync(_tariffsHelper.CurrentTariff.Id);
            if (contextTariff != null)
            {
                contextTariff.InsRuleTariffLink.AddRange(rules);
                await _context.SaveChangesAsync();
                _insRulesHelper.SelectedRules.Clear();
            }
            return RedirectToAction("Edit", "Tariffs", _tariffsHelper.CurrentTariff);
        }

        public IActionResult GoBack()
        {
            return RedirectToAction("Edit", "Tariffs", _tariffsHelper.CurrentTariff);
        }

        public void SaveTempData(string name)
        {
            _insRulesHelper.CurrentRule.Name = name;
        }
    }
}

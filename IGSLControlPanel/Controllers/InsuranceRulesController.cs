using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DBModels.Models;
using DBModels.Models.ManyToManyLinks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IGSLControlPanel.Data;
using IGSLControlPanel.Helpers;

namespace IGSLControlPanel.Controllers
{
    public class InsuranceRulesController : Controller
    {
        private readonly IGSLContext _context;
        private readonly TariffsHelper _tariffsHelper;
        private readonly InsuranceRulesHelper _insRulesHelper;
        private readonly EntityStateHelper _stateHelper;

        public InsuranceRulesController(IGSLContext context, TariffsHelper tariffsHelper, InsuranceRulesHelper insRulesHelper, EntityStateHelper stateHelper)
        {
            _context = context;
            _tariffsHelper = tariffsHelper;
            _insRulesHelper = insRulesHelper;
            _stateHelper = stateHelper;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.InsuranceRules.Where(x => !x.IsDeleted).ToListAsync());
        }

        public IActionResult Create()
        {
            ViewData["TariffId"] = _tariffsHelper.CurrentTariff.Id;
            var tempRule = new InsuranceRule();
            if (_stateHelper.IsInsRuleCreateInProgress)
            {
                tempRule = _insRulesHelper.CurrentRule;
            }
            else
            {
                _insRulesHelper.CurrentRule = tempRule;
                _stateHelper.IsInsRuleCreateInProgress = true;
            }
            return View(tempRule);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(InsuranceRule insuranceRule)
        {
            if (!ModelState.IsValid) return View(insuranceRule);

            if (_stateHelper.IsTariffCreateInProgress)
            {
                _tariffsHelper.CurrentTariff.InsRuleTariffLink.Add(new InsRuleTariffLink
                {
                    Tariff = _tariffsHelper.CurrentTariff,
                    InsRule = insuranceRule
                });
            }
            else
            {
                _context.Add(insuranceRule);
                await _context.SaveChangesAsync();
                insuranceRule.LinksToTariff.Add(new InsRuleTariffLink
                {
                    TariffId = _tariffsHelper.CurrentTariff.Id,
                    InsRuleId = insuranceRule.Id,
                    InsRule = insuranceRule
                });
                await _context.SaveChangesAsync();
                _stateHelper.IsRiskCreateInProgress = false;
                _stateHelper.IsInsRuleCreateInProgress = false;
            }
            return RedirectToAction(_stateHelper.IsTariffCreateInProgress ? "Create" : "Edit", "Tariffs", _tariffsHelper.CurrentTariff);
        }

        public IActionResult Edit(Guid id)
        {
            var insuranceRule = _context.InsuranceRules
                .Include(x => x.LinksToRisks)
                .ThenInclude(x => x.Risk)
                .ThenInclude(x => x.Requirements).SingleOrDefault(x => x.Id == id);
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
        public async Task<IActionResult> Edit(Guid id, InsuranceRule insuranceRule)
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
            return RedirectToAction(_stateHelper.IsTariffCreateInProgress ? "Create" : "Edit", "Tariffs", _tariffsHelper.CurrentTariff);
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
            return RedirectToAction(_stateHelper.IsTariffCreateInProgress ? "Create" : "Edit", "Tariffs", _tariffsHelper.CurrentTariff);
        }

        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveRuleConfirmed(Guid id)
        {
            var insuranceRule = _context.InsuranceRules.Include(x => x.LinksToTariff).Include(x => x.LinksToRisks).SingleOrDefault(s => s.Id == id);

            if (insuranceRule != null)
            {
                insuranceRule.IsDeleted = true;
                if (insuranceRule.LinksToTariff.Count > 0)
                {
                    foreach (var link in insuranceRule.LinksToTariff)
                    {
                        var contextTariff = _context.Tariffs.Include(x => x.InsRuleTariffLink)
                            .SingleOrDefault(x => x.Id == link.TariffId);

                        contextTariff?.InsRuleTariffLink.RemoveAll(x => x.InsRuleId == id);
                    }
                }

                if (insuranceRule.LinksToRisks.Count > 0)
                {
                    foreach (var link in insuranceRule.LinksToRisks)
                    {
                        var contextRisk = _context.Risks.SingleOrDefault(x => x.Id == link.RiskId);

                        contextRisk?.LinksToInsRules.RemoveAll(x => x.InsRuleId == id);
                    }
                }
            }
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
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
                if(rule == null) return RedirectToAction(_stateHelper.IsTariffCreateInProgress ? "Create" : "Edit", "Tariffs", _tariffsHelper.CurrentTariff);
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

            if (!_stateHelper.IsTariffCreateInProgress)
            {
                var contextTariff = await _context.Tariffs.FindAsync(_tariffsHelper.CurrentTariff.Id);
                if (contextTariff != null)
                {
                    contextTariff.InsRuleTariffLink.AddRange(rules);
                    await _context.SaveChangesAsync();
                    _insRulesHelper.SelectedRules.Clear();
                }
            }
            else
            {
                foreach (var link in rules)
                {
                    link.InsRule = await _context.InsuranceRules.FindAsync(link.InsRuleId);
                }
                _tariffsHelper.CurrentTariff.InsRuleTariffLink.AddRange(rules);
            }
            return RedirectToAction(_stateHelper.IsTariffCreateInProgress ? "Create" : "Edit", "Tariffs", _tariffsHelper.CurrentTariff);
        }

        public IActionResult GoBack()
        {
            return RedirectToAction(_stateHelper.IsTariffCreateInProgress ? "Create" : "Edit", "Tariffs", _tariffsHelper.CurrentTariff);
        }

        public void SaveTempData(string name)
        {
            _insRulesHelper.CurrentRule.Name = name;
        }
    }
}

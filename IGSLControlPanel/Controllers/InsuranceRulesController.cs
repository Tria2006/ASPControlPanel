using System;
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
    public class InsuranceRulesController : Controller
    {
        private readonly IGSLContext _context;
        private readonly TariffsHelper _tariffsHelper;

        public InsuranceRulesController(IGSLContext context, TariffsHelper tariffsHelper)
        {
            _context = context;
            _tariffsHelper = tariffsHelper;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.InsuranceRules.ToListAsync());
        }

        public IActionResult Create()
        {
            _tariffsHelper.IsInsRuleCreateInProgress = true;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(InsuranceRule insuranceRule)
        {
            if (!ModelState.IsValid) return View(insuranceRule);

            if (_tariffsHelper.IsTariffCreateInProgress)
            {
                _tariffsHelper.CurrentTariff.InsRuleTariffLink.Add(new InsRuleTariffLink
                {
                    Tariff = _tariffsHelper.CurrentTariff,
                    InsRule = insuranceRule
                });
            }
            else
            {
                insuranceRule.CreateDate = DateTime.Now;
                _context.Add(insuranceRule);
                await _context.SaveChangesAsync();
                insuranceRule.LinksToTariff.Add(new InsRuleTariffLink
                {
                    TariffId = _tariffsHelper.CurrentTariff.Id,
                    InsRuleId = insuranceRule.Id,
                    InsRule = insuranceRule
                });
                await _context.SaveChangesAsync();
                _tariffsHelper.IsInsRuleCreateInProgress = false;
            }
            return RedirectToAction(_tariffsHelper.IsTariffCreateInProgress ? "Create" : "Edit", "Tariffs", _tariffsHelper.CurrentTariff);
        }

        public async Task<IActionResult> Edit(Guid id)
        {
            var insuranceRule = await _context.InsuranceRules.FindAsync(id);
            if (insuranceRule == null)
            {
                return NotFound();
            }
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
                insuranceRule.ModifyDate = DateTime.Now;
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
            return RedirectToAction(_tariffsHelper.IsTariffCreateInProgress ? "Create" : "Edit", "Tariffs", _tariffsHelper.CurrentTariff);
        }

        public async Task<IActionResult> Delete(Guid id)
        {
            var insuranceRule = await _context.InsuranceRules.FirstOrDefaultAsync(m => m.Id == id);
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
                insuranceRule.ModifyDate = DateTime.Now;
            }
            var contextTariff = _context.Tariffs.Include(x => x.InsRuleTariffLink)
                .SingleOrDefault(x => x.Id == _tariffsHelper.CurrentTariff.Id);

            contextTariff?.InsRuleTariffLink.RemoveAll(x => x.InsRuleId == id);
            await _context.SaveChangesAsync();
            return RedirectToAction(_tariffsHelper.IsTariffCreateInProgress ? "Create" : "Edit", "Tariffs", _tariffsHelper.CurrentTariff);
        }

        private bool InsuranceRuleExists(Guid id)
        {
            return _context.InsuranceRules.Any(e => e.Id == id);
        }

        public void InsRuleClick(Guid id)
        {
            _tariffsHelper.SelectUnselectRule(id);
        }
    }
}

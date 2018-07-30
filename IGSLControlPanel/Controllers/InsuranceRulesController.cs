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
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(InsuranceRule insuranceRule)
        {
            if (!ModelState.IsValid) return View(insuranceRule);
            _context.Add(insuranceRule);
            await _context.SaveChangesAsync();
            insuranceRule.LinksToTariff.Add(new InsRuleTariffLink
            {
                TariffId = _tariffsHelper.CurrentTariff.Id,
                Tariff = _tariffsHelper.CurrentTariff,
                InsRuleId = insuranceRule.Id,
                InsRule = insuranceRule
            });
            await _context.SaveChangesAsync();
            return RedirectToAction(_tariffsHelper.IsCreateInProgress ? "Create" : "Edit", "Tariffs", _tariffsHelper.CurrentTariff);
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
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var insuranceRule = await _context.InsuranceRules
                .FirstOrDefaultAsync(m => m.Id == id);
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
            _context.InsuranceRules.Remove(insuranceRule);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool InsuranceRuleExists(Guid id)
        {
            return _context.InsuranceRules.Any(e => e.Id == id);
        }
    }
}

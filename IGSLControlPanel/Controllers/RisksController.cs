using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DBModels.Models;
using DBModels.Models.ManyToManyLinks;
using IGSLControlPanel.Data;

namespace IGSLControlPanel.Controllers
{
    public class RisksController : Controller
    {
        private readonly IGSLContext _context;

        public RisksController(IGSLContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.Risks.Include(x => x.LinksToInsRules).ThenInclude(x => x.InsRule).Include(x => x.Requirements).ToListAsync());
        }

        public IActionResult Create(Guid tariffId, Guid insRuleId)
        {
            var tempRisk = new Risk();
            ViewData["InsRuleId"] = insRuleId;
            ViewData["TariffId"] = tariffId;
            return View(tempRisk);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Risk risk, Guid tariffId, Guid insRuleId)
        {
            if (!ModelState.IsValid) return View(risk);
            _context.Add(risk);
            await _context.SaveChangesAsync();

            risk.LinksToInsRules.Add(new RiskInsRuleLink
            {
                InsRuleId = insRuleId,
                RiskId = risk.Id
            });

            risk.Requirements.Add(new RiskRequirement
            {
                RiskId = risk.Id,
                TariffId = tariffId,
                IsRequired = risk.OnCreateRequired
            });

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(Guid id)
        {
            var risk = _context.Risks.Include(x => x.LinksToInsRules).ThenInclude(x => x.InsRule).Include(x => x.Requirements).SingleOrDefault(x => x.Id == id);
            if (risk == null)
            {
                return NotFound();
            }
            return View(risk);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, Risk risk)
        {
            if (id != risk.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid) return View(risk);
            try
            {
                _context.Update(risk);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RiskExists(risk.Id))
                {
                    return NotFound();
                }
                throw;
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(Guid id)
        {
            var risk = await _context.Risks
                .FirstOrDefaultAsync(m => m.Id == id);
            if (risk == null)
            {
                return NotFound();
            }

            return View(risk);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var risk = await _context.Risks.FindAsync(id);
            risk.IsDeleted = true;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RiskExists(Guid id)
        {
            return _context.Risks.Any(e => e.Id == id);
        }
    }
}

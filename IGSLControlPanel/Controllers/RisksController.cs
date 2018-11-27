using DBModels.Models;
using DBModels.Models.ManyToManyLinks;
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
    public class RisksController : Controller
    {
        private readonly IGSLContext _context;
        private readonly InsuranceRulesHelper _insRuleHelper;
        private readonly IHttpContextAccessor _httpAccessor;
        private readonly RisksHelper _risksHelper;
        private readonly ILog logger;

        public RisksController(IGSLContext context, InsuranceRulesHelper insRuleHelper, IHttpContextAccessor accessor, RisksHelper risksHelper)
        {
            _context = context;
            _httpAccessor = accessor;
            logger = LogManager.GetLogger(typeof(RisksController));
            _insRuleHelper = insRuleHelper;
            _risksHelper = risksHelper;
        }

        public IActionResult Create(Guid tariffId, Guid insRuleId)
        {
            var tempRisk = new Risk
            {
                ValidFrom = DateTime.Today,
                ValidTo = new DateTime(2100, 1, 1)
            };
            ViewData["InsRuleId"] = insRuleId;
            ViewData["TariffId"] = tariffId;
            _insRuleHelper.CurrentRisk = tempRisk;
            return View(tempRisk);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Risk risk, Guid tariffId, Guid insRuleId, string create, string createAndExit)
        {
            if (!ModelState.IsValid) return View(risk);

            _context.Add(risk);
            await _context.SaveChangesAsync();

            _insRuleHelper.CurrentRule.LinksToRisks.Add(new RiskInsRuleLink
            {
                InsRuleId = insRuleId,
                RiskId = risk.Id
            });

            risk.LinksToInsRules.Add(new RiskInsRuleLink
            {
                InsRuleId = insRuleId,
                RiskId = risk.Id
            });

            var req = new RiskRequirement
            {
                RiskId = risk.Id,
                TariffId = tariffId,
                IsRequired = risk.OnCreateRequired
            };
            risk.Requirements.Add(req);

            await _context.SaveChangesAsync();
            _risksHelper.TempRequirement = req;

            logger.Info($"{_httpAccessor.HttpContext.Connection.RemoteIpAddress} created Risk (id={risk.Id})");
            if (!string.IsNullOrEmpty(createAndExit))
                return RedirectToAction("Edit", "InsuranceRules", _insRuleHelper.CurrentRule);
            return RedirectToAction("Edit", new { risk.Id, tariffId });
        }

        public IActionResult Edit(Guid id, Guid tariffId)
        {
            ViewData["TariffId"] = tariffId;
            var risk = _context.Risks.Include(x => x.LinksToInsRules)
                           .ThenInclude(x => x.InsRule)
                           .Include(x => x.Requirements).SingleOrDefault(x => x.Id == id) ?? _insRuleHelper.CurrentRisk;
            if (risk == null)
            {
                return NotFound();
            }
            _insRuleHelper.CurrentRisk = risk;
            _risksHelper.TempRequirement =
                risk.Requirements.SingleOrDefault(x => x.RiskId == id && x.TariffId == tariffId);
            return View(risk);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, Risk risk, string save, string saveAndExit, Guid tariffId)
        {
            if (id != risk.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid) return View(risk);
            try
            {
                _context.Update(risk);
                if (_risksHelper.TempRequirement != null)
                {
                    var requirement = await _context.RiskRequirements.FindAsync(_risksHelper.TempRequirement.Id);
                    if (requirement != null)
                    {
                        requirement.IsRequired = _risksHelper.TempRequirement.IsRequired;
                    }
                }
                await _context.SaveChangesAsync();
                logger.Info($"{_httpAccessor.HttpContext.Connection.RemoteIpAddress} updated Risk (id={risk.Id})");
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RiskExists(risk.Id))
                {
                    return NotFound();
                }
                throw;
            }
            if (!string.IsNullOrEmpty(saveAndExit))
                return RedirectToAction("Edit", "InsuranceRules", _insRuleHelper.CurrentRule);
            return RedirectToAction("Edit", new { id,  tariffId});
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

            var links = _context.InsuranceRules.Where(x => x.LinksToRisks.Any(s => s.RiskId == id));
            foreach (var link in links)
            {
                link.LinksToRisks.RemoveAll(x => x.RiskId == id);
            }

            await _context.SaveChangesAsync();
            logger.Info($"{_httpAccessor.HttpContext.Connection.RemoteIpAddress} deleted(set IsDeleted=true) Risk (id={id})");
            return RedirectToAction("Edit", "InsuranceRules", _insRuleHelper.CurrentRule);
        }

        private bool RiskExists(Guid id)
        {
            return _context.Risks.Any(e => e.Id == id);
        }

        public async Task UpdateRequirement(Guid reqId, bool? required)
        {
            var requirement = await _context.RiskRequirements.FindAsync(reqId);

            if (requirement == null) return;
            _risksHelper.TempRequirement = requirement;
            _risksHelper.TempRequirement.IsRequired = required ?? false;
        }

        public IActionResult GoBack(Guid reqId, bool? required = null)
        {
            return RedirectToAction("Edit", "InsuranceRules", _insRuleHelper.CurrentRule);
        }
    }
}

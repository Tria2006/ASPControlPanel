using System;
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
    public class RisksController : Controller
    {
        private readonly IGSLContext _context;
        private readonly InsuranceRulesHelper _insRuleHelper;
        private readonly EntityStateHelper _stateHelper;
        private readonly IHttpContextAccessor _httpAccessor;
        private readonly ILog logger;

        public RisksController(IGSLContext context, InsuranceRulesHelper insRuleHelper, EntityStateHelper stateHelper, IHttpContextAccessor accessor)
        {
            _context = context;
            _httpAccessor = accessor;
            logger = LogManager.GetLogger(typeof(RisksController));
            _insRuleHelper = insRuleHelper;
            _stateHelper = stateHelper;
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
            if (_stateHelper.IsRiskCreateInProgress)
            {
                tempRisk = _insRuleHelper.CurrentRisk;
            }
            else
            {
                _insRuleHelper.CurrentRisk = tempRisk;
                _stateHelper.IsRiskCreateInProgress = true;
            }
            return View(tempRisk);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Risk risk, Guid tariffId, Guid insRuleId, string create, string createAndExit)
        {
            if (!ModelState.IsValid) return View(risk);

            if (_stateHelper.IsInsRuleCreateInProgress)
            {
                _insRuleHelper.CurrentRule.LinksToRisks.Add(new RiskInsRuleLink
                {
                    InsRule = _insRuleHelper.CurrentRule,
                    Risk = risk
                });
                _insRuleHelper.CurrentRisk = risk;
            }
            else
            {
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

                risk.Requirements.Add(new RiskRequirement
                {
                    RiskId = risk.Id,
                    TariffId = tariffId,
                    IsRequired = risk.OnCreateRequired
                });

                await _context.SaveChangesAsync();
                _stateHelper.IsRiskCreateInProgress = false;
                logger.Info($"{_httpAccessor.HttpContext.Connection.RemoteIpAddress} created Risk (id={risk.Id})");
            }
            if (!string.IsNullOrEmpty(createAndExit))
                return RedirectToAction(_stateHelper.IsInsRuleCreateInProgress ? "Create" : "Edit", "InsuranceRules", _insRuleHelper.CurrentRule);
            return RedirectToAction("Edit", new { risk.Id });
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
            return View(risk);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, Risk risk, string save, string saveAndExit)
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
                return RedirectToAction(_stateHelper.IsInsRuleCreateInProgress ? "Create" : "Edit", "InsuranceRules", _insRuleHelper.CurrentRule);
            return RedirectToAction("Edit", new { id });
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
            return RedirectToAction(_stateHelper.IsInsRuleCreateInProgress ? "Create" : "Edit", "InsuranceRules", _insRuleHelper.CurrentRule);
        }

        private bool RiskExists(Guid id)
        {
            return _context.Risks.Any(e => e.Id == id);
        }

        public async Task UpdateRequirement(Guid reqId, bool? required = null)
        {
            var requirement = await _context.RiskRequirements.FindAsync(reqId);

            if(requirement == null) return;
            requirement.IsRequired = required ?? !requirement.IsRequired;
            await _context.SaveChangesAsync();
        }

        public IActionResult GoBack()
        {
            return RedirectToAction(_stateHelper.IsInsRuleCreateInProgress ? "Create" : "Edit", "InsuranceRules", _insRuleHelper.CurrentRule);
        }
    }
}

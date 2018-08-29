using System;
using System.Linq;
using System.Threading.Tasks;
using DBModels.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IGSLControlPanel.Data;

namespace IGSLControlPanel.Controllers
{
    public class ParameterGroupsController : Controller
    {
        private readonly IGSLContext _context;

        public ParameterGroupsController(IGSLContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View(await _context.ParameterGroups.Where(p => !p.IsDeleted).ToListAsync());
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ParameterGroup parameterGroup, string create, string createAndExit)
        {
            if (!ModelState.IsValid) return View(parameterGroup);
            _context.Add(parameterGroup);
            await _context.SaveChangesAsync();
            if (!string.IsNullOrEmpty(createAndExit))
                return RedirectToAction(nameof(Index));
            return RedirectToAction("Edit", new { parameterGroup.Id });
        }

        public async Task<IActionResult> Edit(Guid id)
        {
            var parameterGroup = await _context.ParameterGroups.FindAsync(id);
            if (parameterGroup == null)
            {
                return NotFound();
            }
            return View(parameterGroup);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, ParameterGroup parameterGroup, string save, string saveAndExit)
        {
            if (id != parameterGroup.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid) return View(parameterGroup);
            try
            {
                _context.Update(parameterGroup);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ParameterGroupExists(parameterGroup.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            if (!string.IsNullOrEmpty(saveAndExit))
                return RedirectToAction(nameof(Index));
            return RedirectToAction("Edit", new { id });
        }

        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var parameterGroup = await _context.ParameterGroups
                .FirstOrDefaultAsync(m => m.Id == id);
            if (parameterGroup == null)
            {
                return NotFound();
            }

            return View(parameterGroup);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var parameterGroup = await _context.ParameterGroups.FindAsync(id);
            parameterGroup.IsDeleted = true;
            var parameters = _context.ProductParameters.Where(x => x.GroupId == id);
            foreach (var parameter in parameters)
            {
                parameter.GroupId = null;
            }
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ParameterGroupExists(Guid id)
        {
            return _context.ParameterGroups.Any(e => e.Id == id);
        }
    }
}

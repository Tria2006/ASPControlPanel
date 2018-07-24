using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IGSLControlPanel.Data;
using IGSLControlPanel.Models;

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
        public async Task<IActionResult> Create(ParameterGroup parameterGroup)
        {
            if (!ModelState.IsValid) return View(parameterGroup);
            parameterGroup.Id = Guid.NewGuid();
            _context.Add(parameterGroup);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var parameterGroup = await _context.ParameterGroups.FindAsync(id);
            if (parameterGroup == null)
            {
                return NotFound();
            }
            return View(parameterGroup);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, ParameterGroup parameterGroup)
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
            return RedirectToAction(nameof(Index));
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

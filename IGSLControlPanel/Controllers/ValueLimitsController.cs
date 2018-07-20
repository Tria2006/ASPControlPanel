using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IGSLControlPanel.Data;
using IGSLControlPanel.Models;

namespace IGSLControlPanel.Controllers
{
    public class ValueLimitsController : Controller
    {
        private readonly IGSLContext _context;

        public ValueLimitsController(IGSLContext context)
        {
            _context = context;
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,ParameterDataType,IntValueFrom,IntValueTo,DateValueFrom,DateValueTo,IsDeleted,ParameterId,ProductId")] ValueLimit valueLimit)
        {
            if (!ModelState.IsValid) return View(valueLimit);
            valueLimit.Id = Guid.NewGuid();
            _context.Add(valueLimit);
            await _context.SaveChangesAsync();
            return RedirectToAction("Create");
        }

        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var valueLimit = await _context.ValueLimits.FindAsync(id);
            if (valueLimit == null)
            {
                return NotFound();
            }
            return View(valueLimit);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,Name,ParameterDataType,IntValueFrom,IntValueTo,DateValueFrom,DateValueTo,IsDeleted,ParameterId,ProductId")] ValueLimit valueLimit)
        {
            if (id != valueLimit.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(valueLimit);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ValueLimitExists(valueLimit.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Edit");
            }
            return View(valueLimit);
        }

        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var valueLimit = await _context.ValueLimits
                .FirstOrDefaultAsync(m => m.Id == id);
            if (valueLimit == null)
            {
                return NotFound();
            }

            return View(valueLimit);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var valueLimit = await _context.ValueLimits.FindAsync(id);
            _context.ValueLimits.Remove(valueLimit);
            await _context.SaveChangesAsync();
            return RedirectToAction("Delete");
        }

        private bool ValueLimitExists(Guid id)
        {
            return _context.ValueLimits.Any(e => e.Id == id);
        }
    }
}

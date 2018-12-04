using DBModels.Models;
using DBModels.Models.ManyToManyLinks;
using IGSLControlPanel.Data;
using IGSLControlPanel.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace IGSLControlPanel.Controllers
{
    public class ParameterGroupsController : Controller
    {
        private readonly IGSLContext _context;
        private readonly GroupsHelper _groupHelper;
        private readonly ProductsHelper _productsHelper;

        public ParameterGroupsController(IGSLContext context, GroupsHelper groupHelper, ProductsHelper productsHelper)
        {
            _context = context;
            _groupHelper = groupHelper;
            _productsHelper = productsHelper;
        }

        public async Task<IActionResult> Index()
        {
            _productsHelper.CurrentProduct = null;
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
            return parameterGroup.IsGlobal ? await EditGlobal(parameterGroup.Id) : View(parameterGroup);
        }

        public async Task<IActionResult> EditGlobal(Guid id)
        {
            var parameterGroup = await _context.ParameterGroups.FindAsync(id);
            if (parameterGroup == null)
            {
                return NotFound();
            }

            ViewData["Parameters"] = _context.ProductParameters.Where(x => x.GroupId == id && !x.IsDeleted).ToList();
            return View("EditGlobal", parameterGroup);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditGlobal(Guid id, ParameterGroup parameterGroup, string save, string saveAndExit)
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
            return RedirectToAction("EditGlobal", new { id });
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

        public async Task SelectGroup(Guid id)
        {
            await _groupHelper.SelectGroup(id, _context);
        }

        public IActionResult AttachGroup(Guid productId)
        {
            var product = _context.Products
                .Include(x => x.LinkToProductParameters)
                .ThenInclude(x => x.Parameter).SingleOrDefault(x => x.Id == productId);
            if (product == null) return PartialView("_ParameterGroupsBlock", _productsHelper.CurrentProduct);

            var globalParams = _groupHelper.GetSelectedGroupParameters(_context);

            foreach (var globalParam in globalParams.Where(x => x.IsParamTemplate))
            {
                var linkParam = new ProductParameter(globalParam);
                _context.ProductParameters.Add(linkParam);
                _context.SaveChanges();

                if (linkParam.Limit != null)
                {
                    linkParam.Limit.ParameterId = linkParam.Id;
                    _context.SaveChanges();
                }

                product.LinkToProductParameters.Add(new ProductLinkToProductParameter
                {
                    ProductId = productId,
                    ProductParameterId = linkParam.Id
                });
                _context.SaveChanges();
            }
            return PartialView("_ParameterGroupsBlock", product);
        }

        public IActionResult GoBack()
        {
            return RedirectToAction("Index");
        }
    }
}

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DBModels.Models;
using IGSLControlPanel.Data;
using IGSLControlPanel.Helpers;
using log4net;
using Microsoft.AspNetCore.Http;

namespace IGSLControlPanel.Controllers
{
    public class LimitListItemsController : Controller
    {
        private readonly IGSLContext _context;
        private readonly ProductsHelper _productsHelper;
        private readonly IHttpContextAccessor _httpAccessor;
        private readonly ILog logger;

        public LimitListItemsController(IGSLContext context, ProductsHelper productsHelper, IHttpContextAccessor accessor)
        {
            _context = context;
            _httpAccessor = accessor;
            logger = LogManager.GetLogger(typeof(ProductParametersController));
            _productsHelper = productsHelper;
        }

        public IActionResult Create(Guid limitId)
        {
            var tempItem = new LimitListItem
            {
                ValueLimitId = limitId,
                ValidFrom = DateTime.Now,
                ValidTo = new DateTime(2100,1,1)
            };
            return View(tempItem);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(LimitListItem limitListItem, string create, string createAndExit)
        {
            if (!ModelState.IsValid) return View(limitListItem);
                _context.Add(limitListItem);
                await _context.SaveChangesAsync();
                _productsHelper.CurrentParameter.Limit.LimitListItems.Add(limitListItem);
                logger.Info($"{_httpAccessor.HttpContext.Connection.RemoteIpAddress} created LimitListItem (id={limitListItem.Id})");

            if (!string.IsNullOrEmpty(createAndExit))
                return RedirectToAction("Edit", "ValueLimits", new {id = limitListItem.ValueLimitId});
            return RedirectToAction("Edit", new { limitListItem.Id });
        }

        public async Task<IActionResult> Edit(Guid id)
        {
            var limitListItem = await _context.LimitListItems.FindAsync(id);
            if (limitListItem == null)
            {
                return NotFound();
            }
            return View(limitListItem);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, LimitListItem limitListItem, string save, string saveAndExit)
        {
            if (id != limitListItem.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid) return View(limitListItem);
            try
            {
                _context.Update(limitListItem);
                await _context.SaveChangesAsync();
                var val =_productsHelper.CurrentParameter.Limit.LimitListItems?.SingleOrDefault(x => x.Id == id);
                if (val != null)
                {
                    _productsHelper.CurrentParameter.Limit.LimitListItems.Remove(val);
                    _productsHelper.CurrentParameter.Limit.LimitListItems.Add(limitListItem);
                }
                logger.Info($"{_httpAccessor.HttpContext.Connection.RemoteIpAddress} edited LimitListItem (id={limitListItem.Id})");
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LimitListItemExists(limitListItem.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            if (!string.IsNullOrEmpty(saveAndExit))
                return RedirectToAction("Edit", "ValueLimits", new { id = limitListItem.ValueLimitId });
            return RedirectToAction("Edit", new { id });
        }

        public async Task<IActionResult> Delete(Guid id)
        {
            var limitListItem = await _context.LimitListItems
                .FirstOrDefaultAsync(m => m.Id == id);
            if (limitListItem == null)
            {
                return NotFound();
            }

            return View(limitListItem);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var limitListItem = await _context.LimitListItems.FindAsync(id);
            limitListItem.IsDeleted = true;
            await _context.SaveChangesAsync();
            _productsHelper.CurrentParameter.Limit.LimitListItems.RemoveAll(x => x.Id == id);
            logger.Info($"{_httpAccessor.HttpContext.Connection.RemoteIpAddress} deleted(set IsDeleted=true) LimitItem (id={id})");
            return RedirectToAction("Edit", "ValueLimits", _productsHelper.CurrentParameter.Limit);
        }

        private bool LimitListItemExists(Guid id)
        {
            return _context.LimitListItems.Any(e => e.Id == id);
        }
    }
}

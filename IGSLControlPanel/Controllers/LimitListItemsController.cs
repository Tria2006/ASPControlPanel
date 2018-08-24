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
        private readonly EntityStateHelper _stateHelper;
        private readonly IHttpContextAccessor _httpAccessor;
        private readonly ILog logger;

        public LimitListItemsController(IGSLContext context, ProductsHelper productsHelper, EntityStateHelper stateHelper, IHttpContextAccessor accessor)
        {
            _context = context;
            _httpAccessor = accessor;
            logger = LogManager.GetLogger(typeof(ProductParametersController));
            _productsHelper = productsHelper;
            _stateHelper = stateHelper;
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
        public async Task<IActionResult> Create(LimitListItem limitListItem)
        {
            if (!ModelState.IsValid) return View(limitListItem);
            if (_stateHelper.IsValueLimitCreateInProgress)
            {
                _productsHelper.CurrentParameter.Limit.LimitListItems.Add(limitListItem);
            }
            else
            {
                _context.Add(limitListItem);
                await _context.SaveChangesAsync();
                logger.Info($"{_httpAccessor.HttpContext.Connection.RemoteIpAddress} created LimitListItem (id={limitListItem.Id})");
            }
            return RedirectToAction(_stateHelper.IsParameterCreateInProgress ? "Create" : "Edit", "ValueLimits", _productsHelper.CurrentParameter.Limit);
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
        public async Task<IActionResult> Edit(Guid id, LimitListItem limitListItem)
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
            return RedirectToAction(_stateHelper.IsParameterCreateInProgress ? "Create" : "Edit", "ProductParameters", _productsHelper.CurrentParameter);
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
            return RedirectToAction(_stateHelper.IsParameterCreateInProgress ? "Create" : "Edit", "ProductParameters", _productsHelper.CurrentParameter);
        }

        private bool LimitListItemExists(Guid id)
        {
            return _context.LimitListItems.Any(e => e.Id == id);
        }
    }
}

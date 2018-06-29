﻿using System;
using System.Linq;
using System.Threading.Tasks;
using IGSLControlPanel.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IGSLControlPanel.Models;
using Newtonsoft.Json;

namespace IGSLControlPanel.Controllers
{
    public class ProductsController : Controller
    {
        private readonly IGSLContext _context;

        public ProductsController(IGSLContext context)
        {
            _context = context;
        }

        // GET: Products
        public async Task<IActionResult> Index()
        {
            var vm = new ProductsListPageViewModel(_context);
            return View(vm.FoldersTree);
        }
        
        // GET: Products/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .SingleOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View();
        }

        // GET: Products/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Products/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ParentFolderId,Name")] FolderTreeEntry folderTreeEntry)
        {
            if (ModelState.IsValid)
            {
                folderTreeEntry.Id = Guid.NewGuid();
                _context.Add(folderTreeEntry);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(folderTreeEntry);
        }

        // GET: Products/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products.SingleOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        // POST: Products/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,ParentFolderId,Name")] Product folderTreeEntry)
        {
            if (id != folderTreeEntry.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(folderTreeEntry);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FolderTreeEntryExists(folderTreeEntry.Id))
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
            return View(folderTreeEntry);
        }

        // GET: Products/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var folderTreeEntry = await _context.FolderTreeEntries
                .SingleOrDefaultAsync(m => m.Id == id);
            if (folderTreeEntry == null)
            {
                return NotFound();
            }

            return View(folderTreeEntry);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var folderTreeEntry = await _context.FolderTreeEntries.SingleOrDefaultAsync(m => m.Id == id);
            _context.FolderTreeEntries.Remove(folderTreeEntry);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool FolderTreeEntryExists(Guid id)
        {
            return _context.FolderTreeEntries.Any(e => e.Id == id);
        }
    }
}
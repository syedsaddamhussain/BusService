﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BusService.Models;

namespace BusService.Controllers
{
    public class BusRouteController : Controller
    {
        private readonly BusServiceContext _context;

        public BusRouteController(BusServiceContext context)
        {
            _context = context;
        }

        // GET: BusRoute
        public async Task<IActionResult> Index()
        {
            //convert to oreder by int
              return View(await _context.BusRoute.OrderBy(a=>Convert.ToInt32(a.BusRouteCode)).ToListAsync());
        }

        // GET: BusRoute/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null || _context.BusRoute == null)
            {
                return NotFound();
            }

            var busRoute = await _context.BusRoute
                .FirstOrDefaultAsync(m => m.BusRouteCode == id);
            if (busRoute == null)
            {
                return NotFound();
            }

            return View(busRoute);
        }

        // GET: BusRoute/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: BusRoute/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BusRouteCode,RouteName")] BusRoute busRoute)
        {
            if (ModelState.IsValid)
            {
                _context.Add(busRoute);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(busRoute);
        }

        // GET: BusRoute/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null || _context.BusRoute == null)
            {
                return NotFound();
            }

            var busRoute = await _context.BusRoute.FindAsync(id);
            if (busRoute == null)
            {
                return NotFound();
            }
            return View(busRoute);
        }

        // POST: BusRoute/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("BusRouteCode,RouteName")] BusRoute busRoute)
        {
            if (id != busRoute.BusRouteCode)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(busRoute);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BusRouteExists(busRoute.BusRouteCode))
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
            return View(busRoute);
        }

        // GET: BusRoute/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null || _context.BusRoute == null)
            {
                return NotFound();
            }

            var busRoute = await _context.BusRoute
                .FirstOrDefaultAsync(m => m.BusRouteCode == id);
            if (busRoute == null)
            {
                return NotFound();
            }

            return View(busRoute);
        }

        // POST: BusRoute/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            if (_context.BusRoute == null)
            {
                return Problem("Entity set 'BusServiceContext.BusRoute'  is null.");
            }
            var busRoute = await _context.BusRoute.FindAsync(id);
            if (busRoute != null)
            {
                _context.BusRoute.Remove(busRoute);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BusRouteExists(string id)
        {
          return _context.BusRoute.Any(e => e.BusRouteCode == id);
        }
    }
}

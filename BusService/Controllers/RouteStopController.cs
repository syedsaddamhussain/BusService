using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BusService.Models;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Http;

namespace BusService.Controllers
{
    public class RouteStopController : Controller
    {
        private readonly BusServiceContext _context;

        public RouteStopController(BusServiceContext context)
        {
            _context = context;
        }

        // GET: RouteStop
        public async Task<IActionResult> Index(string BusRouteCode)
        {
            if (BusRouteCode != null)
            {
                //store in cookie or session
                Response.Cookies.Append("BusRouteCode", BusRouteCode);
                HttpContext.Session.SetString("BusRouteCode", BusRouteCode);

            }else if (Request.Query["BusRouteCode"].Any())
            {
                //store in cookies or session
                BusRouteCode = Request.Query["BusRouteCode"].ToString();
                Response.Cookies.Append("BusRouteCode", BusRouteCode);
                HttpContext.Session.SetString("BusRouteCode", BusRouteCode);

            } else if (Request.Cookies["BusRouteCode"] != null)
            {
                //retrieve the value from cookie
                BusRouteCode = Request.Cookies["BusRouteCode"].ToString();

            } else if (HttpContext.Session.GetString("BusRouteCode") != null)
            {
                //retrieve the value from session
                BusRouteCode = HttpContext.Session.GetString("BusRouteCode");
            }
            else
            {
                TempData["message"] = "Select a Route";
                return RedirectToAction("Index", "BusRoute");
            }
            var busRoute = await _context.BusRoute.Where(a => a.BusRouteCode == BusRouteCode).FirstOrDefaultAsync();

            ViewData["BCode"] = BusRouteCode;
            ViewData["RName"] = busRoute.RouteName;

            var busServiceContext = _context.RouteStop.Include(r => r.BusRouteCodeNavigation)
                .Include(r => r.BusRouteCodeNavigation)
                .Include(r => r.BusStopNumberNavigation)
                .Where(r => r.BusRouteCode == BusRouteCode)
                .OrderBy(r => r.OffsetMinutes);
            return View(await busServiceContext.ToListAsync());
        }

        // GET: RouteStop/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.RouteStop == null)
            {
                return NotFound();
            }

            var routeStop = await _context.RouteStop
                .Include(r => r.BusRouteCodeNavigation)
                .Include(r => r.BusStopNumberNavigation)
                .FirstOrDefaultAsync(m => m.RouteStopId == id);
            if (routeStop == null)
            {
                return NotFound();
            }

            return View(routeStop);
        }

        // GET: RouteStop/Create
        public IActionResult Create()
        {
            string BusCode = string.Empty;
            if (Request.Cookies["BusRouteCode"] != null)
            {
                //retrieve the value from cookie
                BusCode = Request.Cookies["BusRouteCode"].ToString();

            } else if (HttpContext.Session.GetString("BusRouteCode") != null)
            {
                //retrieve the value from session
                BusCode = HttpContext.Session.GetString("BusRouteCode");
            }

            var busRCode = _context.BusRoute.Where(a => a.BusRouteCode == BusCode).FirstOrDefault();
            
            ViewData["BCode"] = BusCode;
            ViewData["RName"] = busRCode.RouteName;

            ViewData["BusRouteCode"] = new SelectList(_context.BusRoute, "BusRouteCode", "BusRouteCode");
            ViewData["BusStopNumber"] = new SelectList(_context.BusStop.OrderBy(a => a.Location), "BusStopNumber", "Location");
            return View();
        }

        // POST: RouteStop/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("RouteStopId,BusRouteCode,BusStopNumber,OffsetMinutes")] RouteStop routeStop)
        {
            string BusCode = string.Empty;
            if (Request.Cookies["BusRouteCode"] != null)
            {
                //retrieve the value from cookie
                BusCode = Request.Cookies["BusRouteCode"].ToString();

            }
            else if (HttpContext.Session.GetString("BusRouteCode") != null)
            {
                //retrieve the value from session
                BusCode = HttpContext.Session.GetString("BusRouteCode");
            }

            var busRCode = _context.BusRoute.Where(a => a.BusRouteCode == BusCode).FirstOrDefault();

            ViewData["BCode"] = BusCode;
            ViewData["RName"] = busRCode.RouteName;

            //important validation
            routeStop.BusRouteCode = BusCode;
            //i.	The offsetMinutes must be zero or more
            if (routeStop.OffsetMinutes < 0 ) 
            {
                ModelState.AddModelError("", "Offset minutes cannot be less than 0");
            }
            //ii.	There must be one and only one record on the table with offsetMinutes of zero …
            //so the first one added for this route must be zero.
            if (routeStop.OffsetMinutes == 0)
            {
                var isZeroExist = _context.RouteStop.Where(a => a.OffsetMinutes == 0 && a.BusRouteCode == routeStop.BusRouteCode);
                if (isZeroExist.Any())
                {
                    ModelState.AddModelError("", "There Can be one offset minute 0 in the record");
                }
            }
            //iii.	There cannot be a duplicate route/stop combination already on file.
            var isDuplicate = _context.RouteStop.Where(a => a.BusRouteCode == routeStop.BusRouteCode
                            && a.BusStopNumber == routeStop.BusStopNumber);
            if (isDuplicate.Any())
            {
                ModelState.AddModelError("", "Already Exists");
            }

            if (ModelState.IsValid)
            {
                _context.Add(routeStop);
                await _context.SaveChangesAsync();
                //iv.	If the record passes these criteria and is successfully added to the database,
                //return to the routeStop listing with a message saying so.
                TempData["message"] = "Stop successfully added";
                return RedirectToAction(nameof(Index));
            }
            ViewData["BusRouteCode"] = new SelectList(_context.BusRoute, "BusRouteCode", "BusRouteCode", routeStop.BusRouteCode);
            ViewData["BusStopNumber"] = new SelectList(_context.BusStop.OrderBy(a => a.Location), "BusStopNumber", "Location", routeStop.BusStopNumber);
            return View(routeStop);
        }

        // GET: RouteStop/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.RouteStop == null)
            {
                return NotFound();
            }

            var routeStop = await _context.RouteStop.FindAsync(id);
            if (routeStop == null)
            {
                return NotFound();
            }
            ViewData["BusRouteCode"] = new SelectList(_context.BusRoute, "BusRouteCode", "BusRouteCode", routeStop.BusRouteCode);
            ViewData["BusStopNumber"] = new SelectList(_context.BusStop, "BusStopNumber", "BusStopNumber", routeStop.BusStopNumber);
            return View(routeStop);
        }

        // POST: RouteStop/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("RouteStopId,BusRouteCode,BusStopNumber,OffsetMinutes")] RouteStop routeStop)
        {
            if (id != routeStop.RouteStopId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(routeStop);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RouteStopExists(routeStop.RouteStopId))
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
            ViewData["BusRouteCode"] = new SelectList(_context.BusRoute, "BusRouteCode", "BusRouteCode", routeStop.BusRouteCode);
            ViewData["BusStopNumber"] = new SelectList(_context.BusStop, "BusStopNumber", "BusStopNumber", routeStop.BusStopNumber);
            return View(routeStop);
        }

        // GET: RouteStop/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.RouteStop == null)
            {
                return NotFound();
            }

            var routeStop = await _context.RouteStop
                .Include(r => r.BusRouteCodeNavigation)
                .Include(r => r.BusStopNumberNavigation)
                .FirstOrDefaultAsync(m => m.RouteStopId == id);
            if (routeStop == null)
            {
                return NotFound();
            }

            return View(routeStop);
        }

        // POST: RouteStop/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.RouteStop == null)
            {
                return Problem("Entity set 'BusServiceContext.RouteStop'  is null.");
            }
            var routeStop = await _context.RouteStop.FindAsync(id);
            if (routeStop != null)
            {
                _context.RouteStop.Remove(routeStop);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RouteStopExists(int id)
        {
          return _context.RouteStop.Any(e => e.RouteStopId == id);
        }
    }
}

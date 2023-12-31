using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ThAmCo.Events.Data;

namespace ThAmCo.Events.Controllers
{
    public class StaffAssignmentsController : Controller
    {
        private readonly EventsDbContext _context;

        public StaffAssignmentsController(EventsDbContext context)
        {
            _context = context;
        }

        // GET: StaffAssignments
        public async Task<IActionResult> Index()
        {
            var eventsDbContext = _context.StaffAssignments.Include(s => s.Event).Include(s => s.Staff);
            return View(await eventsDbContext.ToListAsync());
        }

        // GET: StaffAssignments/Details/5
        public async Task<IActionResult> Details(int? eventId, int? staffId)
        {
            if (eventId == null || staffId == null)
            {
                return NotFound();
            }

            var staffAssignment = await _context.StaffAssignments
                .Where(sa => sa.EventId == eventId && sa.StaffId == staffId)
                .Include(s => s.Event)
                .Include(s => s.Staff)
                .FirstOrDefaultAsync();
            if (staffAssignment == null)
            {
                return NotFound();
            }

            return View(staffAssignment);
        }

        // GET: StaffAssignments/Create
        public IActionResult Create()
        {
            ViewData["EventId"] = new SelectList(_context.Events, "EventId", "Title");
            ViewData["StaffId"] = new SelectList(_context.Staff, "StaffId", "FirstName");
            return View();
        }

        // POST: StaffAssignments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("EventId,StaffId")] StaffAssignment staffAssignment)
        {
            if (ModelState.IsValid)
            {
                // Check if the staff member is already assigned to the event
                bool alreadyAssigned = _context.StaffAssignments.Any(sa => sa.EventId == staffAssignment.EventId && sa.StaffId == staffAssignment.StaffId);
                if (alreadyAssigned)
                {
                    // Add a model error
                    ModelState.AddModelError("", "This staff member is already assigned to this event.");
                }
                else
                {
                    _context.Add(staffAssignment);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            ViewData["EventId"] = new SelectList(_context.Events, "EventId", "Title", staffAssignment.EventId);
            ViewData["StaffId"] = new SelectList(_context.Staff, "StaffId", "FirstName", staffAssignment.StaffId);
            return View(staffAssignment);
        }

        // GET: StaffAssignments/Edit/5
        public async Task<IActionResult> Edit(int? eventId, int? staffId)
        {
            if (eventId == null || staffId == null)
            {
                return NotFound();
            }

            var staffAssignment = await _context.StaffAssignments.FindAsync(eventId, staffId);
            if (staffAssignment == null)
            {
                return NotFound();
            }
            ViewData["EventId"] = new SelectList(_context.Events, "EventId", "Title", staffAssignment.EventId);
            ViewData["StaffId"] = new SelectList(_context.Staff, "StaffId", "FirstName", staffAssignment.StaffId);
            return View(staffAssignment);
        }

        // POST: StaffAssignments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int eventId, int staffId, [Bind("EventId,StaffId")] StaffAssignment staffAssignment)
        {
            if (eventId != staffAssignment.EventId || staffId != staffAssignment.StaffId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(staffAssignment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StaffAssignmentExists(eventId, staffId))
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
            ViewData["EventId"] = new SelectList(_context.Events, "EventId", "Title", staffAssignment.EventId);
            ViewData["StaffId"] = new SelectList(_context.Staff, "StaffId", "FirstName", staffAssignment.StaffId);
            return View(staffAssignment);
        }

        // GET: StaffAssignments/Delete/5
        public async Task<IActionResult> Delete(int? eventId, int? staffId)
        {
            if (eventId == null || staffId == null)
            {
                return NotFound();
            }

            var staffAssignment = await _context.StaffAssignments
                .Where(sa => sa.EventId == eventId && sa.StaffId == staffId)
                .Include(s => s.Event)
                .Include(s => s.Staff)
                .FirstOrDefaultAsync();
            if (staffAssignment == null)
            {
                return NotFound();
            }

            return View(staffAssignment);
        }

        // POST: StaffAssignments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int eventId, int staffId)
        {
            var staffAssignment = await _context.StaffAssignments.FindAsync(eventId, staffId);
            if (staffAssignment != null)
            {
                _context.StaffAssignments.Remove(staffAssignment);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool StaffAssignmentExists(int eventId, int staffId)
        {
            return _context.StaffAssignments.Any(e => e.EventId == eventId && e.StaffId == staffId);
        }
    }
}

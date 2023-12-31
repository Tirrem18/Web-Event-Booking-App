using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ThAmCo.Events.Data;

namespace ThAmCo.Events.Controllers
{
    public class StaffAssignmentsController : Controller
    {
        private readonly EventsDbContext _context;

        // Constructor: Initializes the database context for staff assignments.
        public StaffAssignmentsController(EventsDbContext context)
        {
            _context = context;
        }

        // GET: StaffAssignments - Retrieves and displays a list of all staff assignments.
        public async Task<IActionResult> Index()
        {
            var eventsDbContext = _context.StaffAssignments.Include(s => s.Event).Include(s => s.Staff);
            return View(await eventsDbContext.ToListAsync());
        }

        // GET: StaffAssignments/Details/5 - Retrieves and displays details of a specific staff assignment.
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

        // GET: StaffAssignments/Create - Displays the form for creating a new staff assignment.
        public IActionResult Create()
        {
            ViewData["EventId"] = new SelectList(_context.Events, "EventId", "Title");
            ViewData["StaffId"] = new SelectList(_context.Staff, "StaffId", "FirstName");
            return View();
        }

        // POST: StaffAssignments/Create - Handles the submission of the staff assignment creation form.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("EventId,StaffId")] StaffAssignment staffAssignment)
        {
            if (ModelState.IsValid)
            {
                // Checks if the staff member is already assigned to the event
                bool alreadyAssigned = _context.StaffAssignments.Any(sa => sa.EventId == staffAssignment.EventId && sa.StaffId == staffAssignment.StaffId);
                if (alreadyAssigned)
                {
                    // Adds a model error if staff member is already assigned
                    ModelState.AddModelError("", "This staff member is already assigned to this event.");
                }
                else
                {
                    // Adds the new staff assignment to the database and saves changes
                    _context.Add(staffAssignment);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            ViewData["EventId"] = new SelectList(_context.Events, "EventId", "Title", staffAssignment.EventId);
            ViewData["StaffId"] = new SelectList(_context.Staff, "StaffId", "FirstName", staffAssignment.StaffId);
            return View(staffAssignment);
        }

        // GET: StaffAssignments/Edit/5 - Displays the form for editing an existing staff assignment.
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

        // POST: StaffAssignments/Edit/5 - Handles the submission of the staff assignment editing form.
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
                    // Updates the staff assignment in the database and saves changes
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

        // GET: StaffAssignments/Delete/5 - Displays the confirmation form for deleting a staff assignment.
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

        // POST: StaffAssignments/Delete/5 - Handles the confirmation of staff assignment deletion.
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int eventId, int staffId)
        {
            var staffAssignment = await _context.StaffAssignments.FindAsync(eventId, staffId);
            if (staffAssignment != null)
            {
                // Removes the staff assignment from the database and saves changes
                _context.StaffAssignments.Remove(staffAssignment);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // Checks if a specific staff assignment exists based on event and staff IDs.
        private bool StaffAssignmentExists(int eventId, int staffId)
        {
            return _context.StaffAssignments.Any(e => e.EventId == eventId && e.StaffId == staffId);
        }
    }
}
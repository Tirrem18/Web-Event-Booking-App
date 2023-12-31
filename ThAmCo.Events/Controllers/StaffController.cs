using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ThAmCo.Events.Data;

namespace ThAmCo.Events.Controllers
{
    public class StaffController : Controller
    {
        private readonly EventsDbContext _context;

        // Constructor: Initializes the database context for staff operations.
        public StaffController(EventsDbContext context)
        {
            _context = context;
        }

        // GET: Staff - Retrieves and displays a list of all staff members.
        public async Task<IActionResult> Index()
        {
            return View(await _context.Staff.ToListAsync());
        }

        // GET: Staff/Details/5 - Retrieves and displays details of a specific staff member.
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Staff == null)
            {
                return NotFound();
            }

            // Retrieves staff details including their qualifications and assignments
            var staff = await _context.Staff
                .Include(s => s.StaffQualifications)
                    .ThenInclude(sq => sq.Qualification)
                .Include(s => s.StaffAssignments)
                    .ThenInclude(sa => sa.Event)
                .FirstOrDefaultAsync(m => m.StaffId == id);

            if (staff == null)
            {
                return NotFound();
            }

            return View(staff);
        }

        // GET: Staff/Create - Displays the staff creation form.
        public IActionResult Create()
        {
            ViewData["Qualifications"] = new SelectList(_context.Qualifications, "QualificationId", "Name");
            return View();
        }

        // POST: Staff/Create - Handles the submission of the staff creation form.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("StaffId,FirstName,LastName")] Staff staff, int[] selectedQualifications)
        {
            if (ModelState.IsValid)
            {
                // Adds qualifications to the staff member
                foreach (var qualificationId in selectedQualifications)
                {
                    staff.StaffQualifications.Add(new StaffQualification { QualificationId = qualificationId });
                }

                // Adds the new staff member to the database and saves changes
                _context.Add(staff);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["Qualifications"] = new SelectList(_context.Qualifications, "QualificationId", "Name");
            return View(staff);
        }

        // GET: Staff/Edit/5 - Displays the staff editing form for a specific staff member.
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Staff == null)
            {
                return NotFound();
            }

            var staff = await _context.Staff
                .Include(s => s.StaffQualifications)
                .FirstOrDefaultAsync(s => s.StaffId == id);
            if (staff == null)
            {
                return NotFound();
            }

            ViewData["Qualifications"] = new SelectList(_context.Qualifications, "QualificationId", "Name");
            ViewData["SelectedQualifications"] = staff.StaffQualifications.Select(q => q.QualificationId).ToList();
            return View(staff);
        }

        // POST: Staff/Edit/5 - Handles the submission of the staff editing form.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("FirstName,LastName")] Staff staffEditModel, int[] selectedQualifications)
        {
            if (ModelState.IsValid)
            {
                var staffToUpdate = await _context.Staff
                    .Include(s => s.StaffQualifications)
                    .FirstOrDefaultAsync(s => s.StaffId == id);

                if (staffToUpdate == null)
                {
                    return NotFound();
                }

                // Updates staff details
                staffToUpdate.FirstName = staffEditModel.FirstName;
                staffToUpdate.LastName = staffEditModel.LastName;

                // Clears existing qualifications and adds new ones
                staffToUpdate.StaffQualifications.Clear();
                foreach (var qualId in selectedQualifications)
                {
                    staffToUpdate.StaffQualifications.Add(new StaffQualification { StaffId = id, QualificationId = qualId });
                }

                try
                {
                    _context.Update(staffToUpdate);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StaffExists(id))
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
            ViewData["Qualifications"] = new SelectList(_context.Qualifications, "QualificationId", "Name");
            return View(staffEditModel);
        }

        // GET: Staff/Delete/5 - Displays the staff deletion confirmation form.
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Staff == null)
            {
                return NotFound();
            }

            var staff = await _context.Staff
                .FirstOrDefaultAsync(m => m.StaffId == id);
            if (staff == null)
            {
                return NotFound();
            }

            return View(staff);
        }

        // POST: Staff/Delete/5 - Handles the confirmation of staff deletion.
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Staff == null)
            {
                return Problem("Entity set 'EventsDbContext.Staff' is null.");
            }
            var staff = await _context.Staff.FindAsync(id);
            if (staff != null)
            {
                _context.Staff.Remove(staff);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // Checks if a specific staff member exists based on their ID.
        private bool StaffExists(int id)
        {
            return _context.Staff.Any(e => e.StaffId == id);
        }
    }
}

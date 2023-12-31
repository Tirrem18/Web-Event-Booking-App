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
    public class StaffController : Controller
    {
        private readonly EventsDbContext _context;

        public StaffController(EventsDbContext context)
        {
            _context = context;
        }

        // GET: Staff
        public async Task<IActionResult> Index()
        {
              return View(await _context.Staff.ToListAsync());
        }

        // GET: Staff/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Staff == null)
            {
                return NotFound();
            }

            var staff = await _context.Staff
                .Include(s => s.StaffQualifications)
                    .ThenInclude(sq => sq.Qualification)
                .Include(s => s.StaffAssignments) // Include StaffAssignments
                    .ThenInclude(sa => sa.Event) // Include the Event for each StaffAssignment
                .FirstOrDefaultAsync(m => m.StaffId == id);

            if (staff == null)
            {
                return NotFound();
            }

            return View(staff);
        }

        // GET: Staff/Create
        public IActionResult Create()
        {
            ViewData["Qualifications"] = new SelectList(_context.Qualifications, "QualificationId", "Name");
            return View();
        }

        // POST: Staff/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("StaffId,FirstName,LastName")] Staff staff, int[] selectedQualifications)
        {
            if (ModelState.IsValid)
            {
                foreach (var qualificationId in selectedQualifications)
                {
                    staff.StaffQualifications.Add(new StaffQualification { QualificationId = qualificationId });
                }

                _context.Add(staff);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            // Re-populate the qualifications in case of returning to the form
            ViewData["Qualifications"] = new SelectList(_context.Qualifications, "QualificationId", "Name");
            return View(staff);
        }

        // GET: Staff/Edit/5
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

        // POST: Staff/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
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

                staffToUpdate.FirstName = staffEditModel.FirstName;
                staffToUpdate.LastName = staffEditModel.LastName;

                // Clear existing qualifications
                staffToUpdate.StaffQualifications.Clear();

                // Add new qualifications
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
            // Populate qualifications for the view
            ViewData["Qualifications"] = new SelectList(_context.Qualifications, "QualificationId", "Name");
            return View(staffEditModel);
        }

        // GET: Staff/Delete/5
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

        // POST: Staff/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Staff == null)
            {
                return Problem("Entity set 'EventsDbContext.Staff'  is null.");
            }
            var staff = await _context.Staff.FindAsync(id);
            if (staff != null)
            {
                _context.Staff.Remove(staff);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool StaffExists(int id)
        {
          return _context.Staff.Any(e => e.StaffId == id);
        }
    }
}

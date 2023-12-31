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
    public class GuestsController : Controller
    {
        private readonly EventsDbContext _context;

        // Constructor: Initializes the database context for guest operations.
        public GuestsController(EventsDbContext context)
        {
            _context = context;
        }

        // GET: Guests - Retrieves and displays a list of all guests.
        public async Task<IActionResult> Index()
        {
            return View(await _context.Guests.ToListAsync());
        }

        // GET: Guests/Details/5 - Retrieves and displays details of a specific guest.
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Guests == null)
            {
                return NotFound();
            }

            // Retrieves guest details including associated bookings
            var guest = await _context.Guests
                .Include(g => g.Bookings)
                .ThenInclude(b => b.Event)
                .FirstOrDefaultAsync(m => m.GuestId == id);
            if (guest == null)
            {
                return NotFound();
            }

            return View(guest);
        }

        // GET: Guests/Create - Displays the guest creation form.
        public IActionResult Create()
        {
            return View();
        }

        // POST: Guests/Create - Handles the guest creation form submission.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("GuestId,FirstName,LastName,Email,PhoneNumber")] Guest guest)
        {
            if (ModelState.IsValid)
            {
                // Adds the new guest to the database and saves changes
                _context.Add(guest);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(guest);
        }

        // GET: Guests/Edit/5 - Displays the guest editing form for a specific guest.
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Guests == null)
            {
                return NotFound();
            }

            var guest = await _context.Guests.FindAsync(id);
            if (guest == null)
            {
                return NotFound();
            }
            return View(guest);
        }

        // POST: Guests/Edit/5 - Handles the guest editing form submission.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("GuestId,FirstName,LastName,Email,PhoneNumber")] Guest guest)
        {
            if (id != guest.GuestId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Updates the guest details in the database and saves changes
                    _context.Update(guest);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GuestExists(guest.GuestId))
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
            return View(guest);
        }

        // GET: Guests/Delete/5 - Displays the guest deletion confirmation form.
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Guests == null)
            {
                return NotFound();
            }

            var guest = await _context.Guests
                .FirstOrDefaultAsync(m => m.GuestId == id);
            if (guest == null)
            {
                return NotFound();
            }

            return View(guest);
        }

        // POST: Guests/Delete/5 - Handles the confirmation of guest deletion.
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Guests == null)
            {
                return Problem("Entity set 'EventsDbContext.Guests' is null.");
            }
            var guest = await _context.Guests.FindAsync(id);
            if (guest != null)
            {
                // Removes the guest from the database and saves changes
                _context.Guests.Remove(guest);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // Checks if a specific guest exists based on their ID.
        private bool GuestExists(int id)
        {
            return _context.Guests.Any(e => e.GuestId == id);
        }

        // POST: Guests/Anonymise - Handles the anonymization of guest data.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Anonymise(int id)
        {
            var guest = await _context.Guests.FindAsync(id);
            if (guest == null)
            {
                return NotFound();
            }

            // Anonymizes the guest's data
            guest.FirstName = "Anonymous";
            guest.LastName = "Anonymous";
            guest.Email = "anonymous@example.com";
            guest.PhoneNumber = "0000000000";

            try
            {
                // Updates the anonymized guest details in the database and saves changes
                _context.Update(guest);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!GuestExists(guest.GuestId))
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
    }
}
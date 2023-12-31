using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ThAmCo.Events.Data;

namespace ThAmCo.Events.Controllers
{
    public class BookingsController : Controller
    {
        private readonly EventsDbContext _context;

        // Constructor: Initializes the database context.
        public BookingsController(EventsDbContext context)
        {
            _context = context;
        }

        // GET: Bookings - Retrieves a list of bookings.
        public async Task<IActionResult> Index()
        {
            // Includes related Event and Guest data in the query.
            var eventsDbContext = _context.Bookings.Include(b => b.Event).Include(b => b.Guest);
            return View(await eventsDbContext.ToListAsync());
        }

        // GET: Bookings/Details/5 - Retrieves details of a specific booking.
        public async Task<IActionResult> Details(int? id)
        {
            // Checks if the booking ID is valid.
            if (id == null || _context.Bookings == null)
            {
                return NotFound();
            }

            // Includes related Event and Guest data in the query and finds the specific booking.
            var booking = await _context.Bookings
                .Include(b => b.Event)
                .Include(b => b.Guest)
                .FirstOrDefaultAsync(m => m.BookingId == id);

            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }

        // GET: Bookings/Create - Displays the create booking form.
        public IActionResult Create()
        {
            // Prepares the Event dropdown list.
            ViewData["EventId"] = new SelectList(_context.Events, "EventId", "Title");

            // Prepares the Guest dropdown list with formatted full names and emails.
            var guestsSelectList = _context.Guests
                .Select(g => new {
                    GuestId = g.GuestId,
                    FullName = g.FirstName + " " + g.LastName + "   |  " + g.Email
                })
                .ToList();

            ViewData["GuestId"] = new SelectList(guestsSelectList, "GuestId", "FullName");
            return View();
        }

        // POST: Bookings/Create - Creates a new booking.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BookingId,GuestId,EventId,IsAttending")] Booking booking)
        {
            if (ModelState.IsValid)
            {
                // Checks for existing booking for the same guest and event.
                bool alreadyBooked = _context.Bookings.Any(b => b.EventId == booking.EventId && b.GuestId == booking.GuestId);
                if (alreadyBooked)
                {
                    ModelState.AddModelError("", "This guest is already booked for this event.");
                }
                else
                {
                    // Adds the new booking to the context and saves changes.
                    _context.Add(booking);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }

            // Prepares the Event and Guest dropdown lists again in case of a failed submission.
            ViewData["EventId"] = new SelectList(_context.Events, "EventId", "Title");
            var guestsSelectList = _context.Guests
                .Select(g => new {
                    GuestId = g.GuestId,
                    FullName = g.FirstName + " " + g.LastName + "   |  " + g.Email
                })
                .ToList();
            ViewData["GuestId"] = new SelectList(guestsSelectList, "GuestId", "FullName");

            return View(booking);
        }

        // GET: Bookings/Edit/5 - Displays the edit booking form.
        public async Task<IActionResult> Edit(int? id)
        {
            // Checks if the booking ID is valid.
            if (id == null || _context.Bookings == null)
            {
                return NotFound();
            }

            // Finds the specific booking to edit.
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null)
            {
                return NotFound();
            }

            // Prepares the Event and Guest dropdown lists for editing.
            ViewData["EventId"] = new SelectList(_context.Events, "EventId", "Title");
            var guestsSelectList = _context.Guests
                .Select(g => new {
                    GuestId = g.GuestId,
                    FullName = g.FirstName + " " + g.LastName + "   |  " + g.Email
                })
                .ToList();
            ViewData["GuestId"] = new SelectList(guestsSelectList, "GuestId", "FullName");

            return View(booking);
        }

        // POST: Bookings/Edit/5 - Submits the changes to an existing booking.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("BookingId,GuestId,EventId,IsAttending")] Booking booking)
        {
            // Checks if the provided ID matches the booking ID.
            if (id != booking.BookingId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // Checks for existing booking for the same guest and event, excluding the current booking.
                bool alreadyBooked = _context.Bookings.Any(b => b.EventId == booking.EventId && b.GuestId == booking.GuestId && b.BookingId != id);
                if (alreadyBooked)
                {
                    ModelState.AddModelError("", "This guest is already booked for this event.");
                }
                else
                {
                    // Updates the booking in the context and saves changes.
                    _context.Update(booking);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }

            // Prepares the Event and Guest dropdown lists again in case of a failed submission.
            ViewData["EventId"] = new SelectList(_context.Events, "EventId", "Title");
            var guestsSelectList = _context.Guests
                .Select(g => new {
                    GuestId = g.GuestId,
                    FullName = g.FirstName + " " + g.LastName + "   |  " + g.Email
                })
                .ToList();
            ViewData["GuestId"] = new SelectList(guestsSelectList, "GuestId", "FullName");

            return View(booking);
        }

        // GET: Bookings/Delete/5 - Displays the delete confirmation for a booking.
        public async Task<IActionResult> Delete(int? id)
        {
            // Checks if the booking ID is valid.
            if (id == null || _context.Bookings == null)
            {
                return NotFound();
            }

            // Includes related Event and Guest data in the query and finds the specific booking to delete.
            var booking = await _context.Bookings
                .Include(b => b.Event)
                .Include(b => b.Guest)
                .FirstOrDefaultAsync(m => m.BookingId == id);
            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }

        // POST: Bookings/Delete/5 - Confirms the deletion of a booking.
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Checks if the booking exists in the context.
            if (_context.Bookings == null)
            {
                return Problem("Entity set 'EventsDbContext.Bookings'  is null.");
            }

            // Finds and removes the booking, then saves the changes.
            var booking = await _context.Bookings.FindAsync(id);
            if (booking != null)
            {
                _context.Bookings.Remove(booking);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // Checks if a specific booking exists based on ID.
        private bool BookingExists(int id)
        {
            return _context.Bookings.Any(e => e.BookingId == id);
        }
    }
}

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
    public class BookingsController : Controller
    {
        private readonly EventsDbContext _context;

        public BookingsController(EventsDbContext context)
        {
            _context = context;
        }

        // GET: Bookings
        public async Task<IActionResult> Index()
        {
            var eventsDbContext = _context.Bookings.Include(b => b.Event).Include(b => b.Guest);
            return View(await eventsDbContext.ToListAsync());
        }

        // GET: Bookings/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Bookings == null)
            {
                return NotFound();
            }

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

        // GET: Bookings/Create
        public IActionResult Create()
        {
            ViewData["EventId"] = new SelectList(_context.Events, "EventId", "Title");

            var guestsSelectList = _context.Guests
                .Select(g => new {
                    GuestId = g.GuestId,
                    FullName = g.FirstName + " " + g.LastName + "   |  " + g.Email
                })
                .ToList();

            ViewData["GuestId"] = new SelectList(guestsSelectList, "GuestId", "FullName");
            return View();
        }

        // POST: Bookings/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BookingId,GuestId,EventId,IsAttending")] Booking booking)
        {
            if (ModelState.IsValid)
            {
                // Check if the guest is already booked for the event
                bool alreadyBooked = _context.Bookings.Any(b => b.EventId == booking.EventId && b.GuestId == booking.GuestId);
                if (alreadyBooked)
                {
                    // Add a model error
                    ModelState.AddModelError("", "This guest is already booked for this event.");
                }
                else
                {
                    _context.Add(booking);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
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

        // GET: Bookings/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Bookings == null)
            {
                return NotFound();
            }

            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null)
            {
                return NotFound();
            }
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

        // POST: Bookings/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("BookingId,GuestId,EventId,IsAttending")] Booking booking)
        {
            if (id != booking.BookingId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // Check if the guest is already booked for the event
                bool alreadyBooked = _context.Bookings.Any(b => b.EventId == booking.EventId && b.GuestId == booking.GuestId && b.BookingId != id);
                if (alreadyBooked)
                {
                    // Add a model error
                    ModelState.AddModelError("", "This guest is already booked for this event.");
                }
                else
                {
                    _context.Update(booking);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
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

        // GET: Bookings/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Bookings == null)
            {
                return NotFound();
            }

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

        // POST: Bookings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Bookings == null)
            {
                return Problem("Entity set 'EventsDbContext.Bookings'  is null.");
            }
            var booking = await _context.Bookings.FindAsync(id);
            if (booking != null)
            {
                _context.Bookings.Remove(booking);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BookingExists(int id)
        {
          return _context.Bookings.Any(e => e.BookingId == id);
        }
    }
}

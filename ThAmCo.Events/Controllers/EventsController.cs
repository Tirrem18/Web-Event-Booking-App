using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ThAmCo.Events.Data;
using ThAmCo.Events.Models;

namespace ThAmCo.Events.Controllers
{
    public class EventsController : Controller
    {
        private readonly EventsDbContext _context;

        public EventsController(EventsDbContext context)
        {
            _context = context;
        }

        // GET: Events
        public async Task<IActionResult> Index()
        {
              return View(await _context.Events.ToListAsync());
        }

        // GET: Events/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Events == null)
            {
                return NotFound();
            }

            var @event = await _context.Events
                .Include(e => e.Bookings)
                .ThenInclude(b => b.Guest)
                .FirstOrDefaultAsync(m => m.EventId == id);
            if (@event == null)
            {
                return NotFound();
            }

            return View(@event);
        }
        // GET: Events/InitialCreate
        public async Task<IActionResult> InitialCreate()
        {
            ViewData["EventTypes"] = new SelectList(await FetchEventTypes(), "Id", "Title");
            return View(new InitialCreateViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckAvailability(InitialCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Fetch event types again to repopulate the dropdown list
                ViewData["EventTypes"] = new SelectList(await FetchEventTypes(), "Id", "Title", model.EventTypeId);
                return View("InitialCreate", model); // Re-render the same view with errors
            }


            var availableVenues = await GetAvailableVenuesFromAPI(model.EventTypeId, model.BeginDate, model.EndDate);

            var pickVenueModel = new PickVenueViewModel
            {
                EventTypeId = model.EventTypeId,
                BeginDate = model.BeginDate,
                EndDate = model.EndDate,
                AvailableVenues = availableVenues
            };
            return View("PickVenue", pickVenueModel);
        }



        // GET: Events/Create
        public async Task<IActionResult> Create()
        {
            List<EventTypeDTO> eventTypes = new List<EventTypeDTO>();
            using (var httpClient = new HttpClient())
            {
                // Replace with your Venues API URL
                string apiUrl = "https://localhost:7088/api/eventtypes";
                var response = await httpClient.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    string apiResponse = await response.Content.ReadAsStringAsync();
                    eventTypes = JsonConvert.DeserializeObject<List<EventTypeDTO>>(apiResponse);
                }
            }

            ViewData["EventTypes"] = new SelectList(eventTypes, "Id", "Title");
            return View();
        }


        // POST: Events/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("EventId,Title,EventTypeId")] Event @event)

        {
            Console.WriteLine("Title: " + @event.Title);
            Console.WriteLine("EventTypeId: " + @event.EventTypeId);

            if (ModelState.IsValid)
            {
                _context.Add(@event);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            List<EventTypeDTO> eventTypes = new List<EventTypeDTO>();
            // ... code to fetch event types from API ...
            ViewData["EventTypes"] = new SelectList(eventTypes, "Id", "Title", @event.EventTypeId);

            return View(@event);
        }

        // GET: Events/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var eventModel = await _context.Events.FindAsync(id);
            if (eventModel == null)
            {
                return NotFound();
            }

            var viewModel = new EventEditViewModel
            {
                EventId = eventModel.EventId,
                Title = eventModel.Title
            };

            return View(viewModel);
        }

        // POST: Events/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EventEditViewModel viewModel)
        {
            if (id != viewModel.EventId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var eventToUpdate = await _context.Events.FindAsync(id);
                if (eventToUpdate == null)
                {
                    return NotFound();
                }

                eventToUpdate.Title = viewModel.Title;
                // Other properties are not updated

                try
                {
                    _context.Update(eventToUpdate);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EventExists(viewModel.EventId))
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
            return View(viewModel);
        }

        // GET: Events/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Events == null)
            {
                return NotFound();
            }

            var @event = await _context.Events
                .FirstOrDefaultAsync(m => m.EventId == id);
            if (@event == null)
            {
                return NotFound();
            }

            return View(@event);
        }

        // POST: Events/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Events == null)
            {
                return Problem("Entity set 'EventsDbContext.Events'  is null.");
            }
            var @event = await _context.Events.FindAsync(id);
            if (@event != null)
            {
                _context.Events.Remove(@event);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool EventExists(int id)
        {
          return _context.Events.Any(e => e.EventId == id);
        }

        private async Task<List<EventTypeDTO>> FetchEventTypes()
        {
            List<EventTypeDTO> eventTypes = new List<EventTypeDTO>();
            using (var httpClient = new HttpClient())
            {
                string apiUrl = "https://localhost:7088/api/eventtypes";
                var response = await httpClient.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    string apiResponse = await response.Content.ReadAsStringAsync();
                    eventTypes = JsonConvert.DeserializeObject<List<EventTypeDTO>>(apiResponse);
                }
            }
            return eventTypes;
        }

        private async Task<List<VenueAvailabilityDTO>> GetAvailableVenuesFromAPI(string eventType, DateTime beginDate, DateTime endDate)
        {
            List<VenueAvailabilityDTO> availableVenues = new List<VenueAvailabilityDTO>();
            using (var httpClient = new HttpClient())
            {
                string apiUrl = $"https://localhost:7088/api/availability?eventType={eventType}&beginDate={beginDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}";
                var response = await httpClient.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    string apiResponse = await response.Content.ReadAsStringAsync();
                    availableVenues = JsonConvert.DeserializeObject<List<VenueAvailabilityDTO>>(apiResponse);
                }
            }
            return availableVenues;
        }

    }
}

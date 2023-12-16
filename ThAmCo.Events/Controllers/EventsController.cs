using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ThAmCo.Events.Data;
using ThAmCo.Events.Models;
using ThAmCo.Events.Views.Events;

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
        public async Task<IActionResult> InitialCreate(InitialCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewData["EventTypes"] = new SelectList(await FetchEventTypes(), "Id", "Title", model.EventTypeId);
                return View(model); // Re-render the same view with errors
            }

            var availableVenues = await GetAvailableVenuesFromAPI(model.EventTypeId, model.BeginDate, model.EndDate);
            if (availableVenues.Any())
            {
                // Pass data to PickVenue (GET)
                return RedirectToAction("PickVenue", new { model.EventTypeId, model.BeginDate, model.EndDate });
            }
            else
            {
                ViewData["ErrorMessage"] = "No venues are available for the selected dates and event type.";
                ViewData["EventTypes"] = new SelectList(await FetchEventTypes(), "Id", "Title", model.EventTypeId);
                return View(model);
            }
        }

        // GET: Events/PickVenue
        public async Task<IActionResult> PickVenue(string eventTypeId, DateTime beginDate, DateTime endDate)
        {
            var availableVenues = await GetAvailableVenuesFromAPI(eventTypeId, beginDate, endDate);
            var pickVenueModel = new PickVenueViewModel
            {
                EventTypeId = eventTypeId,
                BeginDate = beginDate,
                EndDate = endDate,
                AvailableVenues = availableVenues
            };
            return View(pickVenueModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PickVenue(PickVenueViewModel model)
        {
            if (string.IsNullOrEmpty(model.SelectedVenue))
            {
                // No venue and date combination selected, show an error message
                ViewData["ErrorMessage"] = "No venue has been selected. Please select a venue to proceed";

                // Repopulate the available venues
                model.AvailableVenues = await GetAvailableVenuesFromAPI(model.EventTypeId, model.BeginDate, model.EndDate);
                return View(model);
            }
            else
            {
                // Split the SelectedVenue into VenueCode and Date
                var parts = model.SelectedVenue.Split('|');
                if (parts.Length == 2)
                {
                    var selectedVenueCode = parts[0];
                    var selectedDate = DateTime.Parse(parts[1]);
                    string eventType = model.EventTypeId;

                    return RedirectToAction("Create", new { selectedVenueCode, selectedDate, eventType });
                }
                else
                {
                    // If the selected venue string is not in the correct format, show an error
                    ViewData["ErrorMessage"] = "Invalid venue selection. Please try again.";
                    model.AvailableVenues = await GetAvailableVenuesFromAPI(model.EventTypeId, model.BeginDate, model.EndDate);
                    return View(model);
                }
            }
        }



        // GET: Events/Create
        public IActionResult Create(string selectedVenueCode, DateTime selectedDate, string eventType)
        {
            var viewModel = new Event
            {
                SelectedVenueCode = selectedVenueCode,
                SelectedDate = selectedDate,
                EventTypeId = eventType,
            };

            // You can also pass any additional data needed for the form
            // For example, if you need a list of event types
            // ViewData["EventTypes"] = new SelectList(...);

            return View(viewModel);
        }


        // POST: Events/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title, SelectedVenueCode, SelectedDate, EventTypeId")] Event @event)
        {
            if (ModelState.IsValid)
            {
                // Add the new event to the context
                _context.Add(@event);
                await _context.SaveChangesAsync();

                // Redirect to the index action/view after creating the event
                return RedirectToAction(nameof(Index));
            }

            // If the model state is not valid, re-render the create view with the current model
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

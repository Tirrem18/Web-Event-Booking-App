using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Text;
using ThAmCo.Events.Data;
using ThAmCo.Events.Models;
using ThAmCo.Venues.Models;

namespace ThAmCo.Events.Controllers
{
    public class EventsController : Controller
    {
        private readonly EventsDbContext _context;

        // Constructor: Initializes the database context for event operations.
        public EventsController(EventsDbContext context)
        {
            _context = context;
        }

        // GET: Events - Retrieves and displays a list of all events.
        public async Task<IActionResult> Index()
        {
            return View(await _context.Events.ToListAsync());
        }

        // GET: Events/Details/5 - Retrieves and displays details of a specific event.
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Events == null)
            {
                return NotFound();
            }

            // Retrieves event details including associated bookings and staff assignments
            var @event = await _context.Events
                .Include(e => e.Bookings)
                .ThenInclude(b => b.Guest)
                .Include(e => e.StaffAssignments)
                .ThenInclude(sa => sa.Staff)
                .FirstOrDefaultAsync(m => m.EventId == id);

            if (@event == null)
            {
                return NotFound();
            }

            return View(@event);
        }

        // GET: Events/InitialCreate - Displays the initial event creation form with event types.
        public async Task<IActionResult> InitialCreate()
        {
            ViewData["EventTypes"] = new SelectList(await FetchEventTypes(), "Id", "Title");
            return View(new InitialCreateViewModel());
        }

        // POST: Events/InitialCreate - Handles the initial event creation form submission.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> InitialCreate(InitialCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewData["EventTypes"] = new SelectList(await FetchEventTypes(), "Id", "Title", model.EventTypeId);
                return View(model); // Return form with validation errors.
            }

            // Checks for available venues based on the event type and dates
            var availableVenues = await GetAvailableVenuesFromAPI(model.EventTypeId, model.BeginDate, model.EndDate);
            if (availableVenues.Any())
            {
                return RedirectToAction("PickVenue", new { model.EventTypeId, model.BeginDate, model.EndDate });
            }
            else
            {
                // Display error message if no venues are available
                ViewData["ErrorMessage"] = "No venues are available for the selected dates and event type.";
                ViewData["EventTypes"] = new SelectList(await FetchEventTypes(), "Id", "Title", model.EventTypeId);
                return View(model); // Return form with error message.
            }
        }

        // GET: Events/PickVenue - Displays the form for selecting an available venue.
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

        // POST: Events/PickVenue - Handles the venue selection form submission.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PickVenue(PickVenueViewModel model)
        {
            if (string.IsNullOrEmpty(model.SelectedVenue))
            {
                // Display error message if no venue is selected
                ViewData["ErrorMessage"] = "No venue has been selected. Please select a venue to proceed";
                model.AvailableVenues = await GetAvailableVenuesFromAPI(model.EventTypeId, model.BeginDate, model.EndDate);
                return View(model); // Return form with validation error.
            }
            else
            {
                // Process the selected venue and redirect to event creation
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
                    // Display error message for invalid venue selection
                    ViewData["ErrorMessage"] = "Invalid venue selection. Please try again.";
                    model.AvailableVenues = await GetAvailableVenuesFromAPI(model.EventTypeId, model.BeginDate, model.EndDate);
                    return View(model); // Return form with error message for invalid selection.
                }
            }
        }

        // GET: Events/Create - Displays the final event creation form.
        public IActionResult Create(string selectedVenueCode, DateTime selectedDate, string eventType)
        {
            var viewModel = new Event
            {
                SelectedVenueCode = selectedVenueCode,
                SelectedDate = selectedDate,
                EventTypeId = eventType,
            };

            return View(viewModel);
        }

        // POST: Events/Create - Handles the final event creation form submission.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title, SelectedVenueCode, SelectedDate, EventTypeId")] Event @event)
        {
            if (ModelState.IsValid)
            {
                // Example staff ID used for creating a reservation
                string staffId = "1";

                // Creates a reservation and processes the event
                string reservationReference = await CreateReservationAsync(@event.SelectedVenueCode, @event.SelectedDate, staffId);
                if (reservationReference == null)
                {
                    // Display error message if reservation can't be booked
                    ViewData["ErrorMessage"] = "Sorry, we can't book this reservation. Please try another venue.";
                    return View(@event);
                }

                // Adds event details including reservation reference and saves to the database
                @event.Reference = reservationReference;
                _context.Add(@event);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(@event); // Return form with validation errors.
        }

        // GET: Events/Edit/5 - Displays the event editing form for a specific event.
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

            // Initializes the event editing model
            var viewModel = new EventEditViewModel
            {
                EventId = eventModel.EventId,
                Title = eventModel.Title
            };

            return View(viewModel);
        }

        // POST: Events/Edit/5 - Handles the event editing form submission.
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

                // Updates the event title
                eventToUpdate.Title = viewModel.Title;

                try
                {
                    // Saves the updated event to the database
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
            return View(viewModel); // Return form with validation errors.
        }

        // POST: Events/Delete/5 - Handles the confirmation of event deletion.
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Events == null)
            {
                return Problem("Entity set 'EventsDbContext.Events' is null.");
            }
            var @event = await _context.Events.FindAsync(id);
            if (@event != null)
            {
                // Deletes the reservation associated with the event if it exists.
                if (!string.IsNullOrEmpty(@event.Reference))
                {
                    var reservationDeleted = await DeleteReservationAsync(@event.Reference);
                    if (!reservationDeleted)
                    {
                        ViewBag.ErrorMessage = "An error occurred while deleting the reservation. Please try again later.";
                    }
                }

                // Removes the event from the database and saves changes.
                _context.Events.Remove(@event);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // Checks if a specific event exists based on its ID.
        private bool EventExists(int id)
        {
            return _context.Events.Any(e => e.EventId == id);
        }

        // Fetches event types from an external API.
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

        // Gets a list of available venues from an API based on event type and date range.
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

        // Creates a reservation for an event using an external API.
        private async Task<string> CreateReservationAsync(string venueCode, DateTime eventDate, string staffId)
        {
            var reservationDto = new ReservationPostDto
            {
                EventDate = eventDate,
                VenueCode = venueCode,
                StaffId = staffId
            };

            using (var httpClient = new HttpClient())
            {
                string apiUrl = "https://localhost:7088/api/reservations";
                var content = new StringContent(JsonConvert.SerializeObject(reservationDto), Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync(apiUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var createdReservation = JsonConvert.DeserializeObject<ReservationGetDto>(responseContent);
                    return createdReservation.Reference; // Returns the reservation reference.
                }
                else
                {
                    return null; // Returns null if the creation was unsuccessful.
                }
            }
        }

        // Deletes a reservation using an external API.
        private async Task<bool> DeleteReservationAsync(string reservationReference)
        {
            using (var httpClient = new HttpClient())
            {
                string apiUrl = $"https://localhost:7088/api/reservations/{reservationReference}";
                var response = await httpClient.DeleteAsync(apiUrl);

                return response.IsSuccessStatusCode; // Returns true if deletion was successful, false otherwise.
            }
        }

    }
}

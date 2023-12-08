using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ThAmCo.Catering.Data;
using ThAmCo.Catering.Models;

namespace ThAmCo.Catering.Controllers
{
    /// <summary>
    /// Manages food bookings.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class FoodBookingController : ControllerBase
    {
        private readonly CateringDbContext _context;

        // Constructor to inject the database context
        public FoodBookingController(CateringDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves all food bookings.
        /// </summary>
        /// <returns>A list of food bookings.</returns>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetAllFoodBookings()
        {
            // Fetching all bookings with associated menu names
            var foodBookings = await _context.FoodBookings
                .Select(fb => new
                {
                    fb.FoodBookingId,
                    fb.ClientReferenceId,
                    fb.NumberOfGuests,
                    fb.MenuId,
                    MenuName = fb.Menu.MenuName
                })
                .ToListAsync();

            // Returning the list of bookings
            return Ok(foodBookings);
        }

        /// <summary>
        /// Creates a new food booking.
        /// </summary>
        /// <param name="foodBookingDto">The food booking data transfer object.</param>
        /// <returns>The created food booking.</returns>
        [HttpPost]
        public async Task<ActionResult<FoodBooking>> BookFood([FromBody] FoodBookingDTO foodBookingDto)
        {
            // Validating the incoming food booking data
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Ensuring the referenced menu exists
            bool menuExists = await _context.Menus.AnyAsync(m => m.MenuId == foodBookingDto.MenuId);
            if (!menuExists)
            {
                return BadRequest(new { message = "Invalid menu ID: No menu found." });
            }

            // Creating and saving the new booking
            var foodBooking = new FoodBooking
            {
                ClientReferenceId = foodBookingDto.ClientReferenceId,
                NumberOfGuests = foodBookingDto.NumberOfGuests,
                MenuId = foodBookingDto.MenuId
            };

            _context.FoodBookings.Add(foodBooking);
            await _context.SaveChangesAsync();

            // Returning the newly created booking
            return CreatedAtAction(nameof(GetFoodBooking), new { id = foodBooking.FoodBookingId }, foodBooking);
        }

        /// <summary>
        /// Retrieves a specific food booking by ID.
        /// </summary>
        /// <param name="id">The ID of the food booking.</param>
        /// <returns>The requested food booking.</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetFoodBooking(int id)
        {
            // Fetching a single booking by ID with menu name
            var foodBooking = await _context.FoodBookings
                .Where(fb => fb.FoodBookingId == id)
                .Select(fb => new
                {
                    fb.FoodBookingId,
                    fb.ClientReferenceId,
                    fb.NumberOfGuests,
                    fb.MenuId,
                    MenuName = fb.Menu.MenuName
                })
                .SingleOrDefaultAsync();

            // Checking if the booking exists
            if (foodBooking == null)
            {
                return NotFound();
            }

            // Returning the found booking
            return foodBooking;
        }

        /// <summary>
        /// Updates a specific food booking (Client Reference, Number of Guests, Menu).
        /// </summary>
        /// <param name="id">The ID of the food booking to update.</param>
        /// <param name="foodBookingDto">The updated food booking data.</param>
        /// <returns>A status indicating the success or failure of the update.</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateFoodBooking(int id, [FromBody] FoodBookingDTO foodBookingDto)
        {
            // Validating the updated data
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Finding the booking to update
            var existingFoodBooking = await _context.FoodBookings.FindAsync(id);
            if (existingFoodBooking == null)
            {
                return NotFound();
            }

            // Ensuring the new menu exists
            var menuExists = await _context.Menus.AnyAsync(m => m.MenuId == foodBookingDto.MenuId);
            if (!menuExists)
            {
                return BadRequest(new { message = "Invalid menu ID: No menu found." });
            }

            // Updating the booking details
            existingFoodBooking.ClientReferenceId = foodBookingDto.ClientReferenceId;
            existingFoodBooking.NumberOfGuests = foodBookingDto.NumberOfGuests;
            existingFoodBooking.MenuId = foodBookingDto.MenuId;

            // Saving the changes
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FoodBookingExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            // Returning no content after successful update
            return NoContent();
        }

        /// <summary>
        /// Cancels a specific food booking.
        /// </summary>
        /// <param name="id">The ID of the food booking to cancel.</param>
        /// <returns>A status indicating the success or failure of the cancellation.</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> CancelFoodBooking(int id)
        {
            // Finding the booking to cancel
            var foodBooking = await _context.FoodBookings.FindAsync(id);

            // Checking if the booking exists
            if (foodBooking == null)
            {
                return NotFound();
            }

            // Removing the booking and saving changes
            _context.FoodBookings.Remove(foodBooking);
            await _context.SaveChangesAsync();

            // Returning no content after successful deletion
            return NoContent();
        }

        // Helper method to check if a food booking exists by ID
        private bool FoodBookingExists(int id)
        {
            return _context.FoodBookings.Any(fb => fb.FoodBookingId == id);
        }
    }
}
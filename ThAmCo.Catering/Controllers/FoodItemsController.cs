using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ThAmCo.Catering.Data;
using ThAmCo.Catering.Models;

namespace ThAmCo.Catering.Controllers
{
    /// <summary>
    /// Manages food items.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class FoodItemsController : ControllerBase
    {
        private readonly CateringDbContext _context;

        // Constructor to inject the database context
        public FoodItemsController(CateringDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves all food items.
        /// </summary>
        /// <returns>A list of all food items.</returns>
        [HttpGet(Name = "GetFoodItems")]
        public async Task<ActionResult<IEnumerable<FoodItemDTO>>> GetFoodItems()
        {
            // Fetching all food items and mapping them to DTOs
            var foodItems = await _context.FoodItems
                .Select(fi => new FoodItemDTO
                {
                    FoodItemId = fi.FoodItemId,
                    Description = fi.Description,
                    UnitPrice = fi.UnitPrice
                })
                .ToListAsync();

            // Returning the list of food items
            return Ok(foodItems);
        }

        /// <summary>
        /// Retrieves a specific food item by ID.
        /// </summary>
        /// <param name="id">The ID of the food item.</param>
        /// <returns>The requested food item.</returns>
        [HttpGet("{id}", Name = "GetFoodItem")]
        public async Task<ActionResult<FoodItemDTO>> GetFoodItem(int id)
        {
            // Fetching a single food item by ID and mapping it to DTO
            var foodItemDto = await _context.FoodItems
                .Where(fi => fi.FoodItemId == id)
                .Select(fi => new FoodItemDTO
                {
                    FoodItemId = fi.FoodItemId,
                    Description = fi.Description,
                    UnitPrice = fi.UnitPrice
                })
                .SingleOrDefaultAsync();

            // Checking if the food item exists
            if (foodItemDto == null)
            {
                return NotFound();
            }

            // Returning the found food item
            return foodItemDto;
        }

        /// <summary>
        /// Updates a specific food item.
        /// </summary>
        /// <param name="id">The ID of the food item to update.</param>
        /// <param name="foodItemDto">The updated food item data.</param>
        /// <returns>A status indicating the success or failure of the update.</returns>
        [HttpPut("{id}", Name = "PutFoodItem")]
        public async Task<IActionResult> PutFoodItem(int id, [FromBody] FoodItemManageDTO foodItemDto)
        {
            // Finding the food item to update
            var foodItem = await _context.FoodItems.FindAsync(id);
            if (foodItem == null)
            {
                return NotFound();
            }

            // Updating the food item properties
            foodItem.Description = foodItemDto.Description;
            foodItem.UnitPrice = foodItemDto.UnitPrice;

            // Saving the updated food item
            _context.Entry(foodItem).State = EntityState.Modified;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FoodItemExists(id))
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
        /// Creates a new food item.
        /// </summary>
        /// <param name="foodItemDto">The food item data transfer object.</param>
        /// <returns>The created food item.</returns>
        [HttpPost(Name = "PostFoodItem")]
        public async Task<ActionResult<FoodItemDTO>> PostFoodItem([FromBody] FoodItemManageDTO foodItemDto)
        {
            // Creating a new food item
            var foodItem = new FoodItem
            {
                Description = foodItemDto.Description,
                UnitPrice = foodItemDto.UnitPrice
            };

            // Adding and saving the new food item
            _context.FoodItems.Add(foodItem);
            await _context.SaveChangesAsync();

            // Preparing the response DTO
            var responseDto = new FoodItemDTO
            {
                FoodItemId = foodItem.FoodItemId,
                Description = foodItem.Description,
                UnitPrice = foodItem.UnitPrice
            };

            // Returning the created food item
            return CreatedAtAction(nameof(GetFoodItem), new { id = foodItem.FoodItemId }, responseDto);
        }

        /// <summary>
        /// Deletes a specific food item.
        /// </summary>
        /// <param name="id">The ID of the food item to delete.</param>
        /// <returns>A status indicating the success or failure of the deletion.</returns>
        [HttpDelete("{id}", Name = "DeleteFoodItem")]
        public async Task<IActionResult> DeleteFoodItem(int id)
        {
            // Finding the food item to delete
            var foodItem = await _context.FoodItems.FindAsync(id);
            if (foodItem == null)
            {
                return NotFound();
            }

            // Removing and saving the changes
            _context.FoodItems.Remove(foodItem);
            await _context.SaveChangesAsync();

            // Returning no content after successful deletion
            return NoContent();
        }

        // Helper method to check if a food item exists by ID
        private bool FoodItemExists(int id)
        {
            return _context.FoodItems.Any(e => e.FoodItemId == id);
        }
    }
}
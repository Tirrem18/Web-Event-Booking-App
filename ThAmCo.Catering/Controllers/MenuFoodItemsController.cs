using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ThAmCo.Catering.Data;

namespace ThAmCo.Catering.Controllers
{
    /// <summary>
    /// Controller for managing the association between menus and food items.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class MenuFoodItemsController : ControllerBase
    {
        private readonly CateringDbContext _context;

        public MenuFoodItemsController(CateringDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Adds a food item to a specified menu.
        /// </summary>
        /// <param name="menuId">The ID of the menu.</param>
        /// <param name="foodItemId">The ID of the food item.</param>
        /// <returns>A confirmation of the addition.</returns>
        [HttpPost]
        public async Task<ActionResult> AddFoodItemToMenu(int menuId, int foodItemId)
        {
            // Check if both menu and food item exist
            if (!MenuExists(menuId))
            {
                return BadRequest($"Menu with ID {menuId} does not exist.");
            }

            if (!FoodItemExists(foodItemId))
            {
                return BadRequest($"Food item with ID {foodItemId} does not exist.");
            }

            // Check if the food item is already in the menu
            if (MenuFoodItemExists(menuId, foodItemId))
            {
                return BadRequest($"Food item with ID {foodItemId} is already in menu with ID {menuId}.");
            }

            // Create and add the new association
            var menuFoodItem = new MenuFoodItem { MenuId = menuId, FoodItemId = foodItemId };
            _context.MenuFoodItems.Add(menuFoodItem);
            await _context.SaveChangesAsync();

            return Ok(new { MenuId = menuId, FoodItemId = foodItemId });
        }

        /// <summary>
        /// Removes a food item from a specified menu.
        /// </summary>
        /// <param name="menuId">The ID of the menu.</param>
        /// <param name="foodItemId">The ID of the food item.</param>
        /// <returns>A status indicating the success or failure of the removal.</returns>
        [HttpDelete("{menuId}/{foodItemId}")]
        public async Task<ActionResult> RemoveFoodItemFromMenu(int menuId, int foodItemId)
        {
            // Find the specific association to be removed
            var menuFoodItem = await _context.MenuFoodItems
                .FirstOrDefaultAsync(mfi => mfi.MenuId == menuId && mfi.FoodItemId == foodItemId);

            // Check if the association exists
            if (menuFoodItem == null)
            {
                return NotFound($"Food item with ID {foodItemId} is not found in menu with ID {menuId}.");
            }

            // Remove the association and save changes
            _context.MenuFoodItems.Remove(menuFoodItem);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Helper method to check if a menu exists by its ID
        private bool MenuExists(int id) => _context.Menus.Any(e => e.MenuId == id);

        // Helper method to check if a food item exists by its ID
        private bool FoodItemExists(int id) => _context.FoodItems.Any(e => e.FoodItemId == id);

        // Helper method to check if a menu-food item association exists
        private bool MenuFoodItemExists(int menuId, int foodItemId) =>
            _context.MenuFoodItems.Any(mfi => mfi.MenuId == menuId && mfi.FoodItemId == foodItemId);
    }
}
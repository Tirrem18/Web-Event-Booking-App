using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ThAmCo.Catering.Data;
using ThAmCo.Catering.Models;

namespace ThAmCo.Catering.Controllers
{
    /// <summary>
    /// Manages menus.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class MenusController : ControllerBase
    {
        private readonly CateringDbContext _context;

        // Constructor to inject the database context
        public MenusController(CateringDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves all menus.
        /// </summary>
        /// <returns>A list of all menus.</returns>
        [HttpGet(Name = "GetMenus")]
        public async Task<ActionResult<IEnumerable<MenuDTO>>> GetMenus()
        {
            // Fetching all menus with their associated food items
            var menus = await _context.Menus
                .Include(m => m.MenuFoodItems)
                    .ThenInclude(mfi => mfi.FoodItem)
                .Select(m => new MenuDTO
                {
                    MenuId = m.MenuId,
                    MenuName = m.MenuName,
                    MenuFoodItems = m.MenuFoodItems.Select(mfi => new FoodItemDTO
                    {
                        FoodItemId = mfi.FoodItem.FoodItemId,
                        Description = mfi.FoodItem.Description,
                        UnitPrice = mfi.FoodItem.UnitPrice
                    }).ToList()
                })
                .ToListAsync();

            // Returning the list of menus
            return Ok(menus);
        }

        /// <summary>
        /// Retrieves a specific menu by ID.
        /// </summary>
        /// <param name="id">The ID of the menu.</param>
        /// <returns>The requested menu.</returns>
        [HttpGet("{id}", Name = "GetMenu")]
        public async Task<ActionResult<MenuDTO>> GetMenu(int id)
        {
            // Fetching a single menu by ID with associated food items
            var menu = await _context.Menus
                .Include(m => m.MenuFoodItems)
                    .ThenInclude(mfi => mfi.FoodItem)
                .Select(m => new MenuDTO
                {
                    MenuId = m.MenuId,
                    MenuName = m.MenuName,
                    MenuFoodItems = m.MenuFoodItems.Select(mfi => new FoodItemDTO
                    {
                        FoodItemId = mfi.FoodItem.FoodItemId,
                        Description = mfi.FoodItem.Description,
                        UnitPrice = mfi.FoodItem.UnitPrice
                    }).ToList()
                })
                .SingleOrDefaultAsync(m => m.MenuId == id);

            // Checking if the menu exists
            if (menu == null)
            {
                return NotFound();
            }

            // Returning the found menu
            return menu;
        }

        /// <summary>
        /// Updates a specific menu's name.
        /// </summary>
        /// <param name="id">The ID of the menu to update.</param>
        /// <param name="menuName">The new name of the menu.</param>
        /// <returns>A status indicating the success or failure of the update.</returns>
        [HttpPut("{id}", Name = "PutMenu")]
        public async Task<IActionResult> PutMenu(int id, [FromBody] string menuName)
        {
            // Check if menuName is provided
            if (string.IsNullOrWhiteSpace(menuName))
            {
                return BadRequest("Menu name is required.");
            }

            // Finding the menu to update
            var menu = await _context.Menus.FindAsync(id);
            if (menu == null)
            {
                return NotFound();
            }

            // Updating the menu's name
            menu.MenuName = menuName;
            _context.Entry(menu).State = EntityState.Modified;

            // Saving the changes
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MenuExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            // Returning no content after successful update
            return NoContent(); // Optionally, you can return the updated menu here
        }

        /// <summary>
        /// Creates a new empty menu.
        /// </summary>
        /// <param name="menuName">The name of the new menu.</param>
        /// <returns>The created menu.</returns>
        [HttpPost(Name = "PostMenu")]
        public async Task<ActionResult<MenuDTO>> PostMenu([FromBody] string menuName)
        {
            // Check if menuName is provided
            if (string.IsNullOrWhiteSpace(menuName))
            {
                return BadRequest("Menu name is required.");
            }

            // Creating a new menu
            var menu = new Menu
            {
                MenuName = menuName
            };

            // Adding and saving the new menu
            _context.Menus.Add(menu);
            await _context.SaveChangesAsync();

            // Preparing the response DTO
            var responseDto = new MenuDTO
            {
                MenuId = menu.MenuId,
                MenuName = menu.MenuName,
                MenuFoodItems = new List<FoodItemDTO>() // Initially empty
            };

            // Returning the created menu
            return CreatedAtAction(nameof(GetMenu), new { id = menu.MenuId }, responseDto);
        }

        /// <summary>
        /// Deletes a specific menu.
        /// </summary>
        /// <param name="id">The ID of the menu to delete.</param>
        /// <returns>A status indicating the success or failure of the deletion.</returns>
        [HttpDelete("{id}", Name = "DeleteMenu")]
        public async Task<IActionResult> DeleteMenu(int id)
        {
            // Finding the menu to delete
            var menu = await _context.Menus.FindAsync(id);
            if (menu == null)
            {
                return NotFound();
            }

            // Removing the menu and saving changes
            _context.Menus.Remove(menu);
            await _context.SaveChangesAsync();

            // Returning no content after successful deletion
            return NoContent();
        }

        // Helper method to check if a menu exists by ID
        private bool MenuExists(int id)
        {
            return _context.Menus.Any(e => e.MenuId == id);
        }
    }
}
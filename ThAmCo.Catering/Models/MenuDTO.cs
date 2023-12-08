using System.ComponentModel.DataAnnotations;

namespace ThAmCo.Catering.Models
{
    /// <summary>
    /// Represents the data transfer object for a menu.
    /// </summary>
    public class MenuDTO
    {
        // Unique identifier for the menu
        public int MenuId { get; set; }

        [Required(ErrorMessage = "Menu name is required.")]
        [MaxLength(100, ErrorMessage = "Menu name cannot be more than 100 characters long.")]
        // Name of the menu, required with a max length constraint
        public string MenuName { get; set; }

        // Collection of food items associated with the menu
        public List<FoodItemDTO> MenuFoodItems { get; set; }
    }
}
using System.ComponentModel.DataAnnotations;

namespace ThAmCo.Catering.Data
{
    /// <summary>
    /// Represents a menu in the catering service.
    /// </summary>
    public class Menu
    {
        // Unique identifier for the menu
        public int MenuId { get; set; }

        [Required(ErrorMessage = "Menu name is required.")]
        [MaxLength(100, ErrorMessage = "Menu name cannot be more than 100 characters long.")]
        // Name of the menu
        public string MenuName { get; set; }

        // Collection representing the many-to-many relationship with FoodItems
        public ICollection<MenuFoodItem> MenuFoodItems { get; set; }

        // Constructor to initialize the collection
        public Menu()
        {
            MenuFoodItems = new HashSet<MenuFoodItem>();
        }
    }
}
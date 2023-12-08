using System.ComponentModel.DataAnnotations;

namespace ThAmCo.Catering.Data
{
    /// <summary>
    /// Represents a food item in the catering service.
    /// </summary>
    public class FoodItem
    {
        // Unique identifier for the food item
        public int FoodItemId { get; set; }

        [Required(ErrorMessage = "Description is required.")]
        [MaxLength(200, ErrorMessage = "Description cannot be more than 200 characters long.")]
        // Description of the food item
        public string Description { get; set; }

        [Required(ErrorMessage = "Unit price is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Unit price must be greater than 0.")]
        // Price per unit of the food item
        public decimal UnitPrice { get; set; }

        // Collection representing the many-to-many relationship with Menus
        public ICollection<MenuFoodItem> MenuFoodItems { get; set; }
    }
}
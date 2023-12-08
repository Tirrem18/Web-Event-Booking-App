using System.ComponentModel.DataAnnotations;

namespace ThAmCo.Catering.Models
{
    /// <summary>
    /// Represents the data transfer object for managing a food item.
    /// </summary>
    public class FoodItemManageDTO
    {
        [Required(ErrorMessage = "Description is required.")]
        [MaxLength(200, ErrorMessage = "Description cannot be more than 200 characters long.")]
        // Description of the food item, required with a max length constraint
        public string Description { get; set; }

        [Required(ErrorMessage = "Unit price is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Unit price must be greater than 0.")]
        // Unit price of the food item, required with a minimum value constraint
        public decimal UnitPrice { get; set; }
    }
}
namespace ThAmCo.Catering.Data
{
    /// <summary>
    /// Represents a many-to-many relationship between Menu and FoodItem.
    /// </summary>
    public class MenuFoodItem
    {
        // Foreign key for Menu
        public int MenuId { get; set; }

        // Navigation property for the associated Menu
        public Menu Menu { get; set; }

        // Foreign key for FoodItem
        public int FoodItemId { get; set; }

        // Navigation property for the associated FoodItem
        public FoodItem FoodItem { get; set; }
    }
}
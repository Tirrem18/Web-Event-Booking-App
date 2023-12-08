using System.ComponentModel.DataAnnotations;

namespace ThAmCo.Catering.Data
{
    /// <summary>
    /// Represents a booking for catering services.
    /// </summary>
    public class FoodBooking
    {
        // Unique identifier for the food booking
        public int FoodBookingId { get; set; }

        [Required(ErrorMessage = "Client reference ID is required.")]
        // Reference identifier for the client
        public int ClientReferenceId { get; set; }

        [Required(ErrorMessage = "Number of guests is required.")]
        [Range(1, 1000, ErrorMessage = "Number of guests must be at least 1.")]
        // Number of guests for the food booking
        public int NumberOfGuests { get; set; } = 0;

        [Required(ErrorMessage = "Menu ID is required.")]
        // Identifier for the selected menu
        public int MenuId { get; set; }

        [Required(ErrorMessage = "Menu is required.")]
        // Navigation property for the associated menu
        public Menu Menu { get; set; }
    }
}
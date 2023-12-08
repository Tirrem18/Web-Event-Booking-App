using System.ComponentModel.DataAnnotations;

namespace ThAmCo.Catering.Models
{
    /// <summary>
    /// Represents the data transfer object for a food booking.
    /// </summary>
    public class FoodBookingDTO
    {
        [Required(ErrorMessage = "Client reference ID is required.")]
        // Unique identifier for the client making the booking
        public int ClientReferenceId { get; set; }

        [Required(ErrorMessage = "Number of guests is required.")]
        [Range(1, 1000, ErrorMessage = "Number of guests must be at least 1.")]
        // Number of guests for the booking
        public int NumberOfGuests { get; set; } = 0;

        [Required(ErrorMessage = "Menu ID is required.")]
        // Identifier for the selected menu
        public int MenuId { get; set; }
    }
}
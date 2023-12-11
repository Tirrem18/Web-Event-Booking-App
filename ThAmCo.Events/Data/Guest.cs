using System.ComponentModel.DataAnnotations;

namespace ThAmCo.Events.Data
{
    public class Guest
    {
        public int GuestId { get; set; }

        [Required, StringLength(20)]
        public string FirstName { get; set; }

        [Required, StringLength(20)]
        public string LastName { get; set; }

        [Required, EmailAddress, StringLength(100)]
        public string Email { get; set; }

        [Required, MinLength(7), MaxLength(15)] //Different countyrs phone numbers are different lengths
        public string PhoneNumber { get; set; }

        public List<Booking> Bookings { get; set; }

    }
}
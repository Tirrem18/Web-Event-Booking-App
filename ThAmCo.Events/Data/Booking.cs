using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ThAmCo.Events.Data
{
    public class Booking
    {
        [Key]
        public int BookingId { get; set; }

        [ForeignKey("Guest")]
        public int GuestId { get; set; }
        public Guest Guest { get; set; }

        [ForeignKey("Event")]
        public int EventId { get; set; }
        public Event Event { get; set; }

        public bool IsAttending { get; set; }
    }
}
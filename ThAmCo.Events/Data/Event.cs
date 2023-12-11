using System.ComponentModel.DataAnnotations;

namespace ThAmCo.Events.Data
{
    public class Event
    {
        [Key]
        public int EventId { get; set; }

        [Required, StringLength(20)]
        public string Title { get; set; }

        [Required]
         public string EventTypeId { get; set; }

        public List<Booking> Bookings { get; set; }

        // Additional properties such as Description, Duration, etc. can be added if needed

        // Navigation properties for relationships with other tables can be added here
    }
}
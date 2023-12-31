using System.ComponentModel.DataAnnotations;

namespace ThAmCo.Events.Data
{
    public class Event
    {
        [Key]
        public int EventId { get; set; }

        [Required, StringLength(20)]
        public string Title { get; set; }

        public string EventTypeId { get; set; }
        public string SelectedVenueCode { get; set; }
        public DateTime SelectedDate { get; set; }
        public string Reference { get; set; }


        public List<Booking> Bookings { get; set; }
        public List<StaffAssignment> StaffAssignments { get; set; }

    }
}
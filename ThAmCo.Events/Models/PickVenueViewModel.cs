using Microsoft.AspNetCore.Mvc.Rendering;

namespace ThAmCo.Events.Models
{
    public class PickVenueViewModel
    {
        public string EventTypeId { get; set; }
        public DateTime BeginDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<VenueAvailabilityDTO> AvailableVenues { get; set; }
        public string SelectedVenue { get; set; }

    }
}

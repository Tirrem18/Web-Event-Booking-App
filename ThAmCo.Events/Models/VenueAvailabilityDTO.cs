namespace ThAmCo.Events.Models
{
    public class VenueAvailabilityDTO
    {
        public string VenueCode { get; set; }
        public string Name { get; set; }
        public DateTime Date { get; set; }
        public double CostPerHour { get; set; }
    }
}

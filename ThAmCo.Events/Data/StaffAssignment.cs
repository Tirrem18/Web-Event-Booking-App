namespace ThAmCo.Events.Data
{
    public class StaffAssignment
    {
        public int EventId { get; set; }
        public int StaffId { get; set; }

        public Event Event { get; set; }
        public Staff Staff { get; set; }
    }
}

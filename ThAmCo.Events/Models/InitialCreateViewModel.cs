namespace ThAmCo.Events.Models
{
    public class InitialCreateViewModel
    {
        public string EventTypeId { get; set; }
        public DateTime BeginDate { get; set; } = new DateTime(2022, 10, 1);
        public DateTime EndDate { get; set; } = new DateTime(2023, 3, 1);
    }

}

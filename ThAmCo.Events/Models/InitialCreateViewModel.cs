namespace ThAmCo.Events.Models
{
    public class InitialCreateViewModel
    {
        public string EventTypeId { get; set; }
        public DateTime BeginDate { get; set; } = new DateTime(2022, 11, 1);
        public DateTime EndDate { get; set; } = new DateTime(2023, 2, 1);
    }

}

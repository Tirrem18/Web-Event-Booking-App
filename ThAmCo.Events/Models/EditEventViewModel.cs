using System.ComponentModel.DataAnnotations;

namespace ThAmCo.Events.Models;
public class EventEditViewModel
{
    public int EventId { get; set; }

    [Required, StringLength(20)]
    public string Title { get; set; }
}
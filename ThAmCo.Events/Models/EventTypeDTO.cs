using System.ComponentModel.DataAnnotations;

public class EventTypeDTO

{

    [Required, MinLength(3), MaxLength(3)]
    public string Id { get; set; } // or public int Id { get; set; } if the identifier is an integer
    public string Title { get; set; }

    // Include other relevant properties that the API might return
}
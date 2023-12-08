using System.ComponentModel.DataAnnotations;

namespace ThAmCo.Events.Data
{
    public class Qualifications
    {
        [Key]
        public int QualificationId { get; set; }

        [Required, StringLength(50)]
        public string Name { get; set; } // e.g., "First Aider", "Bartender"

        // Navigation property for the many-to-many relationship with Staff
        public ICollection<StaffQualification> StaffQualifications { get; set; }
    }
}
using System.ComponentModel.DataAnnotations;

namespace ThAmCo.Events.Data
{
    public class Qualifications
    {
        [Key]
        public int QualificationId { get; set; }

        [Required, StringLength(50)]
        public string Name { get; set; }

        
        public ICollection<StaffQualification> StaffQualifications { get; set; }
    }
}
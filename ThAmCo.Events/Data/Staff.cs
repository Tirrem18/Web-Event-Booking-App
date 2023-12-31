using System.ComponentModel.DataAnnotations;

namespace ThAmCo.Events.Data
{
    public class Staff
    {
        [Key]
        public int StaffId { get; set; }

        [Required, StringLength(20)]
        public string FirstName { get; set; }

        [Required, StringLength(20)]
        public string LastName { get; set; }

        // This will be used to store a list of qualifications
        public ICollection<StaffQualification> StaffQualifications { get; set; }

        // Collection for StaffAssignments
        public ICollection<StaffAssignment> StaffAssignments { get; set; }

        public Staff()
        {
            StaffQualifications = new HashSet<StaffQualification>();
            StaffAssignments = new HashSet<StaffAssignment>();
        }
    }
}
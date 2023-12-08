using ThAmCo.Events.Data;

public class StaffQualification
{
    public int StaffId { get; set; }
    public Staff Staff { get; set; }

    public int QualificationId { get; set; }
    public Qualifications Qualification { get; set; }
}
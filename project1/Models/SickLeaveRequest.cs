using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

public class SickLeaveRequest
{
    public int Id { get; set; }

    public string EmployeeId { get; set; }

    public Employee Employee { get; set; }

    public DateTime StartDate { get; set; }

    [DataType(DataType.Date)]
    [Compare("StartDate", ErrorMessage = "End Date must be later than Start Date.")]
    public DateTime EndDate { get; set; }

    [Required(ErrorMessage = "Reason is required.")]
    public string Reason { get; set; }

    public string? MedicalReport { get; set; }

    public bool? IsApproved { get; set; } = null; //za da na pocetok bide pending

    public int SickLeaveDaysRequested => (EndDate - StartDate).Days + 1;

    public bool HasSufficientSickLeave(int availableSickLeave)
    {
        return SickLeaveDaysRequested <= availableSickLeave;
    }

    //za dokument
    [NotMapped]
    public IFormFile? MedicalReportFile { get; set; }
}

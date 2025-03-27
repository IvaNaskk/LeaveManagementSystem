namespace project1.ViewModels
{
    public class SickLeaveRequestViewModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Reason { get; set; }
        public IFormFile MedicalReportFile { get; set; }
        public int NumberOfDaysRequested { get; set; } // za pratenje na kolku dena se requested
        public bool? IsApproved { get; internal set; }
    }
}

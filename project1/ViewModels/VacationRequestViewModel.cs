namespace project1.ViewModels
{
    public class VacationRequestViewModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Reason { get; set; }
        public int RemainingAnnualLeave { get; set; }
        public int RemainingBonusLeave { get; set; }
        public bool IsAnnualLeave { get; set; } // true za annual leave, false za bonus leave
        public bool? IsApproved { get; internal set; }
    }
}
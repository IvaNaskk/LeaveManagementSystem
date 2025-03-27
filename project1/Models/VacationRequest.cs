using System;
using System.ComponentModel.DataAnnotations;

public enum LeaveType
{
    Annual,
    Bonus
}

public class VacationRequest
{
    public int Id { get; set; }

    public string EmployeeId { get; set; }

    public DateTime StartDate { get; set; }

    [DataType(DataType.Date)]
    [Compare("StartDate", ErrorMessage = "End Date must be later than Start Date.")]
    public DateTime EndDate { get; set; }

    public bool? IsApproved { get; set; } = null; //za da na pocetok bide pending

    public bool IsAnnualLeave { get; set; } 

    public LeaveType LeaveType { get; set; } //dali e annual ili bonus

    public string Reason { get; set; }

    public Employee Employee { get; set; }

    public int LeaveDaysRequested => (EndDate - StartDate).Days + 1;

    //dali e vo dozvolen period
    public bool IsLeaveWithinAllowedPeriod()
    {
        //za annual leave
        if (LeaveType == LeaveType.Annual)
        {
            var currentYear = DateTime.Now.Year;
            var firstPartEndDate = new DateTime(currentYear, 12, 31); // prv del traje do 31.12
            var secondPartStartDate = new DateTime(currentYear + 1, 1, 1); // vtor del pocnuva na 1.1
            var secondPartEndDate = new DateTime(currentYear + 1, 6, 30); // vtor del zavrshuva na 30.06

            if (StartDate <= firstPartEndDate && EndDate <= firstPartEndDate)
            {
                return true;
            }

            if (StartDate >= secondPartStartDate && EndDate <= secondPartEndDate)
            {
                return true;
            }
        }

        //za bonus leave
        if (LeaveType == LeaveType.Bonus)
        {
            var currentYear = DateTime.Now.Year;
            var endOfYear = new DateTime(currentYear, 12, 31);

            if (StartDate <= endOfYear && EndDate <= endOfYear)
            {
                return true;
            }
        }

        return false;
    }

    //dali e vo dozvolen period
    public bool IsBonusLeaveWithinAllowedPeriod()
    {
        if (!IsAnnualLeave)
        {
            var currentYear = DateTime.Now.Year;
            var endOfYear = new DateTime(currentYear, 12, 31);

            return StartDate <= endOfYear && EndDate <= endOfYear;
        }
        return true; 
    }
}

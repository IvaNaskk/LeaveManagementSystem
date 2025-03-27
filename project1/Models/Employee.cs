using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;


public class Employee : IdentityUser
{
    public string Name { get; set; }

    public int AnnualLeaveDays { get; set; } = 21; //vkupen broj na denovi

    public int AnnualLeaveUsedByDec31 { get; set; } = 0;

    public int AnnualLeaveRemainingAfterDec31 { get; set; } = 11; //za posle 31.12

    public int BonusLeaveDays { get; set; }=0; // na pocetok se nula

    public int SickLeaveDays { get; set; }

    public string Role { get; set; } = "Employee";

    public ICollection<VacationRequest> VacationRequests { get; set; } = new List<VacationRequest>();
    public ICollection<SickLeaveRequest> SickLeaveRequests { get; set; } = new List<SickLeaveRequest>();

    public int TotalLeaveDays => AnnualLeaveDays + BonusLeaveDays;

    public DateTime LastAnnualLeaveUsedDate { get; set; } = DateTime.MinValue;

    //dali isteklo?
    public bool IsAnnualLeaveExpired
    {
        get
        {
            var currentYear = DateTime.Now.Year;
            return LastAnnualLeaveUsedDate.Year < currentYear || LastAnnualLeaveUsedDate < new DateTime(currentYear, 12, 31);
        }
    }

    //reset za posle 31.12 
    public void ResetAnnualLeaveIfExpired()
    {
        var currentDate = DateTime.Now;

        if (currentDate.Month == 1 && currentDate.Day == 1)
        {
            if (AnnualLeaveUsedByDec31 < 10)
            {
                AnnualLeaveDays = 21; 
                AnnualLeaveUsedByDec31 = 0; 
                AnnualLeaveRemainingAfterDec31 = 11; 
            }
        }
    }

    //reset na bonus denovi
    public void ResetBonusLeaveIfExpired()
    {
        var currentDate = DateTime.Now;

        if (currentDate.Month == 1 && currentDate.Day == 1)
        {
            BonusLeaveDays = 0; 
        }
    }

    //za sick days
    public void AddSickLeave(int days)
    {
        SickLeaveDays += days;
    }
}

using System;
using System.Collections.Generic;  
using project1.Models;  
using System.ComponentModel.DataAnnotations;

namespace project1.ViewModels
{
    public class EmployeeDashboardViewModel
    {
        public string Name { get; set; }
        public int AnnualLeaveDays { get; set; }
        public int BonusLeaveDays { get; set; }
        public int SickLeaveDays { get; set; }
        public List<VacationRequest> RecentVacationRequests { get; set; }
        public List<SickLeaveRequest> RecentSickLeaveRequests { get; set; }

        // property za site dozvoleni sick leave days
        public Dictionary<int, string> VacationRequestTypes { get; set; } 

    }
}

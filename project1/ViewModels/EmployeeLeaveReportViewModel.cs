using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace project1.ViewModels
{
    public class EmployeeLeaveReportViewModel
    {
        public string EmployeeName { get; set; }
        public int AnnualLeaveUsed { get; set; }
        public int AnnualLeaveRemaining { get; set; }
        public int BonusLeaveUsed { get; set; }
        public int BonusLeaveRemaining { get; set; }
        public int SickLeaveUsed { get; set; }
        public int SickLeaveRemaining { get; set; }
    }
}


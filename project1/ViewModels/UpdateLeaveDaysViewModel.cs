using System;
using System.ComponentModel.DataAnnotations;

namespace project1.ViewModels
{
    public class UpdateLeaveDaysViewModel
    {
        public string EmployeeId { get; set; }
        public string? Email { get; set; }
        public string? NewPassword { get; set; }
        public string? ConfirmNewPassword { get; set; }

        public int AnnualLeaveDays { get; set; }
        public int BonusLeaveDays { get; set; }
    }
}

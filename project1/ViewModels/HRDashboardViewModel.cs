using System.Collections.Generic;

namespace project1.ViewModels
{
    public class HRDashboardViewModel
    {
        public int TotalEmployees { get; set; }
        public int PendingLeaveRequests { get; set; }
        public int ApprovedLeaveRequests { get; set; }
        public int RejectedLeaveRequests { get; set; }
        public List<RecentLeaveRequestViewModel> RecentRequests { get; set; }
        public List<Employee> LowLeaveBalanceEmployees { get; set; }  
        public List<PendingLeaveRequestViewModel> PendingLeaveRequestsList { get; set; }  
    }

    public class PendingLeaveRequestViewModel
    {
        public string EmployeeName { get; set; }
        public string LeaveType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

}

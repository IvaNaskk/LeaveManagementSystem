using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using project1.Data;
using project1.Models;
using project1.ViewModels;
using System.Linq;
using System.Threading.Tasks;

namespace project1.Controllers
{
    [Authorize(Roles = "HR")]
    public class HRController : Controller
    {
        private readonly UserManager<Employee> _userManager;
        private readonly ApplicationDbContext _context;

        public HRController(UserManager<Employee> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        // Dashboard za HR
        public async Task<IActionResult> Dashboard()
        {
            var totalEmployees = await _context.Employees.CountAsync();
            var pendingRequests = await _context.VacationRequests.CountAsync(vr => vr.IsApproved == null);
            var approvedRequests = await _context.VacationRequests.CountAsync(vr => vr.IsApproved == true);
            var rejectedRequests = await _context.VacationRequests.CountAsync(vr => vr.IsApproved == false);

            var recentRequests = await _context.VacationRequests
                .Take(5)
                .Include(vr => vr.Employee)
                .Select(vr => new RecentLeaveRequestViewModel
                {
                    EmployeeName = vr.Employee.Name,
                    LeaveType = vr.IsAnnualLeave ? "Annual" : "Bonus",
                    Status = vr.IsApproved == null ? "Pending" : vr.IsApproved == true ? "Approved" : "Rejected",
                    StartDate = vr.StartDate.ToShortDateString(),
                    EndDate = vr.EndDate.ToShortDateString()
                })
                .ToListAsync();

            var lowLeaveBalanceEmployees = await _context.Employees
                .Where(e => e.AnnualLeaveDays < 5 || e.BonusLeaveDays < 5) 
                .ToListAsync();

            var pendingLeaveRequestsList = await _context.VacationRequests
                .Where(vr => vr.IsApproved == null)
                .Include(vr => vr.Employee)
                .Select(vr => new PendingLeaveRequestViewModel
                {
                    EmployeeName = vr.Employee.Name,
                    LeaveType = vr.IsAnnualLeave ? "Annual" : "Bonus",
                    StartDate = vr.StartDate,
                    EndDate = vr.EndDate
                })
                .ToListAsync();

            var viewModel = new HRDashboardViewModel
            {
                TotalEmployees = totalEmployees,
                PendingLeaveRequests = pendingRequests,
                ApprovedLeaveRequests = approvedRequests,
                RejectedLeaveRequests = rejectedRequests,
                RecentRequests = recentRequests,
                LowLeaveBalanceEmployees = lowLeaveBalanceEmployees,
                PendingLeaveRequestsList = pendingLeaveRequestsList
            };

            return View(viewModel);
        }

        // Menadziranje na Employees
        public IActionResult ManageEmployees(string searchString)
        {
            var employees = from e in _context.Employees
                            select e;

            if (!string.IsNullOrEmpty(searchString))
            {
                employees = employees.Where(e => e.Name.Contains(searchString) || e.Email.Contains(searchString));
            }

            return View(employees.ToList());
        }

        // Dodavanje Employee GET
        [HttpGet]
        public IActionResult AddEmployee()
        {
            return View(new Employee());
        }

        // Dodavanje Employee POST
        [HttpPost]
        public async Task<IActionResult> AddEmployee(Employee model, string password)
        {
            if (ModelState.IsValid)
            {
                model.UserName = model.Email;

                var result = await _userManager.CreateAsync(model, password ?? "DefaultPassword@123");
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(model, "Employee");
                    return RedirectToAction("ManageEmployees");
                }

                // error ako e neuspeshno
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }

            return View(model);
        }

        // Update na Employee POST
        [HttpPost]
        public async Task<IActionResult> UpdateEmployee(Employee model)
        {
            if (ModelState.IsValid)
            {
                var employee = await _userManager.FindByIdAsync(model.Id);
                if (employee == null) return NotFound();

                employee.Name = model.Name;
                employee.Email = model.Email;
                employee.AnnualLeaveDays = model.AnnualLeaveDays;
                employee.BonusLeaveDays = model.BonusLeaveDays; 

                var result = await _userManager.UpdateAsync(employee);
                if (result.Succeeded)
                {
                    return Json(new { success = true, message = "Employee updated successfully!" });
                }

                // error ako e neuspeshno
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }

                return Json(new { success = false, message = "Failed to update employee." });
            }

            return Json(new { success = false, message = "Invalid data." });
        }

        // Brishenje na Employee
        public async Task<IActionResult> DeleteEmployee(string id)
        {
            var employee = await _userManager.FindByIdAsync(id);
            if (employee != null)
            {
                await _userManager.DeleteAsync(employee);
                return RedirectToAction("ManageEmployees"); // Redirect po uspeshno brishenje
            }
            return NotFound();
        }

        // Update na Leave Days GET
        [HttpGet]
        public async Task<IActionResult> UpdateLeaveDays(string employeeId)
        {
            var employee = await _userManager.FindByIdAsync(employeeId);
            if (employee == null) return NotFound();

            var model = new UpdateLeaveDaysViewModel
            {
                EmployeeId = employee.Id,
                AnnualLeaveDays = employee.AnnualLeaveDays,
                BonusLeaveDays = employee.BonusLeaveDays,
                Email = employee.Email,
                NewPassword = "",
                ConfirmNewPassword = ""
            };

            return View(model);
        }

        // Update na Leave Days POST
        [HttpPost]
        public async Task<IActionResult> UpdateLeaveDays(UpdateLeaveDaysViewModel model)
        {
            if (ModelState.IsValid)
            {
                var employee = await _userManager.FindByIdAsync(model.EmployeeId);
                if (employee != null)
                {
                    employee.AnnualLeaveDays = model.AnnualLeaveDays;
                    employee.BonusLeaveDays = model.BonusLeaveDays;

                    _context.Employees.Update(employee);
                    await _context.SaveChangesAsync();

                    return RedirectToAction("ManageEmployees"); // Redirect posle update na leave days
                }
            }

            return View(model);
        }

        //Generiranje na Report 
        [Authorize(Roles = "HR")]
        public async Task<IActionResult> GenerateLeaveReport()
        {
            // zimanje na site employees i nivni detali
            var employees = await _context.Employees
                .Include(e => e.VacationRequests)
                .Include(e => e.SickLeaveRequests)
                .ToListAsync();

            // presmetka na iskoristeni i ostanati denovi za sekoj employee
            var report = employees.Select(e => new EmployeeLeaveReportViewModel
            {
                EmployeeName = e.Name,
                AnnualLeaveUsed = e.VacationRequests
                    .Where(vr => vr.IsApproved == true && vr.IsAnnualLeave)
                    .Sum(vr => (vr.EndDate - vr.StartDate).Days + 1),
                AnnualLeaveRemaining = e.AnnualLeaveDays,
                BonusLeaveUsed = e.VacationRequests
                    .Where(vr => vr.IsApproved == true && !vr.IsAnnualLeave)
                    .Sum(vr => (vr.EndDate - vr.StartDate).Days + 1),
                BonusLeaveRemaining = e.BonusLeaveDays,
                SickLeaveUsed = e.SickLeaveRequests
                    .Where(sl => sl.IsApproved == true)
                    .Sum(sl => (sl.EndDate - sl.StartDate).Days + 1),
            }).ToList();

            return View(report);
        }

        //Export na Excel file
        [Authorize(Roles = "HR")]
        public async Task<IActionResult> ExportLeaveReportToExcel()
        {
            var employees = await _context.Employees
                .Include(e => e.VacationRequests)
                .Include(e => e.SickLeaveRequests)
                .ToListAsync();

            var report = employees.Select(e => new EmployeeLeaveReportViewModel
            {
                EmployeeName = e.Name,
                AnnualLeaveUsed = e.VacationRequests
                    .Where(vr => vr.IsApproved == true && vr.IsAnnualLeave)
                    .Sum(vr => (vr.EndDate - vr.StartDate).Days + 1),
                AnnualLeaveRemaining = e.AnnualLeaveDays,
                BonusLeaveUsed = e.VacationRequests
                    .Where(vr => vr.IsApproved == true && !vr.IsAnnualLeave)
                    .Sum(vr => (vr.EndDate - vr.StartDate).Days + 1),
                BonusLeaveRemaining = e.BonusLeaveDays,
                SickLeaveUsed = e.SickLeaveRequests
                    .Where(sl => sl.IsApproved == true)
                    .Sum(sl => (sl.EndDate - sl.StartDate).Days + 1),
            }).ToList();

            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Leave Report");

                // headers
                worksheet.Cells[1, 1].Value = "Employee";
                worksheet.Cells[1, 2].Value = "Annual Leave Used";
                worksheet.Cells[1, 3].Value = "Annual Leave Remaining";
                worksheet.Cells[1, 4].Value = "Bonus Leave Used";
                worksheet.Cells[1, 5].Value = "Bonus Leave Remaining";
                worksheet.Cells[1, 6].Value = "Sick Leave Used";

                // data
                for (int i = 0; i < report.Count; i++)
                {
                    worksheet.Cells[i + 2, 1].Value = report[i].EmployeeName;
                    worksheet.Cells[i + 2, 2].Value = report[i].AnnualLeaveUsed;
                    worksheet.Cells[i + 2, 3].Value = report[i].AnnualLeaveRemaining;
                    worksheet.Cells[i + 2, 4].Value = report[i].BonusLeaveUsed;
                    worksheet.Cells[i + 2, 5].Value = report[i].BonusLeaveRemaining;
                    worksheet.Cells[i + 2, 6].Value = report[i].SickLeaveUsed;
                }

                // zachuvuvanje na file
                var stream = new MemoryStream();
                package.SaveAs(stream);
                stream.Position = 0;

                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "LeaveReport.xlsx");
            }
        }

    }
}

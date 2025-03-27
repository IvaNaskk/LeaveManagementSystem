using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using project1.Data;
using project1.Models;
using project1.ViewModels;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

public class LeaveRequestController : Controller
{
    private readonly UserManager<Employee> _userManager;
    private readonly ApplicationDbContext _context;

    public LeaveRequestController(UserManager<Employee> userManager, ApplicationDbContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    private async Task<Employee> GetCurrentUserAsync()
    {
        return await _userManager.GetUserAsync(User);
    }

    // HR View za Leave Requests
    [Authorize(Roles = "HR")]
    public async Task<IActionResult> ManageVacationRequests(string statusFilter = "Pending", string sortBy = "EmployeeName")
    {
        // Fetch na vacation requests
        var vacationRequests = _context.VacationRequests
            .Include(vr => vr.Employee)
            .AsQueryable();

        // status filter
        if (statusFilter == "Approved")
        {
            vacationRequests = vacationRequests.Where(vr => vr.IsApproved == true);
        }
        else if (statusFilter == "Rejected")
        {
            vacationRequests = vacationRequests.Where(vr => vr.IsApproved == false);
        }
        else
        {
            vacationRequests = vacationRequests.Where(vr => vr.IsApproved == null);
        }

        var vacationRequestsList = await vacationRequests.ToListAsync();

        return View("~/Views/HR/ManageVacationRequests.cshtml", vacationRequestsList);
    }

    // HR View za Sick Leave Requests
    [Authorize(Roles = "HR")]
    public async Task<IActionResult> ManageSickLeaveRequests(string statusFilter = "Pending", string sortBy = "EmployeeName")
    {
        // Fetch na sick leave requests
        var sickLeaveRequests = _context.SickLeaveRequests
            .Include(sl => sl.Employee) 
            .AsQueryable();

        // status filter
        if (statusFilter == "Approved")
        {
            sickLeaveRequests = sickLeaveRequests.Where(sl => sl.IsApproved == true);
        }
        else if (statusFilter == "Rejected")
        {
            sickLeaveRequests = sickLeaveRequests.Where(sl => sl.IsApproved == false);
        }
        else
        {
            sickLeaveRequests = sickLeaveRequests.Where(sl => sl.IsApproved == null);
        }

        var sickLeaveRequestsList = await sickLeaveRequests.ToListAsync();

        return View("~/Views/HR/ManageSickLeaveRequests.cshtml", sickLeaveRequestsList);
    }

    // Approve/Reject za Vacation Requests za HR
    [Authorize(Roles = "HR")]
    [HttpPost]
    public async Task<IActionResult> ApproveRejectVacationRequest(int id, bool approve)
    {
        var request = await _context.VacationRequests.FindAsync(id);
        if (request != null)
        {
            request.IsApproved = approve;
            _context.Update(request);
            await _context.SaveChangesAsync();

            if (approve)
            {
                var employee = await _userManager.FindByIdAsync(request.EmployeeId);
                if (employee != null)
                {
                    var leaveDuration = (request.EndDate - request.StartDate).Days+1;
                    if (request.IsAnnualLeave)
                    {
                        employee.AnnualLeaveDays -= leaveDuration;
                    }
                    else
                    {
                        employee.BonusLeaveDays -= leaveDuration;
                    }

                    _context.Update(employee);
                    await _context.SaveChangesAsync();
                }
            }
        }

        return RedirectToAction("ManageVacationRequests");
    }

    //// Approve/Reject za Sick Leave Requests za HR
    [Authorize(Roles = "HR")]
    [HttpPost]
    public async Task<IActionResult> ApproveRejectSickLeaveRequest(int id, bool approve)
    {
        var request = await _context.SickLeaveRequests.FindAsync(id);
        if (request != null)
        {
            request.IsApproved = approve;
            _context.Update(request);
            await _context.SaveChangesAsync();

            if (approve)
            {
                var employee = await _userManager.FindByIdAsync(request.EmployeeId);
                if (employee != null)
                {
                    var sickLeaveDuration = (request.EndDate - request.StartDate).Days + 1;
                    employee.SickLeaveDays -= sickLeaveDuration;

                    _context.Update(employee);
                    await _context.SaveChangesAsync();
                }
            }
        }

        return RedirectToAction("ManageSickLeaveRequests");
    }
}

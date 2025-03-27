using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using project1.Data;
using project1.Models;
using project1.ViewModels;
using System.Linq;
using System.Threading.Tasks;
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.IO;

public class EmployeeController : Controller
{
    private readonly UserManager<Employee> _userManager;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<EmployeeController> _logger;


    public EmployeeController(UserManager<Employee> userManager, ApplicationDbContext context, ILogger<EmployeeController> logger)
    {
        _userManager = userManager;
        _context = context;
        _logger = logger;
    }

    // profil
    public async Task<IActionResult> Profile()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound("User not found.");
        }
        return View(user);
    }   

    // Request Vacation GET
    [HttpGet]
    public async Task<IActionResult> RequestVacation()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound("User not found.");
        }

        var viewModel = new VacationRequestViewModel
        {
            RemainingAnnualLeave = user.AnnualLeaveDays,
            RemainingBonusLeave = user.BonusLeaveDays
        };

        return View(viewModel);
    }

    // Request Vacation POST
    [HttpPost]
    public async Task<IActionResult> RequestVacation(VacationRequestViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound("User not found.");
        }

        var vacationRequest = new VacationRequest
        {
            EmployeeId = user.Id,
            StartDate = model.StartDate,
            EndDate = model.EndDate,
            Reason = model.Reason,
            IsApproved = null, // null za da e pending
            IsAnnualLeave = model.IsAnnualLeave // dali e od odmor ili od bonus
        };

        // proverka za dali e bonus i vo tekovna godina
        if (!vacationRequest.IsAnnualLeave && !vacationRequest.IsBonusLeaveWithinAllowedPeriod())
        {
            ModelState.AddModelError("", "Bonus leave must be used within the current calendar year.");
            return View(model);
        }

        // slobodni denovi bez da gi odzeme
        int leaveDaysRequested = vacationRequest.LeaveDaysRequested;

        if (vacationRequest.IsAnnualLeave)
        {
            if (user.AnnualLeaveDays < leaveDaysRequested)
            {
                ModelState.AddModelError("", "Not enough annual leave days.");
                return View(model);
            }
        }
        else
        {
            if (user.BonusLeaveDays < leaveDaysRequested)
            {
                ModelState.AddModelError("", "Not enough bonus leave days.");
                return View(model);
            }
        }

        // socuvaj bez da gi odzeme denovi
        _context.VacationRequests.Add(vacationRequest);
        await _context.SaveChangesAsync();

        return RedirectToAction("Dashboard");
    }

    // Request Sick Leave GET
    [HttpGet]
    public async Task<IActionResult> RequestSickLeave()
    {
        var user = await _userManager.GetUserAsync(User);
        var model = new SickLeaveRequestViewModel();

        return View(model);
    }

    // Request Sick Leave POST
    [HttpPost]
    public async Task<IActionResult> RequestSickLeave(SickLeaveRequestViewModel model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound("User not found.");
        }

        if (model == null)
        {
            ModelState.AddModelError("", "Invalid request.");
            return View(model);
        }

        var existingUser = await _context.Employees.AsNoTracking().FirstOrDefaultAsync(e => e.Id == user.Id);
        if (existingUser == null)
        {
            return NotFound("User not found.");
        }

        user.SickLeaveDays += model.NumberOfDaysRequested;

        var sickRequest = new SickLeaveRequest
        {
            EmployeeId = user.Id,
            StartDate = model.StartDate,
            EndDate = model.EndDate,
            Reason = model.Reason,
            IsApproved = model.IsApproved,
            MedicalReport = model.MedicalReportFile != null ? await SaveMedicalReport(model.MedicalReportFile) : null
        };

        _context.SickLeaveRequests.Add(sickRequest);
        _context.Entry(user).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        var result = await _userManager.UpdateAsync(user);
        if (result.Succeeded)
        {
            return RedirectToAction("Dashboard", "Employee");
        }

        ModelState.AddModelError("", "An error occurred while processing your request.");
        return View(model);
    }

    // Employee Dashboard
    public async Task<IActionResult> Dashboard()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound("User not found.");
        }

        // Vacation request 
        var vacationRequests = await _context.VacationRequests
            .Where(vr => vr.EmployeeId == user.Id)
            .OrderByDescending(vr => vr.StartDate)
            .Take(5)
            .ToListAsync();

        // SickLeave requests
        var sickLeaveRequests = await _context.SickLeaveRequests
            .Where(sl => sl.EmployeeId == user.Id)
            .OrderByDescending(sl => sl.StartDate)
            .Take(5)
            .ToListAsync();

        // proverka dali e obicno ili bonus 
        var vacationRequestTypes = new Dictionary<int, string>();
        foreach (var request in vacationRequests)
        {
            vacationRequestTypes[request.Id] = request.IsAnnualLeave ? "Annual Leave" : "Bonus Leave";
        }

        // za ViewModelot
        var viewModel = new EmployeeDashboardViewModel
        {
            Name = user.Name,
            AnnualLeaveDays = user.AnnualLeaveDays,
            BonusLeaveDays = user.BonusLeaveDays,
            SickLeaveDays = user.SickLeaveDays,
            RecentVacationRequests = vacationRequests,
            RecentSickLeaveRequests = sickLeaveRequests,
            VacationRequestTypes = vacationRequestTypes
        };

        return View(viewModel);
    }

    private async Task<string> SaveMedicalReport(IFormFile medicalReportFile)
    {
        // pateka
        var uploadsFolder = Path.Combine("wwwroot", "uploads", "medicalreports");

        // ako postoi pateka 
        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        // kreira unique file name
        var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(medicalReportFile.FileName);
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        // Save na medical file
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await medicalReportFile.CopyToAsync(stream);
        }

        return Path.Combine("/uploads/medicalreports", uniqueFileName);
    }
}
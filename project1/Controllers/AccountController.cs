using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using project1.Models;
using project1.ViewModels;

namespace project1.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<Employee> signInManager;
        private readonly UserManager<Employee> userManager;

        public AccountController(SignInManager<Employee> signInManager, UserManager<Employee> userManager)
        {
            this.signInManager = signInManager;
            this.userManager = userManager;
            this.userManager = userManager;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        //Za Login
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await userManager.FindByEmailAsync(model.Email);
                if (user != null)
                {
                    var result = await signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, false);

                    if (result.Succeeded)
                    {
                        // proverka na role i redirect 
                        var roles = await userManager.GetRolesAsync(user);
                        if (roles.Contains("HR"))
                        {
                            return RedirectToAction("Dashboard", "HR"); // za Hr
                        }
                        else if (roles.Contains("Employee"))
                        {
                            return RedirectToAction("Dashboard", "Employee"); // za Employee
                        }
                    }
                }
                ModelState.AddModelError("", "Email or password is incorrect.");
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        //Za Registracija
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var employee = new Employee
                {
                    Name = model.Name,
                    Email = model.Email,
                    UserName = model.Email,
                    AnnualLeaveDays = 21, 
                    BonusLeaveDays = 0 
                };

                var result = await userManager.CreateAsync(employee, model.Password);
                if (result.Succeeded)
                {
                    // Avtomatski dobiva "Employee" role
                    await userManager.AddToRoleAsync(employee, "Employee");
                    return RedirectToAction("Login", "Account");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
            }
            return View(model);
        }

        //Za Lgout
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("Login", "Account"); 
        }

        public async Task<IActionResult> UpdateLeaveDays()
        {
            var user = await userManager.GetUserAsync(User);
            if (user != null)
            {
                var currentDate = DateTime.Now;

                if (currentDate.Month == 12 && user.AnnualLeaveDays > 10)
                {
                    user.AnnualLeaveDays -= 10; // 10 dena do 31.12
                }

                if (currentDate.Month == 6 && user.AnnualLeaveDays > 0)
                {
                    user.AnnualLeaveDays = 0; // ostatok do 30.06
                }

                await userManager.UpdateAsync(user);
            }

            return RedirectToAction("Dashboard", "Employee");
        }
    }
}

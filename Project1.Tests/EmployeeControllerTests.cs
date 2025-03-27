using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using project1.Controllers;
using project1.Data;
using project1.Models;
using project1.ViewModels;
using Xunit;

namespace Project1.Tests.Controllers
{
    public class EmployeeControllerTests : IDisposable
    {
        private readonly Mock<UserManager<Employee>> _mockUserManager;
        private readonly ApplicationDbContext _context;
        private readonly Mock<ILogger<EmployeeController>> _mockLogger;
        private readonly EmployeeController _controller;

        public EmployeeControllerTests()
        {
            var store = new Mock<IUserStore<Employee>>();
            _mockUserManager = new Mock<UserManager<Employee>>(
                store.Object, null, null, null, null, null, null, null, null
            );

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;
            _context = new ApplicationDbContext(options);

            _mockLogger = new Mock<ILogger<EmployeeController>>();

            _controller = new EmployeeController(_mockUserManager.Object, _context, _mockLogger.Object);

            var testUser = new Employee
            {
                Id = "1",
                Name = "Test User",
                Email = "test@example.com",
                SickLeaveDays = 10,
                AnnualLeaveDays = 21,
                BonusLeaveDays = 0
            };

            _context.Employees.Add(testUser);
            _context.SaveChanges();
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
        }

        // Test for Profile action
        [Fact]
        public async Task Profile_ReturnsViewWithUser()
        {
            // Arrange
            var user = new Employee
            {
                Id = "1",
                Name = "Test User",
                Email = "test@example.com"
            };

            _mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            // Act
            var result = await _controller.Profile();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(user, viewResult.Model);
        }

        // Test for the GET RequestVacationLeave Action
        [Fact]
        public async Task RequestVacation_Get_ReturnsViewWithViewModel()
        {
            // Arrange
            var user = new Employee
            {
                Id = "1",
                Name = "Test User",
                Email = "test@example.com",
                AnnualLeaveDays = 21,
                BonusLeaveDays = 5
            };

            _mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            // Act
            var result = await _controller.RequestVacation();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<VacationRequestViewModel>(viewResult.Model);
            Assert.Equal(user.AnnualLeaveDays, model.RemainingAnnualLeave);
            Assert.Equal(user.BonusLeaveDays, model.RemainingBonusLeave);
        }

        // Test for the POST RequestVacationLeave Action
        [Fact]
        public async Task RequestVacation_Post_RedirectsToDashboard()
        {
            // Arrange
            var user = new Employee
            {
                Id = "1",
                Name = "Test User",
                Email = "test@example.com",
                AnnualLeaveDays = 21,
                BonusLeaveDays = 0
            };

            _mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            var model = new VacationRequestViewModel
            {
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(3),
                IsAnnualLeave = true,
                RemainingAnnualLeave = 21,
                RemainingBonusLeave = 0,
                Reason = "Vacation for personal reasons" 
            };

            // Act
            var result = await _controller.RequestVacation(model);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Dashboard", redirectResult.ActionName);
        }

        // Test for the GET RequestSickLeave Action
        [Fact]
        public async Task RequestSickLeave_Get_ReturnsViewWithModel()
        {
            // Arrange
            var user = new Employee
            {
                Id = "1",
                Name = "Test User",
                Email = "test@example.com",
                SickLeaveDays = 10
            };

            _mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            // Act
            var result = await _controller.RequestSickLeave();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<SickLeaveRequestViewModel>(viewResult.Model);
            Assert.NotNull(model);
        }

        // Test for the POST RequestSickLeave Action 
        [Fact]
        public async Task RequestSickLeave_Post_RedirectsToDashboard()
        {
            // Arrange
            var user = new Employee
            {
                Id = "1",
                Name = "Test User",
                Email = "test@example.com",
                SickLeaveDays = 10
            };

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var freshContext = new ApplicationDbContext(options);

            freshContext.Employees.Add(user);
            await freshContext.SaveChangesAsync();

            _mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(freshContext.Employees.Find(user.Id));

            _mockUserManager.Setup(um => um.UpdateAsync(It.IsAny<Employee>()))
                .ReturnsAsync(IdentityResult.Success);

            var controller = new EmployeeController(_mockUserManager.Object, freshContext, _mockLogger.Object);

            // Mock IFormFile 
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns("test.pdf");
            fileMock.Setup(f => f.Length).Returns(1024); // 1 KB file
            fileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var model = new SickLeaveRequestViewModel
            {
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(2),
                Reason = "Flu",
                NumberOfDaysRequested = 2,
                MedicalReportFile = fileMock.Object 
            };

            // Act
            var result = await controller.RequestSickLeave(model);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Dashboard", redirectResult.ActionName);
            Assert.Equal("Employee", redirectResult.ControllerName);

            var savedRequest = await freshContext.SickLeaveRequests.FirstOrDefaultAsync();
            Assert.NotNull(savedRequest);
            Assert.Equal(user.Id, savedRequest.EmployeeId);
            Assert.Equal(model.StartDate, savedRequest.StartDate);
            Assert.Equal(model.EndDate, savedRequest.EndDate);
            Assert.Equal(model.Reason, savedRequest.Reason);

            var updatedUser = await freshContext.Employees.FindAsync(user.Id);
            Assert.NotNull(updatedUser);
            Assert.Equal(12, updatedUser.SickLeaveDays); 
        }

        // Test for Dashboard action
        [Fact]
        public async Task Dashboard_ReturnsViewWithViewModel()
        {
            // Arrange
            var user = new Employee
            {
                Id = "1",
                Name = "Test User",
                Email = "test@example.com",
                AnnualLeaveDays = 21,
                BonusLeaveDays = 0,
                SickLeaveDays = 10
            };

            _mockUserManager.Setup(um => um.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                .ReturnsAsync(user);

            var vacationRequest = new VacationRequest
            {
                Id = 1,
                EmployeeId = "1",
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(5),
                IsApproved = null,
                IsAnnualLeave = true,
                Reason = "Vacation for personal reasons" 
            };

            var sickLeaveRequest = new SickLeaveRequest
            {
                Id = 1,
                EmployeeId = "1",
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(2),
                IsApproved = null,
                Reason = "Flu" 
            };

            _context.VacationRequests.Add(vacationRequest);
            _context.SickLeaveRequests.Add(sickLeaveRequest);
            _context.SaveChanges();

            // Act
            var result = await _controller.Dashboard();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<EmployeeDashboardViewModel>(viewResult.Model);
            Assert.Equal(user.Name, model.Name);
            Assert.Single(model.RecentVacationRequests);
            Assert.Single(model.RecentSickLeaveRequests);
        }

        // Test for SaveMedicalReport
        [Fact]
        public async Task SaveMedicalReport_SavesFileAndReturnsCorrectPath()
        {
            // Arrange
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            var freshContext = new ApplicationDbContext(options);

            var controller = new EmployeeController(_mockUserManager.Object, freshContext, _mockLogger.Object);

            // Mock IFormFile
            var fileMock = new Mock<IFormFile>();
            var fileName = "test.pdf";
            var content = "This is a dummy file";
            var ms = new MemoryStream(Encoding.UTF8.GetBytes(content));
            var fileStream = new FormFile(ms, 0, ms.Length, "MedicalReport", fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = "application/pdf"
            };

            fileMock.Setup(f => f.FileName).Returns(fileName);
            fileMock.Setup(f => f.Length).Returns(ms.Length);
            fileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .Returns((Stream stream, CancellationToken token) => ms.CopyToAsync(stream));

            var method = typeof(EmployeeController).GetMethod("SaveMedicalReport", BindingFlags.NonPublic | BindingFlags.Instance);
            var task = (Task<string>)method.Invoke(controller, new object[] { fileMock.Object });

            // Act
            var filePath = await task;

            // Assert
            Assert.NotNull(filePath);
            Assert.StartsWith("/uploads/medicalreports/", filePath); 
            Assert.EndsWith(".pdf", filePath); 

            var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", filePath.TrimStart('/'));
            Assert.True(System.IO.File.Exists(fullPath), "File was not saved to the expected location.");

            var uploadsFolder = Path.Combine("wwwroot", "uploads", "medicalreports");
            Assert.True(Directory.Exists(uploadsFolder), "Uploads directory was not created.");

            var savedContent = await System.IO.File.ReadAllTextAsync(fullPath);
            Assert.Equal(content, savedContent);

            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }

            if (Directory.Exists(uploadsFolder))
            {
                Directory.Delete(uploadsFolder, true);
            }
        }

    }
}
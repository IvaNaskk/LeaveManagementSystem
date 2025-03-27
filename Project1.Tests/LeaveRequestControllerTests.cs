using System;
using System.Security.Claims;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using project1.Controllers;
using project1.Data;
using project1.Models;
using project1.ViewModels;
using Xunit;

namespace Project1.Tests.Controllers
{
    public class LeaveRequestControllerTests : IDisposable
    {
        private readonly Mock<UserManager<Employee>> _mockUserManager;
        private readonly Mock<ApplicationDbContext> _mockContext;
        private readonly LeaveRequestController _controller;

        public LeaveRequestControllerTests()
        {
            var store = new Mock<IUserStore<Employee>>();
            _mockUserManager = new Mock<UserManager<Employee>>(
                store.Object, null, null, null, null, null, null, null, null
            );

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;
            _mockContext = new Mock<ApplicationDbContext>(options);

            _controller = new LeaveRequestController(_mockUserManager.Object, _mockContext.Object);
        }

        public void Dispose()
        {
            // _mockContext.Object.Database.EnsureDeleted();
        }

        // Test for ManageVacationRequests
        [Fact]
        public async Task ManageVacationRequests_ReturnsViewWithPendingRequests()
        {
            // Arrange
            var vacationRequests = new List<VacationRequest>
            {
                new VacationRequest
                {
                    Id = 1,
                    EmployeeId = "1",
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now.AddDays(5),
                    IsApproved = null
                },
                new VacationRequest
                {
                    Id = 2,
                    EmployeeId = "2",
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now.AddDays(3),
                    IsApproved = true
                }
            }.AsQueryable();

            var mockVacationRequestSet = new Mock<DbSet<VacationRequest>>();
            mockVacationRequestSet.As<IAsyncEnumerable<VacationRequest>>()
                .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
                .Returns(new TestAsyncEnumerator<VacationRequest>(vacationRequests.GetEnumerator()));

            mockVacationRequestSet.As<IQueryable<VacationRequest>>()
                .Setup(m => m.Provider)
                .Returns(new TestAsyncQueryProvider<VacationRequest>(vacationRequests.Provider));

            mockVacationRequestSet.As<IQueryable<VacationRequest>>()
                .Setup(m => m.Expression)
                .Returns(vacationRequests.Expression);

            mockVacationRequestSet.As<IQueryable<VacationRequest>>()
                .Setup(m => m.ElementType)
                .Returns(vacationRequests.ElementType);

            mockVacationRequestSet.As<IQueryable<VacationRequest>>()
                .Setup(m => m.GetEnumerator())
                .Returns(vacationRequests.GetEnumerator());

            _mockContext.Setup(c => c.VacationRequests).Returns(mockVacationRequestSet.Object);

            // Act
            var result = await _controller.ManageVacationRequests();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<List<VacationRequest>>(viewResult.Model);
            Assert.Single(model);
        }

        // Test for ManageSickLeaveRequests
        [Fact]
        public async Task ManageSickLeaveRequests_ReturnsViewWithPendingRequests()
        {
            // Arrange
            var sickLeaveRequests = new List<SickLeaveRequest>
            {
                new SickLeaveRequest
                {
                    Id = 1,
                    EmployeeId = "1",
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now.AddDays(2),
                    IsApproved = null
                },
                new SickLeaveRequest
                {
                    Id = 2,
                    EmployeeId = "2",
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now.AddDays(1),
                    IsApproved = true
                }
            }.AsQueryable();

            var mockSickLeaveRequestSet = new Mock<DbSet<SickLeaveRequest>>();
            mockSickLeaveRequestSet.As<IAsyncEnumerable<SickLeaveRequest>>()
                .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
                .Returns(new TestAsyncEnumerator<SickLeaveRequest>(sickLeaveRequests.GetEnumerator()));

            mockSickLeaveRequestSet.As<IQueryable<SickLeaveRequest>>()
                .Setup(m => m.Provider)
                .Returns(new TestAsyncQueryProvider<SickLeaveRequest>(sickLeaveRequests.Provider));

            mockSickLeaveRequestSet.As<IQueryable<SickLeaveRequest>>()
                .Setup(m => m.Expression)
                .Returns(sickLeaveRequests.Expression);

            mockSickLeaveRequestSet.As<IQueryable<SickLeaveRequest>>()
                .Setup(m => m.ElementType)
                .Returns(sickLeaveRequests.ElementType);

            mockSickLeaveRequestSet.As<IQueryable<SickLeaveRequest>>()
                .Setup(m => m.GetEnumerator())
                .Returns(sickLeaveRequests.GetEnumerator());

            _mockContext.Setup(c => c.SickLeaveRequests).Returns(mockSickLeaveRequestSet.Object);

            // Act
            var result = await _controller.ManageSickLeaveRequests();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<List<SickLeaveRequest>>(viewResult.Model);
            Assert.Single(model);
        }

        // Test for Approved Vacation Request  
        [Fact]
        public async Task ApproveVacationRequest_RedirectsToManageVacationRequests()
        {
            // Arrange
            var vacationRequest = new VacationRequest
            {
                Id = 1,
                EmployeeId = "1",
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(5),
                IsApproved = null
            };

            _mockContext.Setup(c => c.VacationRequests.FindAsync(1)).ReturnsAsync(vacationRequest);

            // Act
            var result = await _controller.ApproveRejectVacationRequest(1, true);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("ManageVacationRequests", redirectResult.ActionName);
            Assert.True(vacationRequest.IsApproved);
        }

        // Test for Rejected VacationRequest
        [Fact]
        public async Task RejectLeaveRequest_RedirectsToManageVacationRequests()
        {
            // Arrange
            var vacationRequest = new VacationRequest
            {
                Id = 1,
                EmployeeId = "1",
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(5),
                IsApproved = null
            };

            _mockContext.Setup(c => c.VacationRequests.FindAsync(1)).ReturnsAsync(vacationRequest);

            // Act
            var result = await _controller.ApproveRejectVacationRequest(1, false);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("ManageVacationRequests", redirectResult.ActionName);
            Assert.False(vacationRequest.IsApproved);
        }

        // Test for Approved SickLeaveRequest
        [Fact]
        public async Task ApproveSickLeaveRequest_RedirectsToManageSickLeaveRequests()
        {
            // Arrange
            var sickLeaveRequest = new SickLeaveRequest
            {
                Id = 1,
                EmployeeId = "1",
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(2),
                IsApproved = null
            };

            _mockContext.Setup(c => c.SickLeaveRequests.FindAsync(1)).ReturnsAsync(sickLeaveRequest);

            // Act
            var result = await _controller.ApproveRejectSickLeaveRequest(1, true);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("ManageSickLeaveRequests", redirectResult.ActionName);
            Assert.True(sickLeaveRequest.IsApproved);
        }

        // Test for Rejected SickLeaveRequest
        [Fact]
        public async Task RejectSickLeaveRequest_RedirectsToManageSickLeaveRequests()
        {
            // Arrange
            var sickLeaveRequest = new SickLeaveRequest
            {
                Id = 1,
                EmployeeId = "1",
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(2),
                IsApproved = null
            };

            _mockContext.Setup(c => c.SickLeaveRequests.FindAsync(1)).ReturnsAsync(sickLeaveRequest);

            // Act
            var result = await _controller.ApproveRejectSickLeaveRequest(1, false);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("ManageSickLeaveRequests", redirectResult.ActionName);
            Assert.False(sickLeaveRequest.IsApproved);
        }
    }
}
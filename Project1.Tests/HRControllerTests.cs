using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Moq;
using OfficeOpenXml;
using project1.Controllers;
using project1.Data;
using project1.Models;
using project1.ViewModels;
using Xunit;

namespace Project1.Tests.Controllers
{
    public class HRControllerTests
    {
        private readonly Mock<UserManager<Employee>> _mockUserManager;
        private readonly Mock<ApplicationDbContext> _mockContext;
        private readonly HRController _controller;

        public HRControllerTests()
        {
            var store = new Mock<IUserStore<Employee>>();
            var mockOptions = new Mock<IOptions<IdentityOptions>>();
            var mockPasswordHasher = new Mock<IPasswordHasher<Employee>>();
            var mockUserValidators = new List<IUserValidator<Employee>>();
            var mockPasswordValidators = new List<IPasswordValidator<Employee>>();
            _mockUserManager = new Mock<UserManager<Employee>>(
                store.Object,
                mockOptions.Object,
                mockPasswordHasher.Object,
                mockUserValidators,
                mockPasswordValidators,
                new Mock<ILookupNormalizer>().Object,
                new Mock<IdentityErrorDescriber>().Object,
                new Mock<IServiceProvider>().Object,
                new Mock<ILogger<UserManager<Employee>>>().Object
            );

            var dbContextOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb")
                .Options;

            _mockContext = new Mock<ApplicationDbContext>(dbContextOptions);

            // Employees
            var employees = new List<Employee>
            {
                new Employee { Id = "1", Name = "John Doe", Email = "john@example.com", AnnualLeaveDays = 10, BonusLeaveDays = 5, SickLeaveDays = 3 },
                new Employee { Id = "2", Name = "Jane Doe", Email = "jane@example.com", AnnualLeaveDays = 15, BonusLeaveDays = 2, SickLeaveDays = 0 },
            }.AsQueryable();

            var mockEmployeeSet = new Mock<DbSet<Employee>>();
            mockEmployeeSet.As<IAsyncEnumerable<Employee>>()
                .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
                .Returns(new TestAsyncEnumerator<Employee>(employees.GetEnumerator()));

            mockEmployeeSet.As<IQueryable<Employee>>()
                .Setup(m => m.Provider)
                .Returns(new TestAsyncQueryProvider<Employee>(employees.Provider));

            mockEmployeeSet.As<IQueryable<Employee>>()
                .Setup(m => m.Expression)
                .Returns(employees.Expression);

            mockEmployeeSet.As<IQueryable<Employee>>()
                .Setup(m => m.ElementType)
                .Returns(employees.ElementType);

            mockEmployeeSet.As<IQueryable<Employee>>()
                .Setup(m => m.GetEnumerator())
                .Returns(employees.GetEnumerator());

            _mockContext.Setup(c => c.Employees).Returns(mockEmployeeSet.Object);

            // Vacation Requests
            var vacationRequests = new List<VacationRequest>
            {
                new VacationRequest
                {
                    Id = 1,
                    EmployeeId = "1",
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now.AddDays(5),
                    IsApproved = true, // Approved request
                    IsAnnualLeave = true, // Annual leave
                    Employee = employees.First(e => e.Id == "1") // Link to John Doe
                },
                new VacationRequest
                {
                    Id = 2,
                    EmployeeId = "2",
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now.AddDays(3),
                    IsApproved = null, // Pending request
                    IsAnnualLeave = false, // Bonus leave
                    Employee = employees.First(e => e.Id == "2") // Link to Jane Doe
                },
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

            // Sick Leave Requests
            var sickLeaveRequests = new List<SickLeaveRequest>
            {
                new SickLeaveRequest
                {
                    Id = 1,
                    EmployeeId = "1",
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now.AddDays(2),
                    IsApproved = true
                },
                new SickLeaveRequest
                {
                    Id = 2,
                    EmployeeId = "2",
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now.AddDays(1),
                    IsApproved = true
                },
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

            _controller = new HRController(_mockUserManager.Object, _mockContext.Object);
        }

        // Test for Dashboard
        [Fact]
        public async Task Dashboard_ReturnsViewWithViewModel()
        {
            // Act
            var result = await _controller.Dashboard();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<HRDashboardViewModel>(viewResult.Model);
            Assert.Equal(2, model.TotalEmployees); // 2 employees
            Assert.Equal(1, model.PendingLeaveRequests); // 1 pending request
        }

        // Test for ManageEmployees
        [Fact]
        public void ManageEmployees_ReturnsViewWithEmployees()
        {
            // Arrange
            var searchString = "";

            // Act
            var result = _controller.ManageEmployees(searchString);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<List<Employee>>(viewResult.Model);
            Assert.Equal(2, model.Count); // Updated to 2 employees
        }

        //Test for GET AddEmployee
        [Fact]
        public void AddEmployee_Get_ReturnsViewWithNewEmployeeModel()
        {
            // Act
            var result = _controller.AddEmployee();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<Employee>(viewResult.Model);
            Assert.NotNull(model);
        }

        // Test for POST AddEmployee
        [Fact]
        public async Task AddEmployee_Post_RedirectsToManageEmployees()
        {
            // Arrange
            var model = new Employee { Name = "New Employee", Email = "new@example.com" };
            _mockUserManager.Setup(um => um.CreateAsync(It.IsAny<Employee>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _controller.AddEmployee(model, "Password@123");

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("ManageEmployees", redirectResult.ActionName);
        }

        // Test for POST UpdateEmployee
        [Fact]
        public async Task UpdateEmployee_Post_ReturnsSuccess()
        {
            // Arrange
            var model = new Employee { Id = "1", Name = "Updated Name", Email = "updated@example.com" };
            _mockUserManager.Setup(um => um.FindByIdAsync("1")).ReturnsAsync(new Employee { Id = "1", Name = "John Doe", Email = "john@example.com" });
            _mockUserManager.Setup(um => um.UpdateAsync(It.IsAny<Employee>())).ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _controller.UpdateEmployee(model);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var successProperty = jsonResult.Value.GetType().GetProperty("success");
            Assert.NotNull(successProperty);
            var successValue = successProperty.GetValue(jsonResult.Value);
            Assert.NotNull(successValue);
            Assert.True((bool)successValue);
        }

        // Test for DeleteEmployee
        [Fact]
        public async Task DeleteEmployee_ValidId_RedirectsToManageEmployees()
        {
            // Arrange
            var employee = new Employee { Id = "1", Name = "John Doe" };
            _mockUserManager.Setup(um => um.FindByIdAsync("1")).ReturnsAsync(employee);
            _mockUserManager.Setup(um => um.DeleteAsync(employee)).ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _controller.DeleteEmployee("1");

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("ManageEmployees", redirectResult.ActionName);
        }

        // Test for GET UpdateLeaveDays
        [Fact]
        public async Task UpdateLeaveDays_Get_ReturnsViewWithViewModel()
        {
            // Arrange
            var employeeId = "1";
            var employee = new Employee
            {
                Id = employeeId,
                Name = "John Doe",
                Email = "john@example.com",
                AnnualLeaveDays = 10,
                BonusLeaveDays = 5
            };

            _mockUserManager.Setup(um => um.FindByIdAsync(employeeId))
                .ReturnsAsync(employee);

            // Act
            var result = await _controller.UpdateLeaveDays(employeeId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<UpdateLeaveDaysViewModel>(viewResult.Model);
            Assert.NotNull(model);
            Assert.Equal(employee.Id, model.EmployeeId);
            Assert.Equal(employee.AnnualLeaveDays, model.AnnualLeaveDays);
            Assert.Equal(employee.BonusLeaveDays, model.BonusLeaveDays);
            Assert.Equal(employee.Email, model.Email);
            Assert.Equal("", model.NewPassword);
            Assert.Equal("", model.ConfirmNewPassword);
        }

        // Test for POST UpdateLeaveDays
        [Fact]
        public async Task UpdateLeaveDays_Post_RedirectsToManageEmployees()
        {
            // Arrange
            var model = new UpdateLeaveDaysViewModel
            {
                EmployeeId = "1",
                AnnualLeaveDays = 15,
                BonusLeaveDays = 10,
                Email = "john@example.com",
                NewPassword = "",
                ConfirmNewPassword = ""
            };

            var employee = new Employee
            {
                Id = "1",
                Name = "John Doe",
                Email = "john@example.com",
                AnnualLeaveDays = 10,
                BonusLeaveDays = 5
            };

            _mockUserManager.Setup(um => um.FindByIdAsync(model.EmployeeId))
                .ReturnsAsync(employee);

            _mockContext.Setup(c => c.SaveChangesAsync(default))
                .ReturnsAsync(1);

            // Act
            var result = await _controller.UpdateLeaveDays(model);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("ManageEmployees", redirectResult.ActionName);
            Assert.Equal(15, employee.AnnualLeaveDays);
            Assert.Equal(10, employee.BonusLeaveDays);
        }

        // Test for GenerateLeaveReport
        [Fact]
        public async Task GenerateLeaveReport_ReturnsViewWithReport()
        {
            // Arrange
            var employees = new List<Employee>
            {
                new Employee
                {
                    Id = "1",
                    Name = "John Doe",
                    AnnualLeaveDays = 10,
                    BonusLeaveDays = 5,
                    SickLeaveDays = 3,
                    VacationRequests = new List<VacationRequest>
                    {
                        new VacationRequest
                        {
                            Id = 1,
                            EmployeeId = "1",
                            StartDate = DateTime.Now,
                            EndDate = DateTime.Now.AddDays(5),
                            IsApproved = true,
                            IsAnnualLeave = true
                        },
                        new VacationRequest
                        {
                            Id = 2,
                            EmployeeId = "1",
                            StartDate = DateTime.Now,
                            EndDate = DateTime.Now.AddDays(3),
                            IsApproved = true,
                            IsAnnualLeave = false
                        }
                    },
                    SickLeaveRequests = new List<SickLeaveRequest>
                    {
                        new SickLeaveRequest
                        {
                            Id = 1,
                            EmployeeId = "1",
                            StartDate = DateTime.Now,
                            EndDate = DateTime.Now.AddDays(2),
                            IsApproved = true
                        }
                    }
                },
                new Employee
                {
                    Id = "2",
                    Name = "Jane Doe",
                    AnnualLeaveDays = 15,
                    BonusLeaveDays = 2,
                    SickLeaveDays = 0,
                    VacationRequests = new List<VacationRequest>
                    {
                        new VacationRequest
                        {
                            Id = 3,
                            EmployeeId = "2",
                            StartDate = DateTime.Now,
                            EndDate = DateTime.Now.AddDays(2),
                            IsApproved = true,
                            IsAnnualLeave = true
                        }
                    },
                    SickLeaveRequests = new List<SickLeaveRequest>()
                }
            };

            var mockEmployeeSet = new Mock<DbSet<Employee>>();
            mockEmployeeSet.As<IAsyncEnumerable<Employee>>()
                .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
                .Returns(new TestAsyncEnumerator<Employee>(employees.GetEnumerator()));

            mockEmployeeSet.As<IQueryable<Employee>>()
                .Setup(m => m.Provider)
                .Returns(new TestAsyncQueryProvider<Employee>(employees.AsQueryable().Provider));

            mockEmployeeSet.As<IQueryable<Employee>>()
                .Setup(m => m.Expression)
                .Returns(employees.AsQueryable().Expression);

            mockEmployeeSet.As<IQueryable<Employee>>()
                .Setup(m => m.ElementType)
                .Returns(employees.AsQueryable().ElementType);

            mockEmployeeSet.As<IQueryable<Employee>>()
                .Setup(m => m.GetEnumerator())
                .Returns(employees.GetEnumerator());

            _mockContext.Setup(c => c.Employees)
                .Returns(mockEmployeeSet.Object);

            // Act
            var result = await _controller.GenerateLeaveReport();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<List<EmployeeLeaveReportViewModel>>(viewResult.Model);
            Assert.Equal(2, model.Count); // Ensure there are 2 employees in the report

            // Verify the first employee's leave report
            var johnDoeReport = model.FirstOrDefault(e => e.EmployeeName == "John Doe");
            Assert.NotNull(johnDoeReport);
            Assert.Equal(6, johnDoeReport.AnnualLeaveUsed);
            Assert.Equal(10, johnDoeReport.AnnualLeaveRemaining);
            Assert.Equal(4, johnDoeReport.BonusLeaveUsed);
            Assert.Equal(5, johnDoeReport.BonusLeaveRemaining);
            Assert.Equal(3, johnDoeReport.SickLeaveUsed);

            // Verify the second employee's leave report
            var janeDoeReport = model.FirstOrDefault(e => e.EmployeeName == "Jane Doe");
            Assert.NotNull(janeDoeReport);
            Assert.Equal(3, janeDoeReport.AnnualLeaveUsed);
            Assert.Equal(15, janeDoeReport.AnnualLeaveRemaining);
            Assert.Equal(0, janeDoeReport.BonusLeaveUsed);
            Assert.Equal(2, janeDoeReport.BonusLeaveRemaining);
            Assert.Equal(0, janeDoeReport.SickLeaveUsed);
        }

        // Test for ExportLeaveReportToExcel
        [Fact]
        public async Task ExportLeaveReportToExcel_ReturnsFileResult()
        {
            // Arrange
            var employees = new List<Employee>
            {
                new Employee
                {
                    Id = "1",
                    Name = "John Doe",
                    AnnualLeaveDays = 10,
                    BonusLeaveDays = 5,
                    SickLeaveDays = 3,
                    VacationRequests = new List<VacationRequest>
                    {
                        new VacationRequest
                        {
                            Id = 1,
                            EmployeeId = "1",
                            StartDate = DateTime.Now,
                            EndDate = DateTime.Now.AddDays(5),
                            IsApproved = true,
                            IsAnnualLeave = true
                        },
                        new VacationRequest
                        {
                            Id = 2,
                            EmployeeId = "1",
                            StartDate = DateTime.Now,
                            EndDate = DateTime.Now.AddDays(3),
                            IsApproved = true,
                            IsAnnualLeave = false
                        }
                    },
                    SickLeaveRequests = new List<SickLeaveRequest>
                    {
                        new SickLeaveRequest
                        {
                            Id = 1,
                            EmployeeId = "1",
                            StartDate = DateTime.Now,
                            EndDate = DateTime.Now.AddDays(2),
                            IsApproved = true
                        }
                    }
                },
                new Employee
                {
                    Id = "2",
                    Name = "Jane Doe",
                    AnnualLeaveDays = 15,
                    BonusLeaveDays = 2,
                    SickLeaveDays = 0,
                    VacationRequests = new List<VacationRequest>
                    {
                        new VacationRequest
                        {
                            Id = 3,
                            EmployeeId = "2",
                            StartDate = DateTime.Now,
                            EndDate = DateTime.Now.AddDays(2),
                            IsApproved = true,
                            IsAnnualLeave = true
                        }
                    },
                    SickLeaveRequests = new List<SickLeaveRequest>()
                }
            };

            var mockEmployeeSet = new Mock<DbSet<Employee>>();
            mockEmployeeSet.As<IAsyncEnumerable<Employee>>()
                .Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
                .Returns(new TestAsyncEnumerator<Employee>(employees.GetEnumerator()));

            mockEmployeeSet.As<IQueryable<Employee>>()
                .Setup(m => m.Provider)
                .Returns(new TestAsyncQueryProvider<Employee>(employees.AsQueryable().Provider));

            mockEmployeeSet.As<IQueryable<Employee>>()
                .Setup(m => m.Expression)
                .Returns(employees.AsQueryable().Expression);

            mockEmployeeSet.As<IQueryable<Employee>>()
                .Setup(m => m.ElementType)
                .Returns(employees.AsQueryable().ElementType);

            mockEmployeeSet.As<IQueryable<Employee>>()
                .Setup(m => m.GetEnumerator())
                .Returns(employees.GetEnumerator());

            _mockContext.Setup(c => c.Employees)
                .Returns(mockEmployeeSet.Object);

            // Act
            var result = await _controller.ExportLeaveReportToExcel();

            // Assert
            var fileResult = Assert.IsType<FileStreamResult>(result); 
            Assert.Equal("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileResult.ContentType);
            Assert.Equal("LeaveReport.xlsx", fileResult.FileDownloadName);

            using (var stream = fileResult.FileStream)
            using (var package = new ExcelPackage(stream))
            {
                var worksheet = package.Workbook.Worksheets["Leave Report"];
                Assert.NotNull(worksheet);

                // Verify headers
                Assert.Equal("Employee", worksheet.Cells[1, 1].Text);
                Assert.Equal("Annual Leave Used", worksheet.Cells[1, 2].Text);
                Assert.Equal("Annual Leave Remaining", worksheet.Cells[1, 3].Text);
                Assert.Equal("Bonus Leave Used", worksheet.Cells[1, 4].Text);
                Assert.Equal("Bonus Leave Remaining", worksheet.Cells[1, 5].Text);
                Assert.Equal("Sick Leave Used", worksheet.Cells[1, 6].Text);

                // Verify data for the first employee (John Doe)
                Assert.Equal("John Doe", worksheet.Cells[2, 1].Text);
                Assert.Equal(6, worksheet.Cells[2, 2].GetValue<int>()); 
                Assert.Equal(10, worksheet.Cells[2, 3].GetValue<int>()); 
                Assert.Equal(4, worksheet.Cells[2, 4].GetValue<int>()); 
                Assert.Equal(5, worksheet.Cells[2, 5].GetValue<int>()); 
                Assert.Equal(3, worksheet.Cells[2, 6].GetValue<int>()); 

                // Verify data for the second employee (Jane Doe)
                Assert.Equal("Jane Doe", worksheet.Cells[3, 1].Text);
                Assert.Equal(3, worksheet.Cells[3, 2].GetValue<int>()); 
                Assert.Equal(15, worksheet.Cells[3, 3].GetValue<int>()); 
                Assert.Equal(0, worksheet.Cells[3, 4].GetValue<int>()); 
                Assert.Equal(2, worksheet.Cells[3, 5].GetValue<int>()); 
                Assert.Equal(0, worksheet.Cells[3, 6].GetValue<int>()); 
            }
        }
    }

    internal class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
    {
        private readonly IQueryProvider _inner;

        internal TestAsyncQueryProvider(IQueryProvider inner)
        {
            _inner = inner;
        }

        public IQueryable CreateQuery(Expression expression)
        {
            return new TestAsyncEnumerable<TEntity>(expression);
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            return new TestAsyncEnumerable<TElement>(expression);
        }

        public object Execute(Expression expression)
        {
            return _inner.Execute(expression);
        }

        public TResult Execute<TResult>(Expression expression)
        {
            return _inner.Execute<TResult>(expression);
        }

        public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
        {
            var expectedResultType = typeof(TResult).GetGenericArguments()[0];
            var executionResult = typeof(IQueryProvider)
                .GetMethod(
                    name: nameof(IQueryProvider.Execute),
                    genericParameterCount: 1,
                    types: new[] { typeof(Expression) })
                .MakeGenericMethod(expectedResultType)
                .Invoke(this, new[] { expression });

            return (TResult)typeof(Task).GetMethod(nameof(Task.FromResult))
                .MakeGenericMethod(expectedResultType)
                .Invoke(null, new[] { executionResult });
        }
    }

    internal class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
    {
        public TestAsyncEnumerable(IEnumerable<T> enumerable)
            : base(enumerable)
        { }

        public TestAsyncEnumerable(Expression expression)
            : base(expression)
        { }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
        }

        IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);
    }

    internal class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
    {
        private readonly IEnumerator<T> _inner;

        public TestAsyncEnumerator(IEnumerator<T> inner)
        {
            _inner = inner;
        }

        public T Current => _inner.Current;

        public ValueTask DisposeAsync()
        {
            _inner.Dispose();
            return new ValueTask();
        }

        public ValueTask<bool> MoveNextAsync()
        {
            return new ValueTask<bool>(_inner.MoveNext());
        }
    }
}
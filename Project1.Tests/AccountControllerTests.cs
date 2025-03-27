using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authentication;
using Moq;
using project1.Controllers;
using project1.Models;
using project1.ViewModels;
using Xunit;

namespace Project1.Tests.Controllers
{
    public class AccountControllerTests
    {
        private readonly Mock<UserManager<Employee>> _mockUserManager;
        private readonly Mock<SignInManager<Employee>> _mockSignInManager;
        private readonly AccountController _controller;

        public AccountControllerTests()
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

            var contextAccessor = new Mock<IHttpContextAccessor>();
            var userPrincipalFactory = new Mock<IUserClaimsPrincipalFactory<Employee>>();
            _mockSignInManager = new Mock<SignInManager<Employee>>(
                _mockUserManager.Object,
                contextAccessor.Object,
                userPrincipalFactory.Object,
                new Mock<IOptions<IdentityOptions>>().Object,
                new Mock<ILogger<SignInManager<Employee>>>().Object,
                new Mock<IAuthenticationSchemeProvider>().Object,
                new Mock<IUserConfirmation<Employee>>().Object
            );

            _controller = new AccountController(_mockSignInManager.Object, _mockUserManager.Object);
        }
        // Test for GET Login
        [Fact]
        public void Login_Get_ReturnsView()
        {
            // Act
            var result = _controller.Login();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.ViewName);
        }

        // Test for POST Valid Login
        [Fact]
        public async Task Login_ValidCredentials_RedirectsToDashboard()
        {
            // Arrange
            var user = new Employee { Email = "test@example.com" };
            _mockUserManager.Setup(um => um.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(user);
            _mockSignInManager.Setup(sm => sm.PasswordSignInAsync(user, "password", false, false))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success); // Use fully qualified name
            _mockUserManager.Setup(um => um.GetRolesAsync(user)).ReturnsAsync(new[] { "Employee" });

            var model = new LoginViewModel { Email = "test@example.com", Password = "password" };

            // Act
            var result = await _controller.Login(model);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Dashboard", redirectResult.ActionName);
            Assert.Equal("Employee", redirectResult.ControllerName);
        }

        // Test for Invalid Login
        [Fact]
        public async Task Login_InvalidCredentials_ReturnsViewWithError()
        {
            // Arrange
            _mockUserManager.Setup(um => um.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((Employee)null!);
            var model = new LoginViewModel { Email = "test@example.com", Password = "wrongpassword" };

            // Act
            var result = await _controller.Login(model);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.False(_controller.ModelState.IsValid);
        }

        // Test for GET Register
        [Fact]
        public void Register_Get_ReturnsView()
        {
            // Act
            var result = _controller.Register();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.ViewName);
        }

        // Test for POST Register
        [Fact]
        public async Task Register_RedirectsToLogin()
        {
            // Arrange
            var model = new RegisterViewModel { Name = "Test User", Email = "test@example.com", Password = "Password@123", ConfirmPassword = "Password@123" };
            _mockUserManager.Setup(um => um.CreateAsync(It.IsAny<Employee>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _controller.Register(model);

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirectResult.ActionName);
        }

        // Test for Logout
        [Fact]
        public async Task Logout_RedirectsToLogin()
        {
            // Arrange
            _mockSignInManager.Setup(sm => sm.SignOutAsync()).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Logout();

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirectResult.ActionName);
        }

    }
}
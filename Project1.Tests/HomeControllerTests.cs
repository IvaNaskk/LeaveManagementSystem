using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using project1.Controllers;
using project1.Models;
using Xunit;

namespace Project1.Tests.Controllers
{
    // Test for HR
    public class HomeControllerTests
    {
        [Fact]
        public void Index_AuthenticatedHRUser_RedirectsToHRDashboard()
        {
            // Arrange
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Role, "HR")
            }, "mock"));

            var controller = new HomeController(Mock.Of<ILogger<HomeController>>())
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = user }
                }
            };

            // Act
            var result = controller.Index();

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Dashboard", redirectResult.ActionName);
            Assert.Equal("HR", redirectResult.ControllerName);
        }

        // Test for Employee
        [Fact]
        public void Index_AuthenticatedEmployeeUser_RedirectsToEmployeeDashboard()
        {
            // Arrange
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Role, "Employee")
            }, "mock"));

            var controller = new HomeController(Mock.Of<ILogger<HomeController>>())
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = user }
                }
            };

            // Act
            var result = controller.Index();

            // Assert
            var redirectResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Dashboard", redirectResult.ActionName);
            Assert.Equal("Employee", redirectResult.ControllerName);
        }

        // Test for Unauthenticated User
        [Fact]
        public void Index_UnauthenticatedUser_ReturnsView()
        {
            // Arrange
            var user = new ClaimsPrincipal(new ClaimsIdentity()); // Unauthenticated user
            var controller = new HomeController(Mock.Of<ILogger<HomeController>>())
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = user }
                }
            };

            // Act
            var result = controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.ViewName); 
        }

        // Test for Privacy only for HR
        [Fact]
        public void Privacy_AuthenticatedHRUser_ReturnsView()
        {
            // Arrange
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.Role, "HR")
            }, "mock"));

            var controller = new HomeController(Mock.Of<ILogger<HomeController>>())
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = user }
                }
            };

            // Act
            var result = controller.Privacy();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(viewResult.ViewName); 
        }

        // Test for for Privacy for Unauthenticated User
        [Fact]
        public void Privacy_UnauthenticatedUser_ReturnsForbidden()
        {
            // Arrange
            var user = new ClaimsPrincipal(new ClaimsIdentity()); // Unauthenticated user
            var controller = new HomeController(Mock.Of<ILogger<HomeController>>())
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = user }
                }
            };

            // Act
            var result = controller.Privacy();

            // Assert
            var forbidResult = Assert.IsType<ForbidResult>(result);
        }

        // Test for Error
        [Fact]
        public void Error_ReturnsViewWithErrorViewModel()
        {
            // Arrange
            var controller = new HomeController(Mock.Of<ILogger<HomeController>>())
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext()
                }
            };

            // Act
            var result = controller.Error();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ErrorViewModel>(viewResult.Model);
            Assert.NotNull(model.RequestId); 
        }
    }
}
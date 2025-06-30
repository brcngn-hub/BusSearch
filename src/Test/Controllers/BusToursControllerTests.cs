using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Test.Helpers;
using Web.Controllers;
using Xunit;

namespace Test.Controllers
{
    public class BusToursControllerTests
    {
        private readonly Mock<IBusTourService> _mockBusTourService;
        private readonly Mock<ILogger<BusToursController>> _mockLogger;
        private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
        private readonly BusToursController _controller;

        public BusToursControllerTests()
        {
            _mockBusTourService = new Mock<IBusTourService>();
            _mockLogger = new Mock<ILogger<BusToursController>>();
            _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            _controller = new BusToursController(_mockBusTourService.Object, _mockLogger.Object, _mockHttpContextAccessor.Object);
        }

        [Fact]
        public async Task Index_WithValidParameters_ReturnsViewResult()
        {
            // Arrange
            var sessionId = "test-session-id";
            var deviceId = "test-device-id";
            var search = "test";
            _mockBusTourService.Setup(x => x.GetBusLocationsAsync(sessionId, deviceId, null)).ReturnsAsync(TestDataBuilder.CreateMockLocations());
            var context = new DefaultHttpContext();
            context.Session = new TestSession();
            context.Session.SetString("ObiletSessionId", sessionId);
            context.Session.SetString("ObiletDeviceId", deviceId);
            _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(context);

            // Act
            var result = await _controller.Index();

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult);
        }

        [Fact]
        public async Task BusTours_WithValidParameters_ReturnsViewResult()
        {
            // Arrange
            var originId = "1";
            var destinationId = "2";
            var departureDate = DateTime.Today.AddDays(1);
            var sort = "departureAsc";
            var sessionId = "test-session-id";
            var deviceId = "test-device-id";
            _mockBusTourService.Setup(x => x.GetJourneysAsync(originId, destinationId, departureDate, sessionId, deviceId)).ReturnsAsync(TestDataBuilder.CreateJourneyList());
            _mockBusTourService.Setup(x => x.GetBusLocationsAsync(sessionId, deviceId)).ReturnsAsync(TestDataBuilder.CreateMockLocations());
            var context = new DefaultHttpContext();
            context.Session = new TestSession();
            context.Session.SetString("ObiletSessionId", sessionId);
            context.Session.SetString("ObiletDeviceId", deviceId);
            _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(context);

            // Act
            var result = await _controller.BusTours(originId, destinationId, departureDate, sort);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult);
        }

        [Fact]
        public async Task BusTours_WithEmptyOriginId_ReturnsViewWithError()
        {
            // Arrange
            var originId = "";
            var destinationId = "2";
            var departureDate = DateTime.Today.AddDays(1);
            var sort = "departureAsc";
            var sessionId = "test-session-id";
            var deviceId = "test-device-id";
            var context = new DefaultHttpContext();
            context.Session = new TestSession();
            context.Session.SetString("ObiletSessionId", sessionId);
            context.Session.SetString("ObiletDeviceId", deviceId);
            _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(context);

            // Act
            var result = await _controller.BusTours(originId, destinationId, departureDate, sort);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult);
        }

        [Fact]
        public async Task BusTours_WithServiceException_ReturnsViewWithError()
        {
            // Arrange
            var originId = "1";
            var destinationId = "2";
            var departureDate = DateTime.Today.AddDays(1);
            var sort = "departureAsc";
            var sessionId = "test-session-id";
            var deviceId = "test-device-id";
            _mockBusTourService.Setup(x => x.GetJourneysAsync(originId, destinationId, departureDate, sessionId, deviceId)).ThrowsAsync(new Exception("Service error"));
            var context = new DefaultHttpContext();
            context.Session = new TestSession();
            context.Session.SetString("ObiletSessionId", sessionId);
            context.Session.SetString("ObiletDeviceId", deviceId);
            _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(context);

            // Act
            var result = await _controller.BusTours(originId, destinationId, departureDate, sort);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.NotNull(viewResult);
        }
    }
} 
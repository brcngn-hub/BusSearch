using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Web.Controllers;
using Test.Helpers;

namespace EnginBusTour.Tests.Controllers
{
    public class BusToursApiControllerTests
    {
        private readonly Mock<IBusTourService> _mockBusTourService;
        private readonly Mock<ILogger<BusToursApiController>> _mockLogger;
        private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
        private readonly BusToursApiController _controller;

        public BusToursApiControllerTests()
        {
            _mockBusTourService = new Mock<IBusTourService>();
            _mockLogger = new Mock<ILogger<BusToursApiController>>();
            _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            _controller = new BusToursApiController(_mockBusTourService.Object, _mockLogger.Object, _mockHttpContextAccessor.Object);
            
            var httpContext = new DefaultHttpContext();
            var session = new Mock<ISession>();
            httpContext.Session = session.Object;
            _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);
        }

        [Fact]
        public async Task GetLocations_WithValidSession_ShouldReturnLocations()
        {
            // Arrange
            var session = TestDataBuilder.CreateMockSession();
            var expectedLocations = new List<BusLocationDto>
            {
                new BusLocationDto { Id = "1", Name = "Istanbul", City = "Istanbul", Country = "Turkey" },
                new BusLocationDto { Id = "2", Name = "Ankara", City = "Ankara", Country = "Turkey" }
            };

            _mockBusTourService.Setup(x => x.CreateObiletSessionAsync())
                .ReturnsAsync(session);
            _mockBusTourService.Setup(x => x.GetBusLocationsAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(expectedLocations);

            // Act
            var result = await _controller.GetLocations();

            // Assert
            result.Should().BeOfType<ActionResult<IEnumerable<BusLocationDto>>>();
            var actionResult = result.Result as OkObjectResult;
            actionResult.Should().NotBeNull();
            actionResult.Value.Should().BeEquivalentTo(expectedLocations);
        }

        [Fact]
        public async Task GetLocations_WithSearchTerm_ShouldReturnFilteredLocations()
        {
            // Arrange
            var searchTerm = "Istanbul";
            var session = TestDataBuilder.CreateMockSession();
            var expectedLocations = new List<BusLocationDto>
            {
                new BusLocationDto { Id = "1", Name = "Istanbul", City = "Istanbul", Country = "Turkey" }
            };

            _mockBusTourService.Setup(x => x.CreateObiletSessionAsync())
                .ReturnsAsync(session);
            _mockBusTourService.Setup(x => x.GetBusLocationsAsync(It.IsAny<string>(), It.IsAny<string>(), searchTerm))
                .ReturnsAsync(expectedLocations);

            // Act
            var result = await _controller.GetLocations(searchTerm);

            // Assert
            result.Should().BeOfType<ActionResult<IEnumerable<BusLocationDto>>>();
            var actionResult = result.Result as OkObjectResult;
            actionResult.Should().NotBeNull();
            actionResult.Value.Should().BeEquivalentTo(expectedLocations);
        }

        [Fact]
        public async Task GetJourneys_WithValidParameters_ShouldReturnJourneys()
        {
            // Arrange
            var originId = "1";
            var destinationId = "2";
            var departureDate = DateTime.Now.AddDays(1);
            var session = TestDataBuilder.CreateMockSession();
            var expectedJourneys = new List<BusTourDto>
            {
                new BusTourDto
                {
                    Name = "Istanbul - Ankara",
                    DepartureDate = departureDate,
                    Price = 150.0m
                }
            };

            _mockBusTourService.Setup(x => x.CreateObiletSessionAsync())
                .ReturnsAsync(session);
            _mockBusTourService.Setup(x => x.GetJourneysAsync(originId, destinationId, departureDate, It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(expectedJourneys);

            // Act
            var result = await _controller.GetJourneys(originId, destinationId, departureDate);

            // Assert
            result.Should().BeOfType<ActionResult<IEnumerable<BusTourDto>>>();
            var actionResult = result.Result as OkObjectResult;
            actionResult.Should().NotBeNull();
            actionResult.Value.Should().BeEquivalentTo(expectedJourneys);
        }

        [Fact]
        public async Task GetJourneys_WithMissingParameters_ShouldReturnBadRequest()
        {
            // Arrange
            var originId = "";
            var destinationId = "";
            var departureDate = DateTime.Now.AddDays(1);

            // Act
            var result = await _controller.GetJourneys(originId, destinationId, departureDate);

            // Assert
            result.Should().BeOfType<ActionResult<IEnumerable<BusTourDto>>>();
            var actionResult = result.Result as BadRequestObjectResult;
            actionResult.Should().NotBeNull();
            actionResult.Value.Should().Be("Kalkış noktası ID'si ve varış noktası ID'si gereklidir");
        }
    }
} 
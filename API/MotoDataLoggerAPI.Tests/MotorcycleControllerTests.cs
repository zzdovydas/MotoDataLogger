//c:\Users\Dovydas\Documents\MotoDataLogger\API\MotoDataLoggerAPI.Tests\MotorcycleControllerTests.cs
using MotoDataLoggerAPI.Controllers;
using MotoDataLoggerAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using MotoDataLoggerAPI.Repository;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace MotoDataLoggerAPI.Tests.Controller
{
    public class MotorcycleControllerTests : IAsyncLifetime
    {
        private readonly MotorcycleController _controller;
        private readonly IMotorcycleRepository _repository;
        private readonly MotoDataContext _context;

        public MotorcycleControllerTests()
        {
            var options = new DbContextOptionsBuilder<MotoDataContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            _context = new MotoDataContext(options);
            _repository = new MotorcycleRepository(_context);
            _controller = new MotorcycleController(_repository);
        }
        private async Task ClearDatabase()
        {
            _context.Motorcycles.RemoveRange(_context.Motorcycles);
            await _context.SaveChangesAsync();
        }
        public async Task InitializeAsync()
        {
            await ClearDatabase();
        }
        public async Task DisposeAsync()
        {
            await _context.DisposeAsync();
        }

        [Fact]
        public async Task GetMotorcycles_ReturnsOkResult_WithMotorcycleList()
        {
            await _repository.AddMotorcycleAsync(new Motorcycle { Manufacturer = "Honda", Model = "CBR", Year = 2020 });
            await _repository.AddMotorcycleAsync(new Motorcycle { Manufacturer = "Yamaha", Model = "YZF", Year = 2022 });

            var result = await _controller.GetMotorcycles();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var motorcycles = Assert.IsAssignableFrom<List<Motorcycle>>(okResult.Value);
            Assert.NotEmpty(motorcycles);
            Assert.Equal(2, motorcycles.Count);
        }
        [Fact]
        public async Task GetMotorcycle_ExistingId_ReturnsOkResult_WithMotorcycle()
        {
            var createdMotorcycle = await _repository.AddMotorcycleAsync(new Motorcycle { Manufacturer = "Honda", Model = "CBR", Year = 2020 });

            var result = await _controller.GetMotorcycle(createdMotorcycle.Id);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var motorcycle = Assert.IsType<Motorcycle>(okResult.Value);
            Assert.Equal(createdMotorcycle.Id, motorcycle.Id);
        }

        [Fact]
        public async Task GetMotorcycle_NonExistingId_ReturnsNotFoundResult()
        {
            var result = await _controller.GetMotorcycle(999);

            Assert.IsType<NotFoundObjectResult>(result);
            var notFoundResult = result as NotFoundObjectResult;
            Assert.Equal("Motorcycle not found", notFoundResult.Value);
        }

        [Fact]
        public async Task AddMotorcycle_ValidMotorcycle_ReturnsCreatedAtAction()
        {
            var newMotorcycle = new Motorcycle { Manufacturer = "Suzuki", Model = "GSX", Year = 2023 };

            var result = await _controller.AddMotorcycle(newMotorcycle);

            Assert.IsType<CreatedAtActionResult>(result);
            var createdAtActionResult = result as CreatedAtActionResult;
            Assert.Equal(nameof(MotorcycleController.GetMotorcycle), createdAtActionResult.ActionName);
            Assert.NotNull(createdAtActionResult.Value);
            var returnedMotorcycle = Assert.IsType<Motorcycle>(createdAtActionResult.Value);
            Assert.Equal(newMotorcycle.Manufacturer, returnedMotorcycle.Manufacturer);
        }

        [Fact]
        public async Task AddMotorcycle_NullMotorcycle_ReturnsBadRequest()
        {
            var result = await _controller.AddMotorcycle(null);

            Assert.IsType<BadRequestObjectResult>(result);
            var badRequestResult = result as BadRequestObjectResult;
            Assert.Equal("Motorcycle data is null", badRequestResult.Value);
        }

        [Fact]
        public async Task AddMotorcycle_InvalidModel_ReturnsBadRequest()
        {
            var invalidMotorcycle = new Motorcycle { Manufacturer = "Suzuki", Model = "", Year = 2023 };
            _controller.ModelState.AddModelError("Model", "The Model field is required.");

            var result = await _controller.AddMotorcycle(invalidMotorcycle);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task UpdateMotorcycle_ExistingId_ValidData_ReturnsOkResult()
        {
            var createdMotorcycle = await _repository.AddMotorcycleAsync(new Motorcycle { Manufacturer = "Honda", Model = "CBR", Year = 2020 });

            var updatedMotorcycle = new Motorcycle { Id = createdMotorcycle.Id, Manufacturer = "UpdatedManufacturer", Model = "UpdatedModel", Year = 2021, Description = "Updated" };

            var result = await _controller.UpdateMotorcycle(createdMotorcycle.Id, updatedMotorcycle);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var motorcycle = Assert.IsType<Motorcycle>(okResult.Value);
            Assert.Equal(createdMotorcycle.Id, motorcycle.Id);
            Assert.Equal(updatedMotorcycle.Manufacturer, motorcycle.Manufacturer);
            Assert.Equal(updatedMotorcycle.Model, motorcycle.Model);
            Assert.Equal(updatedMotorcycle.Year, motorcycle.Year);
            Assert.Equal(updatedMotorcycle.Description, motorcycle.Description);

        }
        [Fact]
        public async Task UpdateMotorcycle_NullMotorcycle_ReturnsBadRequest()
        {
            var createdMotorcycle = await _repository.AddMotorcycleAsync(new Motorcycle { Manufacturer = "Honda", Model = "CBR", Year = 2020 });

            var result = await _controller.UpdateMotorcycle(createdMotorcycle.Id, null);

            Assert.IsType<BadRequestObjectResult>(result);
            var badRequestResult = result as BadRequestObjectResult;
            Assert.Equal("Motorcycle is null", badRequestResult.Value);
        }

        [Fact]
        public async Task UpdateMotorcycle_NonExistingId_ReturnsNotFoundResult()
        {
            var updatedMotorcycle = new Motorcycle { Id = 999, Manufacturer = "UpdatedManufacturer", Model = "UpdatedModel", Year = 2021, Description = "Updated" };

            var result = await _controller.UpdateMotorcycle(999, updatedMotorcycle);

            Assert.IsType<NotFoundObjectResult>(result);
            var notFoundResult = result as NotFoundObjectResult;
            Assert.Equal("Motorcycle not found", notFoundResult.Value);
        }

        [Fact]
        public async Task UpdateMotorcycle_WrongId_ReturnsBadRequest()
        {
            var createdMotorcycle = await _repository.AddMotorcycleAsync(new Motorcycle { Manufacturer = "Honda", Model = "CBR", Year = 2020 });
            var updatedMotorcycle = new Motorcycle { Id = 998, Manufacturer = "UpdatedManufacturer", Model = "UpdatedModel", Year = 2021, Description = "Updated" };

            var result = await _controller.UpdateMotorcycle(createdMotorcycle.Id, updatedMotorcycle);

            Assert.IsType<BadRequestObjectResult>(result);
            var badRequestResult = result as BadRequestObjectResult;
            Assert.Equal("Id is incorrect", badRequestResult.Value);
        }
        [Fact]
        public async Task DeleteMotorcycle_ExistingId_ReturnsNoContentResult()
        {
            var createdMotorcycle = await _repository.AddMotorcycleAsync(new Motorcycle { Manufacturer = "Honda", Model = "CBR", Year = 2020 });

            var result = await _controller.DeleteMotorcycle(createdMotorcycle.Id);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteMotorcycle_NonExistingId_ReturnsNotFoundResult()
        {
            var result = await _controller.DeleteMotorcycle(999);

            Assert.IsType<NotFoundObjectResult>(result);
            var notFoundResult = result as NotFoundObjectResult;
            Assert.Equal("Motorcycle not found", notFoundResult.Value);
        }
    }
}

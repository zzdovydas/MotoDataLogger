using MotoDataLoggerAPI.Controllers;
using MotoDataLoggerAPI.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Xunit;
using FluentAssertions;
using System.Reflection;
using System;

namespace MotoDataLoggerAPI.Tests.Controller
{
    public class MotorcycleControllerTests : IDisposable // Implement IDisposable
    {
        private readonly MotorcycleController _controller;
        private static List<Motorcycle> _motorcycles;

        public MotorcycleControllerTests()
        {
            _controller = new MotorcycleController();
            
            FieldInfo field = typeof(MotorcycleController).GetField("_motorcycles", BindingFlags.NonPublic | BindingFlags.Static);
            _motorcycles = (List<Motorcycle>)field.GetValue(null);
        }

        [Fact]
        public void GetMotorcycles_ReturnsOkResult_WithMotorcycleList()
        {
            // Arrange
            _controller.AddMotorcycle(new Motorcycle { Manufacturer = "Honda", Model = "CBR", Year = 2020 });
            _controller.AddMotorcycle(new Motorcycle { Manufacturer = "Yamaha", Model = "YZF", Year = 2022 });

            // Act
            var result = _controller.GetMotorcycles();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var motorcycles = Assert.IsAssignableFrom<List<Motorcycle>>(okResult.Value);
            Assert.NotEmpty(motorcycles);
            motorcycles.Count.Should().Be(2);
        }
        [Fact]
        public void GetMotorcycle_ExistingId_ReturnsOkResult_WithMotorcycle()
        {
            // Arrange
            var createdMotorcycle = _controller.AddMotorcycle(new Motorcycle { Manufacturer = "Honda", Model = "CBR", Year = 2020 }) as CreatedAtActionResult;
            Assert.NotNull(createdMotorcycle);
            var motorcycleId = (createdMotorcycle.Value as Motorcycle)?.Id;
            Assert.NotNull(motorcycleId);

            // Act
            var result = _controller.GetMotorcycle(motorcycleId.Value);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var motorcycle = Assert.IsType<Motorcycle>(okResult.Value);
            Assert.Equal(motorcycleId, motorcycle.Id);
        }

        [Fact]
        public void GetMotorcycle_NonExistingId_ReturnsNotFoundResult()
        {
            // Act
            var result = _controller.GetMotorcycle(999); // Non-existing ID

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public void AddMotorcycle_ValidMotorcycle_ReturnsCreatedAtAction()
        {
            // Arrange
            var newMotorcycle = new Motorcycle { Manufacturer = "Suzuki", Model = "GSX", Year = 2023 };

            // Act
            var result = _controller.AddMotorcycle(newMotorcycle) as CreatedAtActionResult;
            Assert.NotNull(result);

            // Assert
            Assert.IsType<CreatedAtActionResult>(result);
            Assert.Equal(nameof(MotorcycleController.GetMotorcycle), result.ActionName);
            Assert.NotNull(result.Value);
            var returnedMotorcycle = Assert.IsType<Motorcycle>(result.Value);
            Assert.Equal(newMotorcycle.Manufacturer, returnedMotorcycle.Manufacturer);
        }

        [Fact]
        public void AddMotorcycle_NullMotorcycle_ReturnsBadRequest()
        {
            // Act
            var result = _controller.AddMotorcycle(null);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public void AddMotorcycle_InvalidModel_ReturnsBadRequest()
        {
            // Arrange
            var invalidMotorcycle = new Motorcycle { Manufacturer = "Suzuki", Model = "", Year = 2023 };
            _controller.ModelState.AddModelError("Model", "The Model field is required.");

            // Act
            var result = _controller.AddMotorcycle(invalidMotorcycle) as BadRequestObjectResult;
            Assert.NotNull(result);
            
            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public void UpdateMotorcycle_ExistingId_ValidData_ReturnsOkResult()
        {
            // Arrange
            var createdMotorcycle = _controller.AddMotorcycle(new Motorcycle { Manufacturer = "Honda", Model = "CBR", Year = 2020 }) as CreatedAtActionResult;
            Assert.NotNull(createdMotorcycle);
            var motorcycleId = (createdMotorcycle.Value as Motorcycle)?.Id;
            Assert.NotNull(motorcycleId);

            var updatedMotorcycle = new Motorcycle { Id = motorcycleId.Value, Manufacturer = "UpdatedManufacturer", Model = "UpdatedModel", Year = 2021, Description = "Updated" };

            // Act
            var result = _controller.UpdateMotorcycle(motorcycleId.Value, updatedMotorcycle);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var motorcycle = Assert.IsType<Motorcycle>(okResult.Value);
            Assert.Equal(motorcycleId, motorcycle.Id);
            Assert.Equal(updatedMotorcycle.Manufacturer, motorcycle.Manufacturer);
            Assert.Equal(updatedMotorcycle.Model, motorcycle.Model);
            Assert.Equal(updatedMotorcycle.Year, motorcycle.Year);
            Assert.Equal(updatedMotorcycle.Description, motorcycle.Description);

        }
        [Fact]
        public void UpdateMotorcycle_NullMotorcycle_ReturnsBadRequest()
        {
            // Arrange
            var createdMotorcycle = _controller.AddMotorcycle(new Motorcycle { Manufacturer = "Honda", Model = "CBR", Year = 2020 }) as CreatedAtActionResult;
            Assert.NotNull(createdMotorcycle);
            var motorcycleId = (createdMotorcycle.Value as Motorcycle)?.Id;
            Assert.NotNull(motorcycleId);

            // Act
            var result = _controller.UpdateMotorcycle(motorcycleId.Value, null);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public void UpdateMotorcycle_NonExistingId_ReturnsNotFoundResult()
        {
            // Arrange
            var updatedMotorcycle = new Motorcycle { Id = 999, Manufacturer = "UpdatedManufacturer", Model = "UpdatedModel", Year = 2021, Description = "Updated" };

            // Act
            var result = _controller.UpdateMotorcycle(999, updatedMotorcycle);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public void UpdateMotorcycle_WrongId_ReturnsBadRequest()
        {
            // Arrange
            var createdMotorcycle = _controller.AddMotorcycle(new Motorcycle { Manufacturer = "Honda", Model = "CBR", Year = 2020 }) as CreatedAtActionResult;
            Assert.NotNull(createdMotorcycle);
            var motorcycleId = (createdMotorcycle.Value as Motorcycle)?.Id;
            Assert.NotNull(motorcycleId);
            var updatedMotorcycle = new Motorcycle { Id = 998, Manufacturer = "UpdatedManufacturer", Model = "UpdatedModel", Year = 2021, Description = "Updated" };

            // Act
            var result = _controller.UpdateMotorcycle(motorcycleId.Value, updatedMotorcycle);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }
        [Fact]
        public void DeleteMotorcycle_ExistingId_ReturnsNoContentResult()
        {
            // Arrange
            var createdMotorcycle = _controller.AddMotorcycle(new Motorcycle { Manufacturer = "Honda", Model = "CBR", Year = 2020 }) as CreatedAtActionResult;
            Assert.NotNull(createdMotorcycle);
            var motorcycleId = (createdMotorcycle.Value as Motorcycle)?.Id;
            Assert.NotNull(motorcycleId);

            // Act
            var result = _controller.DeleteMotorcycle(motorcycleId.Value);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public void DeleteMotorcycle_NonExistingId_ReturnsNotFoundResult()
        {
            // Act
            var result = _controller.DeleteMotorcycle(999); // Non-existing ID

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }
        public void Dispose()
        {
            // Clear the list after each test
             _motorcycles.Clear();
        }
    }
}

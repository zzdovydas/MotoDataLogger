using MotoDataLoggerAPI.Controllers;
using MotoDataLoggerAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using MotoDataLoggerAPI.Repository;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using MotoDataLoggerAPI.Data;

namespace MotoDataLoggerAPI.Tests.Controller
{
    public class MotoDataControllerTests : IAsyncLifetime
    {
        private readonly MotoDataController _controller;
        private readonly IMotoDataRepository _repository;
        private readonly MotoDataContext _context;

        public MotoDataControllerTests()
        {
            var options = new DbContextOptionsBuilder<MotoDataContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;
            _context = new MotoDataContext(options);
            _repository = new MotoDataRepository(_context);
            _controller = new MotoDataController(_repository);
        }

        private async Task ClearDatabase()
        {
            _context.MotoDatas.RemoveRange(_context.MotoDatas);
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
        public async Task PostMotoData_ValidData_ReturnsOkResult()
        {
            var motoData = new MotoData
            {
                Timestamp = DateTime.Now,
                LightSensitivity = 10.5,
                MagneticField = 50.2
            };

            var result = await _controller.PostMotoData(motoData);

            Assert.IsType<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            Assert.Equal("Data received successfully", okResult.Value);
        }

        [Fact]
        public async Task PostMotoData_NullData_ReturnsBadRequest()
        {
            var result = await _controller.PostMotoData(null);

            Assert.IsType<BadRequestObjectResult>(result);
            var badRequestResult = result as BadRequestObjectResult;
            Assert.Equal("Data is null", badRequestResult.Value);
        }

        [Fact]
        public async Task GetMotoData_ReturnsOkResult_WithDataList()
        {
            await _repository.AddMotoDataAsync(new MotoData { Timestamp = DateTime.Now, LightSensitivity = 10 });
            await _repository.AddMotoDataAsync(new MotoData { Timestamp = DateTime.Now, LightSensitivity = 11 });

            var result = await _controller.GetMotoData();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var motoDataList = Assert.IsAssignableFrom<List<MotoData>>(okResult.Value);
            Assert.NotEmpty(motoDataList); // Check if the list is not empty
            Assert.Equal(2, motoDataList.Count);
        }

        [Fact]
        public async Task GetMotoData_EmptyList_ReturnsOkResult_WithEmptyList()
        {
            var result = await _controller.GetMotoData();

            var okResult = Assert.IsType<OkObjectResult>(result);
            var motoDataList = Assert.IsAssignableFrom<List<MotoData>>(okResult.Value);
            Assert.Empty(motoDataList);
        }
    }
}

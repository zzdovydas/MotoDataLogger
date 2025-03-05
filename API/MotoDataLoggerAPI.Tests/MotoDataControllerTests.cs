using MotoDataLoggerAPI.Controllers;
using MotoDataLoggerAPI.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using Xunit;
using FluentAssertions;
using System.Reflection;

namespace MotoDataLoggerAPI.Tests.Controller
{
    public class MotoDataControllerTests : IDisposable
    {
        private readonly MotoDataController _controller;
        private static List<MotoData> _motoDataList;

        public MotoDataControllerTests()
        {
            // Create a fresh instance of the controller for each test
            _controller = new MotoDataController();

            // Get the private static _motoDataList list using reflection
            FieldInfo field = typeof(MotoDataController).GetField("_motoDataList", BindingFlags.NonPublic | BindingFlags.Static);
            _motoDataList = (List<MotoData>)field.GetValue(null);
        }

        [Fact]
        public void PostMotoData_ValidData_ReturnsOkResult()
        {
            // Arrange
            var motoData = new MotoData
            {
                Timestamp = DateTime.Now,
                LightSensitivity = 10.5,
                MagneticField = 50.2
            };

            // Act
            var result = _controller.PostMotoData(motoData);

            // Assert
            Assert.IsType<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            Assert.Equal("Data received successfully", okResult.Value);
        }

        [Fact]
        public void PostMotoData_NullData_ReturnsBadRequest()
        {
            // Act
            var result = _controller.PostMotoData(null);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
            var badRequestResult = result as BadRequestObjectResult;
            Assert.Equal("Data is null", badRequestResult.Value);
        }

        [Fact]
        public void GetMotoData_ReturnsOkResult_WithDataList()
        {
            // Arrange
            // Add some MotoData to the list to make sure there is something to test
            _controller.PostMotoData(new MotoData { Timestamp = DateTime.Now, LightSensitivity = 10 });
            _controller.PostMotoData(new MotoData { Timestamp = DateTime.Now, LightSensitivity = 11 });

            // Act
            var result = _controller.GetMotoData();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var motoDataList = Assert.IsAssignableFrom<List<MotoData>>(okResult.Value);
            Assert.NotEmpty(motoDataList); // Check if the list is not empty
            motoDataList.Count.Should().Be(2);
        }

        [Fact]
        public void GetMotoData_EmptyList_ReturnsOkResult_WithEmptyList()
        {
            // Act
            var result = _controller.GetMotoData();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var motoDataList = Assert.IsAssignableFrom<List<MotoData>>(okResult.Value);
            Assert.Empty(motoDataList);
        }
          public void Dispose()
        {
            // Clear the list after each test
            _motoDataList.Clear();
        }
    }
}

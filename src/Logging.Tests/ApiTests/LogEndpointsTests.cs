using FluentAssertions;
using Logging.Abstractions.Contracts;
using Logging.Abstractions.Interfaces;
using Logging.IngestionApi.Endpoints;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Moq;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace Logging.Tests.Endpoints
{
    public class LogEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly Mock<ILogWriter> _writerMock = new();
        private readonly Mock<ILogValidationService> _validatorMock = new();

        public LogEndpointsTests()
        {
            _factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureServices(services =>
                    {
                        services.AddSingleton(_writerMock.Object);
                        services.AddSingleton(_validatorMock.Object);
                    });
                });
        }

        // --------------------------------------------------------
        //  1) Single Log - Valid => 202 Accepted
        // --------------------------------------------------------
        [Fact]
        public async Task SingleLog_Should_Return_Accepted_When_Valid()
        {
            // Arrange
            var log = new LogEvent
            {
                Message = "test message",
                Timestamp = DateTime.UtcNow
            };

            _validatorMock
                .Setup(v => v.Validate(It.IsAny<LogEvent>()))
                .Returns(new List<string>()); // No errors => valid

            var client = _factory.CreateClient();

            // Act
            var response = await client.PostAsJsonAsync("/api/logs", log);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Accepted);
            _writerMock.Verify(w => w.WriteAsync(It.IsAny<LogEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        // --------------------------------------------------------
        //  2) Single Log - Invalid => 400 BadRequest
        // --------------------------------------------------------
        [Fact]
        public async Task SingleLog_Should_Return_BadRequest_When_Invalid()
        {
            // Arrange
            var log = new LogEvent
            {
                Message = "", // invalid
                Timestamp = DateTime.UtcNow
            };

            _validatorMock
                .Setup(v => v.Validate(It.IsAny<LogEvent>()))
                .Returns(new List<string> { "message is required" });

            var client = _factory.CreateClient();

            // Act
            var response = await client.PostAsJsonAsync("/api/logs", log);
            var body = await response.Content.ReadFromJsonAsync<LogEndpoints.ValidationErrorResponse>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            body!.Errors.Should().Contain("message is required");

            _writerMock.Verify(w => w.WriteAsync(It.IsAny<LogEvent>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        // --------------------------------------------------------
        //  3) Batch Log - Valid => 202 Accepted
        // --------------------------------------------------------
        [Fact]
        public async Task BatchLogs_Should_Return_Accepted_When_Valid()
        {
            // Arrange
            var batch = new LogBatch
            {
                Events = new List<LogEvent>
                {
                    new LogEvent { Message = "a", Timestamp = DateTime.UtcNow },
                    new LogEvent { Message = "b", Timestamp = DateTime.UtcNow }
                }
            };

            _validatorMock
                .Setup(v => v.Validate(It.IsAny<LogEvent>()))
                .Returns(new List<string>()); // All valid

            var client = _factory.CreateClient();

            // Act
            var response = await client.PostAsJsonAsync("/api/logs/batch", batch);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Accepted);
            _writerMock.Verify(w => w.WriteBatchAsync(
                It.IsAny<IReadOnlyCollection<LogEvent>>(),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }

        // --------------------------------------------------------
        // 4) Batch Log - Invalid => 400 BadRequest
        // --------------------------------------------------------
        [Fact]
        public async Task BatchLogs_Should_Return_BadRequest_When_Invalid()
        {
            // Arrange
            var batch = new LogBatch
            {
                Events = new List<LogEvent>
                {
                    new LogEvent { Message = "", Timestamp = DateTime.UtcNow }
                }
            };

            _validatorMock
                .Setup(v => v.Validate(It.IsAny<LogEvent>()))
                .Returns(new List<string> { "message is required" });

            var client = _factory.CreateClient();

            // Act
            var response = await client.PostAsJsonAsync("/api/logs/batch", batch);
            var body = await response.Content.ReadFromJsonAsync<LogEndpoints.ValidationErrorResponse>();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
            body!.Errors.Should().Contain("message is required");

            _writerMock.Verify(w => w.WriteBatchAsync(
                It.IsAny<IReadOnlyCollection<LogEvent>>(),
                It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}

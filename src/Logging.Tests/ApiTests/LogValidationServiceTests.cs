using FluentAssertions;
using Logging.Abstractions.Contracts;
using Logging.IngestionApi.Services;
using Xunit;

namespace Logging.Tests.Validation;

public class LogValidationServiceTests
{
    private readonly LogValidationService _service = new();

    [Fact]
    public void Validate_Should_Return_Error_When_Message_Is_Missing()
    {
        var log = new LogEvent
        {
            Timestamp = DateTime.UtcNow,
            Application = "test-app",
            Environment = "dev"
        };

        var errors = _service.Validate(log);

        errors.Should().Contain("message is required.");
    }

    [Fact]
    public void Validate_Should_Return_Error_When_Timestamp_Is_Default()
    {
        var log = new LogEvent
        {
            Message = "hello",
            Application = "test-app",
            Environment = "dev",
            Timestamp = default
        };

        var errors = _service.Validate(log);

        errors.Should().Contain("timestamp is required.");
    }

    [Fact]
    public void Validate_Should_Return_No_Errors_When_Log_Is_Valid()
    {
        var log = new LogEvent
        {
            Message = "hello",
            Timestamp = DateTime.UtcNow,
            Application = "test-app",
            Environment = "dev"
        };

        var errors = _service.Validate(log);

        errors.Should().BeEmpty();
    }

    [Fact]
    public void IsValid_Should_Return_False_When_Errors()
    {
        var log = new LogEvent
        {
            Message = "",
            Timestamp = default
        };

        var result = _service.IsValid(log, out var reason);

        result.Should().BeFalse();
        reason.Should().NotBeNullOrEmpty();
    }
}

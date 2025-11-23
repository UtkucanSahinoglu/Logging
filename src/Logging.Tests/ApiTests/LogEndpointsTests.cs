using FluentAssertions;
using Logging.Abstractions.Contracts;
using Moq;
using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace Logging.Tests.ApiTests;

public class LogEndpointsTests : IClassFixture<TestApplicationFactory>
{
    private readonly TestApplicationFactory _factory;
    private readonly HttpClient _client;

    public LogEndpointsTests(TestApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task SingleLog_Should_Return_Accepted_When_Valid()
    {
        var log = new LogEvent { Message = "test", Timestamp = DateTime.UtcNow };

        _factory.ValidatorMock
            .Setup(v => v.Validate(It.IsAny<LogEvent>()))
            .Returns(new List<string>());

        var response = await _client.PostAsJsonAsync("/api/logs", log);

        response.StatusCode.Should().Be(HttpStatusCode.Accepted);

        _factory.WriterMock.Verify(
            w => w.WriteAsync(It.IsAny<LogEvent>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}

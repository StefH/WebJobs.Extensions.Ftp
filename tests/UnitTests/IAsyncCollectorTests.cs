using System.Linq.Expressions;
using System.Text;
using Bindings.Sample.Ftp;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Moq;
using WebJobs.Extensions.Ftp.Models;

namespace UnitTests;

// ReSharper disable once InconsistentNaming
public class IAsyncCollectorTests
{
    [Fact]
    public async Task RunBindingIAsyncCollectorAsync_With_A_Message_Should_Call_AddAsync()
    {
        // Arrange
        var value = "test";
        var bytes = Encoding.UTF8.GetBytes(value);
        var requestMock = new Mock<HttpRequest>();
        var queryParameters = new Dictionary<string, StringValues>
        {
            { "message", value }
        };
        requestMock.Setup(r => r.Query).Returns(new QueryCollection(queryParameters));

        var collectorMock = new Mock<IAsyncCollector<FtpFile>>();

        // Act
        var response = await FtpBindingsSample.RunBindingIAsyncCollectorAsync(requestMock.Object, collectorMock.Object, Mock.Of<ILogger>());

        // Assert
        response.Should().BeOfType<OkObjectResult>().Which.Value.Should().Be("FtpFile added to IAsyncCollector.");

        // Verify
        Expression<Func<FtpFile, bool>> match = f => f.Name == "stef-asynccollector.txt" &&
                                                     f.Content != null &&
                                                     bytes.SequenceEqual(f.Content);

        collectorMock.Verify(c => c.AddAsync(It.Is(match), It.IsAny<CancellationToken>()), Times.Once);
        collectorMock.VerifyNoOtherCalls();
    }
}
using HolyConnect.Application.Common;
using HolyConnect.Application.Interfaces;
using HolyConnect.Application.Services;
using HolyConnect.Domain.Entities;
using Moq;

namespace HolyConnect.Application.Tests.Common;

public class RequestExecutorFactoryTests
{
    [Fact]
    public void GetExecutor_WithValidRequest_ShouldReturnCorrectExecutor()
    {
        // Arrange
        var mockExecutor1 = new Mock<IRequestExecutor>();
        mockExecutor1.Setup(e => e.CanExecute(It.IsAny<Request>())).Returns(false);
        
        var mockExecutor2 = new Mock<IRequestExecutor>();
        mockExecutor2.Setup(e => e.CanExecute(It.IsAny<Request>())).Returns(true);
        
        var executors = new List<IRequestExecutor> { mockExecutor1.Object, mockExecutor2.Object };
        var factory = new RequestExecutorFactory(executors);
        
        var request = new RestRequest { Name = "Test" };

        // Act
        var result = factory.GetExecutor(request);

        // Assert
        Assert.Same(mockExecutor2.Object, result);
    }

    [Fact]
    public void GetExecutor_CalledTwiceWithSameRequestType_ShouldUseCachedExecutor()
    {
        // Arrange
        var mockExecutor = new Mock<IRequestExecutor>();
        mockExecutor.Setup(e => e.CanExecute(It.IsAny<Request>())).Returns(true);
        
        var executors = new List<IRequestExecutor> { mockExecutor.Object };
        var factory = new RequestExecutorFactory(executors);
        
        var request1 = new RestRequest { Name = "Test1" };
        var request2 = new RestRequest { Name = "Test2" };

        // Act
        var result1 = factory.GetExecutor(request1);
        var result2 = factory.GetExecutor(request2);

        // Assert
        Assert.Same(result1, result2);
        // CanExecute should only be called once (for the first request), subsequent calls use cache
        mockExecutor.Verify(e => e.CanExecute(It.IsAny<Request>()), Times.Once);
    }

    [Fact]
    public void GetExecutor_WithNoMatchingExecutor_ShouldThrowNotSupportedException()
    {
        // Arrange
        var mockExecutor = new Mock<IRequestExecutor>();
        mockExecutor.Setup(e => e.CanExecute(It.IsAny<Request>())).Returns(false);
        
        var executors = new List<IRequestExecutor> { mockExecutor.Object };
        var factory = new RequestExecutorFactory(executors);
        
        var request = new RestRequest { Name = "Test" };

        // Act & Assert
        var exception = Assert.Throws<NotSupportedException>(() => factory.GetExecutor(request));
        Assert.Contains("No executor found", exception.Message);
    }

    [Fact]
    public void GetExecutor_WithNullRequest_ShouldThrowArgumentNullException()
    {
        // Arrange
        var mockExecutor = new Mock<IRequestExecutor>();
        var executors = new List<IRequestExecutor> { mockExecutor.Object };
        var factory = new RequestExecutorFactory(executors);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => factory.GetExecutor(null!));
    }

    [Fact]
    public void Constructor_WithNullExecutors_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new RequestExecutorFactory(null!));
    }
}

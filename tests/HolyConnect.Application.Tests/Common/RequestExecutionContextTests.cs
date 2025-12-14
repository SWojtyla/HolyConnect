using HolyConnect.Application.Common;
using HolyConnect.Application.Interfaces;
using Moq;

namespace HolyConnect.Application.Tests.Common;

public class RequestExecutionContextTests
{
    [Fact]
    public void Constructor_ShouldSetAllProperties()
    {
        // Arrange
        var mockActiveEnvironment = new Mock<IActiveEnvironmentService>();
        var mockVariableResolver = new Mock<IVariableResolver>();
        var mockExecutorFactory = new Mock<IRequestExecutorFactory>();
        var mockResponseExtractor = new Mock<IResponseValueExtractor>();

        // Act
        var context = new RequestExecutionContext(
            mockActiveEnvironment.Object,
            mockVariableResolver.Object,
            mockExecutorFactory.Object,
            mockResponseExtractor.Object);

        // Assert
        Assert.Same(mockActiveEnvironment.Object, context.ActiveEnvironment);
        Assert.Same(mockVariableResolver.Object, context.VariableResolver);
        Assert.Same(mockExecutorFactory.Object, context.ExecutorFactory);
        Assert.Same(mockResponseExtractor.Object, context.ResponseExtractor);
    }

    [Fact]
    public void Constructor_ShouldAllowNullResponseExtractor()
    {
        // Arrange
        var mockActiveEnvironment = new Mock<IActiveEnvironmentService>();
        var mockVariableResolver = new Mock<IVariableResolver>();
        var mockExecutorFactory = new Mock<IRequestExecutorFactory>();

        // Act
        var context = new RequestExecutionContext(
            mockActiveEnvironment.Object,
            mockVariableResolver.Object,
            mockExecutorFactory.Object);

        // Assert
        Assert.Same(mockActiveEnvironment.Object, context.ActiveEnvironment);
        Assert.Same(mockVariableResolver.Object, context.VariableResolver);
        Assert.Same(mockExecutorFactory.Object, context.ExecutorFactory);
        Assert.Null(context.ResponseExtractor);
    }

    [Fact]
    public void Properties_ShouldBeReadOnly()
    {
        // Arrange
        var mockActiveEnvironment = new Mock<IActiveEnvironmentService>();
        var mockVariableResolver = new Mock<IVariableResolver>();
        var mockExecutorFactory = new Mock<IRequestExecutorFactory>();

        var context = new RequestExecutionContext(
            mockActiveEnvironment.Object,
            mockVariableResolver.Object,
            mockExecutorFactory.Object);

        // Assert - verify properties don't have setters
        var activeEnvProperty = typeof(RequestExecutionContext).GetProperty(nameof(RequestExecutionContext.ActiveEnvironment));
        Assert.NotNull(activeEnvProperty);
        Assert.True(activeEnvProperty!.CanRead);
        Assert.False(activeEnvProperty.CanWrite || activeEnvProperty.SetMethod?.IsPublic == true);
    }
}

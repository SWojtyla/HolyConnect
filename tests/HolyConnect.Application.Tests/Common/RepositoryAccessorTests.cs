using HolyConnect.Application.Common;
using HolyConnect.Application.Interfaces;
using HolyConnect.Domain.Entities;
using Moq;

namespace HolyConnect.Application.Tests.Common;

public class RepositoryAccessorTests
{
    [Fact]
    public void Constructor_ShouldSetAllProperties()
    {
        // Arrange
        var mockRequests = new Mock<IRepository<Request>>();
        var mockCollections = new Mock<IRepository<Collection>>();
        var mockEnvironments = new Mock<IRepository<Domain.Entities.Environment>>();
        var mockFlows = new Mock<IRepository<Flow>>();
        var mockHistory = new Mock<IRepository<RequestHistoryEntry>>();

        // Act
        var accessor = new RepositoryAccessor(
            mockRequests.Object,
            mockCollections.Object,
            mockEnvironments.Object,
            mockFlows.Object,
            mockHistory.Object);

        // Assert
        Assert.Same(mockRequests.Object, accessor.Requests);
        Assert.Same(mockCollections.Object, accessor.Collections);
        Assert.Same(mockEnvironments.Object, accessor.Environments);
        Assert.Same(mockFlows.Object, accessor.Flows);
        Assert.Same(mockHistory.Object, accessor.History);
    }

    [Fact]
    public void Properties_ShouldBeReadOnly()
    {
        // Arrange
        var mockRequests = new Mock<IRepository<Request>>();
        var mockCollections = new Mock<IRepository<Collection>>();
        var mockEnvironments = new Mock<IRepository<Domain.Entities.Environment>>();
        var mockFlows = new Mock<IRepository<Flow>>();
        var mockHistory = new Mock<IRepository<RequestHistoryEntry>>();

        var accessor = new RepositoryAccessor(
            mockRequests.Object,
            mockCollections.Object,
            mockEnvironments.Object,
            mockFlows.Object,
            mockHistory.Object);

        // Assert - verify properties don't have setters
        var requestsProperty = typeof(RepositoryAccessor).GetProperty(nameof(RepositoryAccessor.Requests));
        Assert.NotNull(requestsProperty);
        Assert.True(requestsProperty!.CanRead);
        Assert.False(requestsProperty.CanWrite || requestsProperty.SetMethod?.IsPublic == true);
    }
}

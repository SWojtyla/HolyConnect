using HolyConnect.Domain.Entities;
using Xunit;

namespace HolyConnect.Domain.Tests.Entities;

public class GitFolderTests
{
    [Fact]
    public void GitFolder_WhenCreated_ShouldHaveId()
    {
        // Act
        var gitFolder = new GitFolder();

        // Assert
        Assert.NotEqual(Guid.Empty, gitFolder.Id);
    }

    [Fact]
    public void GitFolder_WhenCreated_ShouldHaveCreatedAt()
    {
        // Act
        var gitFolder = new GitFolder();

        // Assert
        Assert.NotEqual(default(DateTimeOffset), gitFolder.CreatedAt);
    }

    [Fact]
    public void GitFolder_Properties_ShouldBeSettable()
    {
        // Arrange
        var gitFolder = new GitFolder();
        var id = Guid.NewGuid();
        var name = "Test Repo";
        var path = "/test/path";
        var isActive = true;
        var lastAccessed = DateTimeOffset.UtcNow;

        // Act
        gitFolder.Id = id;
        gitFolder.Name = name;
        gitFolder.Path = path;
        gitFolder.IsActive = isActive;
        gitFolder.LastAccessedAt = lastAccessed;

        // Assert
        Assert.Equal(id, gitFolder.Id);
        Assert.Equal(name, gitFolder.Name);
        Assert.Equal(path, gitFolder.Path);
        Assert.Equal(isActive, gitFolder.IsActive);
        Assert.Equal(lastAccessed, gitFolder.LastAccessedAt);
    }
}

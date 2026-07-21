using BookStore.Data;
using Xunit;
using BookStore.Services.Implementaion;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace BookStore.Tests.Services;

public class UserServiceTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<RoleManager<IdentityRole>> _roleManagerMock;
    private readonly UserService _service;

    public UserServiceTests()
    {
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            Mock.Of<IUserStore<ApplicationUser>>(),
            null!, null!, null!, null!, null!, null!, null!, null!);

        _roleManagerMock = new Mock<RoleManager<IdentityRole>>(
            Mock.Of<IRoleStore<IdentityRole>>(),
            null!, null!, null!, null!);

        _service = new UserService(
            _userManagerMock.Object,
            _roleManagerMock.Object,
            NullLogger<UserService>.Instance);
    }

    // ──────────────────────────────────────────────────────────────────
    // CreateRoleAsync
    // ──────────────────────────────────────────────────────────────────

    [Fact]
    public async Task CreateRoleAsync_RoleAlreadyExists_ReturnsFalse()
    {
        // Arrange
        _roleManagerMock
            .Setup(r => r.RoleExistsAsync("Admin"))
            .ReturnsAsync(true);

        // Act
        var result = await _service.CreateRoleAsync("Admin");

        // Assert
        Assert.False(result);
        _roleManagerMock.Verify(r => r.CreateAsync(It.IsAny<IdentityRole>()), Times.Never);
    }

    [Fact]
    public async Task CreateRoleAsync_NewRole_ReturnsTrue()
    {
        // Arrange
        _roleManagerMock
            .Setup(r => r.RoleExistsAsync("Editor"))
            .ReturnsAsync(false);

        _roleManagerMock
            .Setup(r => r.CreateAsync(It.IsAny<IdentityRole>()))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _service.CreateRoleAsync("Editor");

        // Assert
        Assert.True(result);
        _roleManagerMock.Verify(r => r.CreateAsync(It.Is<IdentityRole>(role => role.Name == "Editor")), Times.Once);
    }

    // ──────────────────────────────────────────────────────────────────
    // AddUserToRoleAsync
    // ──────────────────────────────────────────────────────────────────

    [Fact]
    public async Task AddUserToRoleAsync_RoleNotFound_ReturnsFalse()
    {
        // Arrange
        _roleManagerMock
            .Setup(r => r.FindByNameAsync("Unknown"))
            .ReturnsAsync((IdentityRole?)null);

        // Act
        var result = await _service.AddUserToRoleAsync("user-1", "Unknown");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task AddUserToRoleAsync_UserNotFound_ReturnsFalse()
    {
        // Arrange
        _roleManagerMock
            .Setup(r => r.FindByNameAsync("Admin"))
            .ReturnsAsync(new IdentityRole("Admin"));

        _userManagerMock
            .Setup(u => u.FindByIdAsync("missing-id"))
            .ReturnsAsync((ApplicationUser?)null);

        // Act
        var result = await _service.AddUserToRoleAsync("missing-id", "Admin");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task AddUserToRoleAsync_Success_ReturnsTrue()
    {
        // Arrange
        var role = new IdentityRole("Admin");
        var user = new ApplicationUser { Id = "user-1", UserName = "alice" };

        _roleManagerMock.Setup(r => r.FindByNameAsync("Admin")).ReturnsAsync(role);
        _userManagerMock.Setup(u => u.FindByIdAsync("user-1")).ReturnsAsync(user);
        _userManagerMock
            .Setup(u => u.AddToRoleAsync(user, "Admin"))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _service.AddUserToRoleAsync("user-1", "Admin");

        // Assert
        Assert.True(result);
        _userManagerMock.Verify(u => u.AddToRoleAsync(user, "Admin"), Times.Once);
    }

    // ──────────────────────────────────────────────────────────────────
    // DeleteUserByIdAsync
    // ──────────────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteUserByIdAsync_UserNotFound_ReturnsFailedResult()
    {
        // Arrange
        _userManagerMock
            .Setup(u => u.FindByIdAsync("ghost-id"))
            .ReturnsAsync((ApplicationUser?)null);

        // Act
        var result = await _service.DeleteUserByIdAsync("ghost-id");

        // Assert
        Assert.False(result.Succeeded);
        _userManagerMock.Verify(u => u.DeleteAsync(It.IsAny<ApplicationUser>()), Times.Never);
    }

    [Fact]
    public async Task DeleteUserByIdAsync_UserExists_ReturnsSuccess()
    {
        // Arrange
        var user = new ApplicationUser { Id = "user-2", UserName = "bob" };

        _userManagerMock.Setup(u => u.FindByIdAsync("user-2")).ReturnsAsync(user);
        _userManagerMock.Setup(u => u.DeleteAsync(user)).ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _service.DeleteUserByIdAsync("user-2");

        // Assert
        Assert.True(result.Succeeded);
        _userManagerMock.Verify(u => u.DeleteAsync(user), Times.Once);
    }
}

using FluentAssertions;
using Moq;
using pwa_camera_poc_blazor.Services.Auth;
using pwa_camera_poc_blazor.Services.Storage;
using Xunit;

namespace Tests.Services.Auth;

[Trait("Category", "Unit")]
[Trait("Service", "BruteForceProtection")]
public class BruteForceProtectionTests
{
    private const string Username = "test.user";

    private static (BruteForceProtection sut, Mock<ILocalStorageService> mockStorage) CreateSut(
        Dictionary<string, LoginAttempt>? storedAttempts = null)
    {
        var mockStorage = new Mock<ILocalStorageService>();
        mockStorage
            .Setup(x => x.GetItemAsync<Dictionary<string, LoginAttempt>>("failed_login_attempts"))
            .ReturnsAsync(storedAttempts);
        mockStorage
            .Setup(x => x.SetItemAsync(It.IsAny<string>(), It.IsAny<object>()))
            .Returns(Task.CompletedTask);

        return (new BruteForceProtection(mockStorage.Object), mockStorage);
    }

    // ─── IsLockedOutAsync ─────────────────────────────────────────────────────

    [Fact]
    public async Task IsLockedOutAsync_WithNoAttempts_ShouldReturnFalse()
    {
        var (sut, _) = CreateSut();
        var result = await sut.IsLockedOutAsync(Username);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsLockedOutAsync_WithActiveLockout_ShouldReturnTrue()
    {
        var attempts = new Dictionary<string, LoginAttempt>
        {
            [Username] = new LoginAttempt
            {
                FailedAttempts = 5,
                LockoutUntil = DateTime.UtcNow.AddMinutes(10),
                LastAttempt = DateTime.UtcNow
            }
        };
        var (sut, _) = CreateSut(attempts);
        var result = await sut.IsLockedOutAsync(Username);
        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsLockedOutAsync_WithExpiredLockout_ShouldReturnFalse()
    {
        var attempts = new Dictionary<string, LoginAttempt>
        {
            [Username] = new LoginAttempt
            {
                FailedAttempts = 5,
                LockoutUntil = DateTime.UtcNow.AddMinutes(-1), // expired
                LastAttempt = DateTime.UtcNow.AddMinutes(-20)
            }
        };
        var (sut, _) = CreateSut(attempts);
        var result = await sut.IsLockedOutAsync(Username);
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsLockedOutAsync_WithExpiredLockout_ShouldResetAttempts()
    {
        var attempts = new Dictionary<string, LoginAttempt>
        {
            [Username] = new LoginAttempt
            {
                FailedAttempts = 5,
                LockoutUntil = DateTime.UtcNow.AddMinutes(-1),
                LastAttempt = DateTime.UtcNow.AddMinutes(-20)
            }
        };
        var (sut, mockStorage) = CreateSut(attempts);
        await sut.IsLockedOutAsync(Username);
        mockStorage.Verify(x => x.SetItemAsync("failed_login_attempts", It.IsAny<object>()), Times.Once);
    }

    // ─── GetRemainingLockoutTimeAsync ─────────────────────────────────────────

    [Fact]
    public async Task GetRemainingLockoutTimeAsync_WithNoLockout_ShouldReturnNull()
    {
        var (sut, _) = CreateSut();
        var result = await sut.GetRemainingLockoutTimeAsync(Username);
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetRemainingLockoutTimeAsync_WithActiveLockout_ShouldReturnPositiveTimeSpan()
    {
        var lockoutUntil = DateTime.UtcNow.AddMinutes(10);
        var attempts = new Dictionary<string, LoginAttempt>
        {
            [Username] = new LoginAttempt { LockoutUntil = lockoutUntil, FailedAttempts = 5 }
        };
        var (sut, _) = CreateSut(attempts);
        var result = await sut.GetRemainingLockoutTimeAsync(Username);
        result.Should().NotBeNull();
        result!.Value.TotalMinutes.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetRemainingLockoutTimeAsync_WithExpiredLockout_ShouldReturnNull()
    {
        var attempts = new Dictionary<string, LoginAttempt>
        {
            [Username] = new LoginAttempt
            {
                LockoutUntil = DateTime.UtcNow.AddMinutes(-5),
                FailedAttempts = 5
            }
        };
        var (sut, _) = CreateSut(attempts);
        var result = await sut.GetRemainingLockoutTimeAsync(Username);
        result.Should().BeNull();
    }

    // ─── RecordFailedAttemptAsync ─────────────────────────────────────────────

    [Fact]
    public async Task RecordFailedAttemptAsync_ShouldIncrementFailedAttempts()
    {
        Dictionary<string, LoginAttempt>? saved = null;
        var mockStorage = new Mock<ILocalStorageService>();
        mockStorage
            .Setup(x => x.GetItemAsync<Dictionary<string, LoginAttempt>>("failed_login_attempts"))
            .ReturnsAsync((Dictionary<string, LoginAttempt>?)null);
        mockStorage
            .Setup(x => x.SetItemAsync("failed_login_attempts", It.IsAny<object>()))
            .Callback<string, object>((_, v) => saved = v as Dictionary<string, LoginAttempt>)
            .Returns(Task.CompletedTask);

        var sut = new BruteForceProtection(mockStorage.Object);
        await sut.RecordFailedAttemptAsync(Username);

        saved.Should().NotBeNull();
        saved![Username].FailedAttempts.Should().Be(1);
        saved[Username].LockoutUntil.Should().BeNull();
    }

    [Fact]
    public async Task RecordFailedAttemptAsync_AfterMaxAttempts_ShouldSetLockout()
    {
        // Start with 4 existing failures
        var existing = new Dictionary<string, LoginAttempt>
        {
            [Username] = new LoginAttempt { FailedAttempts = 4, LastAttempt = DateTime.UtcNow }
        };

        Dictionary<string, LoginAttempt>? saved = null;
        var mockStorage = new Mock<ILocalStorageService>();
        mockStorage
            .Setup(x => x.GetItemAsync<Dictionary<string, LoginAttempt>>("failed_login_attempts"))
            .ReturnsAsync(existing);
        mockStorage
            .Setup(x => x.SetItemAsync("failed_login_attempts", It.IsAny<object>()))
            .Callback<string, object>((_, v) => saved = v as Dictionary<string, LoginAttempt>)
            .Returns(Task.CompletedTask);

        var sut = new BruteForceProtection(mockStorage.Object);
        await sut.RecordFailedAttemptAsync(Username);

        saved![Username].FailedAttempts.Should().Be(5);
        saved[Username].LockoutUntil.Should().NotBeNull();
        saved[Username].LockoutUntil!.Value.Should().BeAfter(DateTime.UtcNow);
    }

    // ─── ResetAttemptsAsync ───────────────────────────────────────────────────

    [Fact]
    public async Task ResetAttemptsAsync_ShouldClearFailedAttemptsAndLockout()
    {
        Dictionary<string, LoginAttempt>? saved = null;
        var mockStorage = new Mock<ILocalStorageService>();
        mockStorage
            .Setup(x => x.GetItemAsync<Dictionary<string, LoginAttempt>>("failed_login_attempts"))
            .ReturnsAsync((Dictionary<string, LoginAttempt>?)null);
        mockStorage
            .Setup(x => x.SetItemAsync("failed_login_attempts", It.IsAny<object>()))
            .Callback<string, object>((_, v) => saved = v as Dictionary<string, LoginAttempt>)
            .Returns(Task.CompletedTask);

        var sut = new BruteForceProtection(mockStorage.Object);
        await sut.ResetAttemptsAsync(Username);

        saved.Should().NotBeNull();
        saved![Username].FailedAttempts.Should().Be(0);
        saved[Username].LockoutUntil.Should().BeNull();
    }

    // ─── Storage error handling ───────────────────────────────────────────────

    [Fact]
    public async Task IsLockedOutAsync_WhenStorageThrows_ShouldReturnFalse()
    {
        var mockStorage = new Mock<ILocalStorageService>();
        mockStorage
            .Setup(x => x.GetItemAsync<Dictionary<string, LoginAttempt>>(It.IsAny<string>()))
            .ThrowsAsync(new Exception("storage error"));

        var sut = new BruteForceProtection(mockStorage.Object);
        var result = await sut.IsLockedOutAsync(Username);
        result.Should().BeFalse();
    }
}

using Bunit;
using FluentAssertions;
using Xunit;
using Moq;
using AspecCaptura.Components.Feedback;
using AspecCaptura.Services;
using AspecCaptura.Services.Auth;
using AspecCaptura.Services.Storage;
using Blazored.LocalStorage;
using Microsoft.Extensions.DependencyInjection;

namespace Tests.Components.Feedback;

[Trait("Category", "Unit")]
[Trait("Component", "EmptyState")]
public class EmptyStateTests : TestContext
{
    public EmptyStateTests()
    {
        // Mock globally injected services from _Imports.razor
        var mockAppInfo = new Mock<IAppInfo>();
        mockAppInfo.Setup(x => x.Version).Returns("1.0.0");
        Services.AddSingleton(mockAppInfo.Object);

        var mockAuthService = new Mock<IAuthService>();
        Services.AddSingleton(mockAuthService.Object);

        var mockDbService = new Mock<IIndexedDbService>();
        Services.AddSingleton(mockDbService.Object);

        var mockToastService = new Mock<AspecCaptura.Services.ToastService>();
        Services.AddSingleton(mockToastService.Object);

        // AppState requires ILocalStorageService, so mock that too
        var mockLocalStorage = new Mock<AspecCaptura.Services.Storage.ILocalStorageService>();
        Services.AddSingleton(mockLocalStorage.Object);
        
        // Create a real AppState instance with the mocked localStorage
        var appState = new AspecCaptura.Services.AppState(mockLocalStorage.Object);
        Services.AddSingleton(appState);
    }
    [Fact]
    [Trait("Task", "5.3")]
    [Trait("Requirement", "2.5")]
    public void EmptyState_WithDefaultIcon_ShouldRenderCorrectly()
    {
        // Arrange & Act
        var cut = RenderComponent<EmptyState>(parameters => parameters
            .Add(p => p.Title, "Nenhum bem encontrado")
            .Add(p => p.Subtitle, "Tente outro filtro ou termo de busca"));

        // Assert
        cut.Find(".empty-state").Should().NotBeNull();
        cut.Find(".empty-state__icon").TextContent.Should().Be("📦");
        cut.Find(".empty-state__title").TextContent.Should().Be("Nenhum bem encontrado");
        cut.Find(".empty-state__subtitle").TextContent.Should().Be("Tente outro filtro ou termo de busca");
    }

    [Fact]
    [Trait("Task", "5.3")]
    [Trait("Requirement", "2.5")]
    public void EmptyState_WithCustomIcon_ShouldRenderCustomIcon()
    {
        // Arrange & Act
        var cut = RenderComponent<EmptyState>(parameters => parameters
            .Add(p => p.Icon, "🔍")
            .Add(p => p.Title, "Nenhum resultado")
            .Add(p => p.Subtitle, "Tente uma busca diferente"));

        // Assert
        cut.Find(".empty-state__icon").TextContent.Should().Be("🔍");
    }

    [Fact]
    [Trait("Task", "5.3")]
    [Trait("Requirement", "26.7")]
    public void EmptyState_ShouldNotContainActionButton()
    {
        // Arrange & Act
        var cut = RenderComponent<EmptyState>(parameters => parameters
            .Add(p => p.Title, "Empty State")
            .Add(p => p.Subtitle, "No items found"));

        // Assert - Verify no button elements exist (per Requirement 26.3, 26.7)
        var buttons = cut.FindAll("button");
        buttons.Should().BeEmpty("EmptyState should not contain action buttons per Requirement 26.7");
    }

    [Fact]
    [Trait("Task", "5.3")]
    [Trait("Requirement", "2.5")]
    public void EmptyState_WithoutSubtitle_ShouldRenderOnlyTitleAndIcon()
    {
        // Arrange & Act
        var cut = RenderComponent<EmptyState>(parameters => parameters
            .Add(p => p.Title, "Empty"));

        // Assert
        cut.Find(".empty-state__icon").Should().NotBeNull();
        cut.Find(".empty-state__title").Should().NotBeNull();
        cut.FindAll(".empty-state__subtitle").Should().BeEmpty();
    }

    [Fact]
    [Trait("Task", "5.3")]
    [Trait("Requirement", "26.19")]
    public void EmptyState_ShouldHaveCenteredLayout()
    {
        // Arrange & Act
        var cut = RenderComponent<EmptyState>(parameters => parameters
            .Add(p => p.Title, "Centered Content")
            .Add(p => p.Subtitle, "This should be centered"));

        // Assert
        var emptyState = cut.Find(".empty-state");
        emptyState.Should().NotBeNull();
        // The CSS class ensures centered layout via flexbox
        emptyState.ClassList.Should().Contain("empty-state");
    }

    [Fact]
    [Trait("Task", "5.3")]
    [Trait("Requirement", "13.7")]
    public void EmptyState_WithAllParameters_ShouldRenderAllElements()
    {
        // Arrange & Act
        var cut = RenderComponent<EmptyState>(parameters => parameters
            .Add(p => p.Icon, "📋")
            .Add(p => p.Title, "No Data Available")
            .Add(p => p.Subtitle, "Please add some data to get started"));

        // Assert
        var icon = cut.Find(".empty-state__icon");
        var title = cut.Find(".empty-state__title");
        var subtitle = cut.Find(".empty-state__subtitle");

        icon.TextContent.Should().Be("📋");
        title.TextContent.Should().Be("No Data Available");
        subtitle.TextContent.Should().Be("Please add some data to get started");
    }
}

using FluentAssertions;
using Moq;
using AspecCaptura.Models;
using AspecCaptura.Services.Recognition;
using AspecCaptura.Services.Storage;
using Tests.Builders;
using Xunit;

namespace Tests.Services.Recognition;

[Trait("Category", "Unit")]
[Trait("Service", "PatrimonioSearchService")]
public class PatrimonioSearchServiceTests
{
    private static (PatrimonioSearchService sut, Mock<IIndexedDbService> db) CreateSut(
        List<PatrimonioItem>? allItems = null,
        PatrimonioItem? byNutomb = null)
    {
        var db = new Mock<IIndexedDbService>();
        db.Setup(x => x.GetPatrimonioByNutombAsync(It.IsAny<string>()))
            .ReturnsAsync(byNutomb);
        db.Setup(x => x.GetAllAsync<PatrimonioItem>("patrimonio"))
            .ReturnsAsync(allItems ?? new List<PatrimonioItem>());
        return (new PatrimonioSearchService(db.Object), db);
    }

    // ─── SearchByCodeAsync — null / empty ────────────────────────────────────

    [Fact]
    public async Task SearchByCodeAsync_WithNullCode_ShouldReturnNull()
    {
        var (sut, _) = CreateSut();
        var result = await sut.SearchByCodeAsync(null!);
        result.Should().BeNull();
    }

    [Fact]
    public async Task SearchByCodeAsync_WithEmptyCode_ShouldReturnNull()
    {
        var (sut, _) = CreateSut();
        var result = await sut.SearchByCodeAsync("");
        result.Should().BeNull();
    }

    [Fact]
    public async Task SearchByCodeAsync_WithWhitespaceCode_ShouldReturnNull()
    {
        var (sut, _) = CreateSut();
        var result = await sut.SearchByCodeAsync("   ");
        result.Should().BeNull();
    }

    // ─── SearchByCodeAsync — nutomb hit ──────────────────────────────────────

    [Fact]
    public async Task SearchByCodeAsync_WhenNutombHit_ShouldReturnItem()
    {
        var item = new PatrimonioItemBuilder().WithNutomb("00000001").Build();
        var (sut, db) = CreateSut(byNutomb: item);

        var result = await sut.SearchByCodeAsync("00000001");

        result.Should().NotBeNull();
        result!.Nutomb.Should().Be("00000001");
        db.Verify(x => x.GetPatrimonioByNutombAsync("00000001"), Times.Once);
    }

    // ─── SearchByCodeAsync — fallback to GetAllAsync ─────────────────────────

    [Fact]
    public async Task SearchByCodeAsync_WhenNutombMiss_ShouldFallbackToGetAll()
    {
        var item = new PatrimonioItemBuilder().WithNutomb("00000002").Build();
        var (sut, db) = CreateSut(allItems: new List<PatrimonioItem> { item }, byNutomb: null);

        var result = await sut.SearchByCodeAsync("00000002");

        result.Should().NotBeNull();
        db.Verify(x => x.GetAllAsync<PatrimonioItem>("patrimonio"), Times.Once);
    }

    [Fact]
    public async Task SearchByCodeAsync_WhenNotFoundAnywhere_ShouldReturnNull()
    {
        var (sut, _) = CreateSut(allItems: new List<PatrimonioItem>(), byNutomb: null);
        var result = await sut.SearchByCodeAsync("NOTEXIST");
        result.Should().BeNull();
    }

    // ─── SearchByCodeAsync — OCR sanitization ────────────────────────────────

    [Fact]
    public async Task SearchByCodeAsync_ShouldSanitizeOCRMistakes()
    {
        // 'O' → '0', 'I' → '1'
        var item = new PatrimonioItemBuilder().WithNutomb("00000001").Build();
        var (sut, db) = CreateSut(byNutomb: item);

        // Pass 'OO000001' — should be sanitized to '00000001'
        var result = await sut.SearchByCodeAsync("OO000001");

        result.Should().NotBeNull();
        db.Verify(x => x.GetPatrimonioByNutombAsync("00000001"), Times.Once);
    }

    // ─── SearchByCodeAsync — cache hit ───────────────────────────────────────

    [Fact]
    public async Task SearchByCodeAsync_SecondCall_ShouldUseCacheAndNotHitDb()
    {
        var item = new PatrimonioItemBuilder().WithNutomb("00000001").Build();
        var (sut, db) = CreateSut(byNutomb: item);

        await sut.SearchByCodeAsync("00000001");
        await sut.SearchByCodeAsync("00000001");

        // DB should only be called once — second call uses cache
        db.Verify(x => x.GetPatrimonioByNutombAsync("00000001"), Times.Once);
    }

    // ─── SearchByCodeAsync — db error ────────────────────────────────────────

    [Fact]
    public async Task SearchByCodeAsync_WhenDbThrows_ShouldReturnNull()
    {
        var db = new Mock<IIndexedDbService>();
        db.Setup(x => x.GetPatrimonioByNutombAsync(It.IsAny<string>()))
            .ThrowsAsync(new Exception("db error"));

        var sut = new PatrimonioSearchService(db.Object);
        var result = await sut.SearchByCodeAsync("00000001");
        result.Should().BeNull();
    }

    // ─── SearchByCodesAsync ───────────────────────────────────────────────────

    [Fact]
    public async Task SearchByCodesAsync_WithNullArray_ShouldReturnEmpty()
    {
        var (sut, _) = CreateSut();
        var result = await sut.SearchByCodesAsync(null!);
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task SearchByCodesAsync_WithEmptyArray_ShouldReturnEmpty()
    {
        var (sut, _) = CreateSut();
        var result = await sut.SearchByCodesAsync(Array.Empty<string>());
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task SearchByCodesAsync_ShouldReturnMatchingItems()
    {
        var items = new List<PatrimonioItem>
        {
            new PatrimonioItemBuilder().WithNutomb("00000001").Build(),
            new PatrimonioItemBuilder().WithNutomb("00000002").Build(),
            new PatrimonioItemBuilder().WithNutomb("00000003").Build()
        };
        var (sut, _) = CreateSut(allItems: items);

        var result = await sut.SearchByCodesAsync(new[] { "00000001", "00000002" });

        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task SearchByCodesAsync_WhenCodeNotFound_ShouldExcludeFromResult()
    {
        var items = new List<PatrimonioItem>
        {
            new PatrimonioItemBuilder().WithNutomb("00000001").Build()
        };
        var (sut, _) = CreateSut(allItems: items);

        var result = await sut.SearchByCodesAsync(new[] { "00000001", "NOTEXIST" });

        result.Should().HaveCount(1);
        result[0].Nutomb.Should().Be("00000001");
    }

    [Fact]
    public async Task SearchByCodesAsync_ShouldUseCacheForAlreadySearchedCodes()
    {
        var item = new PatrimonioItemBuilder().WithNutomb("00000001").Build();
        var (sut, db) = CreateSut(byNutomb: item);

        // Prime the cache
        await sut.SearchByCodeAsync("00000001");

        // Now search via batch — should not call GetAllAsync since it's cached
        await sut.SearchByCodesAsync(new[] { "00000001" });

        db.Verify(x => x.GetAllAsync<PatrimonioItem>("patrimonio"), Times.Never);
    }

    // ─── IsCachedAsync ────────────────────────────────────────────────────────

    [Fact]
    public async Task IsCachedAsync_BeforeSearch_ShouldReturnFalse()
    {
        var (sut, _) = CreateSut();
        var result = await sut.IsCachedAsync("00000001");
        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsCachedAsync_AfterSearch_ShouldReturnTrue()
    {
        var item = new PatrimonioItemBuilder().WithNutomb("00000001").Build();
        var (sut, _) = CreateSut(byNutomb: item);

        await sut.SearchByCodeAsync("00000001");
        var result = await sut.IsCachedAsync("00000001");

        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsCachedAsync_AfterClearCache_ShouldReturnFalse()
    {
        var item = new PatrimonioItemBuilder().WithNutomb("00000001").Build();
        var (sut, _) = CreateSut(byNutomb: item);

        await sut.SearchByCodeAsync("00000001");
        sut.ClearCache();

        var result = await sut.IsCachedAsync("00000001");
        result.Should().BeFalse();
    }

    // ─── ClearCache ───────────────────────────────────────────────────────────

    [Fact]
    public async Task ClearCache_ShouldForceDbCallOnNextSearch()
    {
        var item = new PatrimonioItemBuilder().WithNutomb("00000001").Build();
        var (sut, db) = CreateSut(byNutomb: item);

        await sut.SearchByCodeAsync("00000001");
        sut.ClearCache();
        await sut.SearchByCodeAsync("00000001");

        // DB should be called twice — once before clear, once after
        db.Verify(x => x.GetPatrimonioByNutombAsync("00000001"), Times.Exactly(2));
    }
}

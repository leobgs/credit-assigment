using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using PengajuanKredit.Api.Data;
using PengajuanKredit.Api.Dtos;
using PengajuanKredit.Api.Exceptions;
using PengajuanKredit.Api.Services;

namespace PengajuanKredit.Tests.Services;

public class PengajuanKreditServiceTests
{
    private static AppDbContext CreateInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public async Task CreateAsync_DataValid_MenyimpanDanMengisiAngsuran()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var calculator = new AngsuranCalculator();
        var service = new PengajuanKreditService(context, calculator);

        var request = new CreatePengajuanKreditRequest(100_000_000m, 12m, 60);

        // Act
        var result = await service.CreateAsync(request);

        // Assert
        result.Id.Should().NotBeEmpty();
        result.Plafon.Should().Be(100_000_000m);
        result.Bunga.Should().Be(12m);
        result.Tenor.Should().Be(60);
        result.Angsuran.Should().BeApproximately(2224444.47m, 1m);
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        result.UpdatedAt.Should().Be(result.CreatedAt);
    }

    [Fact]
    public async Task GetByIdAsync_IdTidakAda_MelemparNotFoundException()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var calculator = new AngsuranCalculator();
        var service = new PengajuanKreditService(context, calculator);

        // Act & Assert
        var act = async () => await service.GetByIdAsync(Guid.NewGuid());
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task UpdateAsync_DataValid_MemperbaruiUpdatedAtDanAngsuran()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var calculator = new AngsuranCalculator();
        var service = new PengajuanKreditService(context, calculator);

        var created = await service.CreateAsync(new CreatePengajuanKreditRequest(50_000_000m, 10m, 24));
        var initialUpdatedAt = created.UpdatedAt;

        // Act
        var updateRequest = new UpdatePengajuanKreditRequest(60_000_000m, 12m, 36);
        var updated = await service.UpdateAsync(created.Id, updateRequest);

        // Assert
        updated.Plafon.Should().Be(60_000_000m);
        updated.Tenor.Should().Be(36);
        updated.Angsuran.Should().BeGreaterThan(0);
        updated.UpdatedAt.Should().BeOnOrAfter(initialUpdatedAt);
    }

    [Fact]
    public async Task DeleteAsync_DataDihapus_LaluGetByIdMelemparNotFoundException()
    {
        // Arrange
        using var context = CreateInMemoryDbContext();
        var calculator = new AngsuranCalculator();
        var service = new PengajuanKreditService(context, calculator);

        var created = await service.CreateAsync(new CreatePengajuanKreditRequest(20_000_000m, 10m, 12));

        // Act
        await service.DeleteAsync(created.Id);

        // Assert
        var act = async () => await service.GetByIdAsync(created.Id);
        await act.Should().ThrowAsync<NotFoundException>();
    }
}

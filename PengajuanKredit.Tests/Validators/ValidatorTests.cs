using FluentAssertions;
using PengajuanKredit.Api.Dtos;
using PengajuanKredit.Api.Validators;

namespace PengajuanKredit.Tests.Validators;

public class ValidatorTests
{
    private readonly CreatePengajuanKreditRequestValidator _createValidator = new();
    private readonly UpdatePengajuanKreditRequestValidator _updateValidator = new();
    private readonly HitungAngsuranRequestValidator _hitungValidator = new();

    [Theory]
    [InlineData(0)]
    [InlineData(-1000)]
    public void CreateValidator_PlafonNolAtauNegatif_HarusTidakValid(decimal invalidPlafon)
    {
        var request = new CreatePengajuanKreditRequest(invalidPlafon, 10m, 12);
        var result = _createValidator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(request.Plafon) && e.ErrorMessage.Contains("Plafon harus lebih besar dari 0"));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void CreateValidator_BungaNolAtauNegatif_HarusTidakValid(decimal invalidBunga)
    {
        var request = new CreatePengajuanKreditRequest(10_000_000m, invalidBunga, 12);
        var result = _createValidator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(request.Bunga) && e.ErrorMessage.Contains("Bunga harus lebih besar dari 0%"));
    }

    [Fact]
    public void CreateValidator_BungaLebihDari100_HarusTidakValid()
    {
        var request = new CreatePengajuanKreditRequest(10_000_000m, 101m, 12);
        var result = _createValidator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(request.Bunga) && e.ErrorMessage.Contains("Bunga maksimal 100%"));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void CreateValidator_TenorNolAtauNegatif_HarusTidakValid(int invalidTenor)
    {
        var request = new CreatePengajuanKreditRequest(10_000_000m, 12m, invalidTenor);
        var result = _createValidator.Validate(request);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(request.Tenor) && e.ErrorMessage.Contains("Tenor harus lebih besar dari 0"));
    }

    [Fact]
    public void HitungAngsuranValidator_DataValid_HarusValid()
    {
        var request = new HitungAngsuranRequest(100_000_000m, 12m, 60);
        var result = _hitungValidator.Validate(request);

        result.IsValid.Should().BeTrue();
    }
}

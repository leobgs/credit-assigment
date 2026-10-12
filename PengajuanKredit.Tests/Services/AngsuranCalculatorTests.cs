using FluentAssertions;
using PengajuanKredit.Api.Services;

namespace PengajuanKredit.Tests.Services;

public class AngsuranCalculatorTests
{
    private readonly AngsuranCalculator _calculator = new();

    [Fact]
    public void HitungAngsuranPerBulan_ContohKasusPRD_MenghasilkanNilaiSesuai()
    {
        // Arrange: Plafon 100jt, Bunga 12%, Tenor 60 bulan
        decimal plafon = 100_000_000m;
        decimal bunga = 12m;
        int tenor = 60;

        // Act
        decimal hasil = _calculator.HitungAngsuranPerBulan(plafon, bunga, tenor);

        // Assert: Sesuai PRD contoh 2224444.47 (sekitar 2.224.445) dengan toleransi 1 rupiah
        hasil.Should().BeApproximately(2224444.47m, 1m);
        Math.Round(hasil).Should().Be(2224445m);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1000)]
    public void HitungAngsuranPerBulan_PlafonNolAtauNegatif_MelemparArgumentOutOfRangeException(decimal invalidPlafon)
    {
        Action act = () => _calculator.HitungAngsuranPerBulan(invalidPlafon, 12m, 60);

        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("plafon");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-12)]
    public void HitungAngsuranPerBulan_TenorNolAtauNegatif_MelemparArgumentOutOfRangeException(int invalidTenor)
    {
        Action act = () => _calculator.HitungAngsuranPerBulan(10_000_000m, 12m, invalidTenor);

        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("tenorBulan");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    public void HitungAngsuranPerBulan_BungaDiLuarRentang0Sampai100_MelemparArgumentOutOfRangeException(decimal invalidBunga)
    {
        Action act = () => _calculator.HitungAngsuranPerBulan(10_000_000m, invalidBunga, 12);

        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("bungaPersenTahunan");
    }

    [Fact]
    public void HitungAngsuranPerBulan_BungaNolPersen_MenghitungAngsuranPokokBagiTenor()
    {
        // Plafon 12jt, bunga 0%, tenor 12 bulan -> 1.000.000 / bulan
        decimal hasil = _calculator.HitungAngsuranPerBulan(12_000_000m, 0m, 12);

        hasil.Should().Be(1_000_000m);
    }
}

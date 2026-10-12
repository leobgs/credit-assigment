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

        // Assert: Sesuai PRD contoh sekitar 2.224.445 per bulan (toleransi 1 rupiah sesuai T9)
        hasil.Should().BeApproximately(2224444.47m, 1m);
        Math.Round(hasil).Should().Be(2224445m);
    }
}

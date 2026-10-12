namespace PengajuanKredit.Api.Services;

public class AngsuranCalculator : IAngsuranCalculator
{
    public decimal HitungAngsuranPerBulan(decimal plafon, decimal bungaPersenTahunan, int tenorBulan)
    {
        if (plafon <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(plafon), "Plafon harus lebih besar dari 0.");
        }

        if (tenorBulan <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(tenorBulan), "Tenor harus lebih besar dari 0.");
        }

        if (bungaPersenTahunan < 0 || bungaPersenTahunan > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(bungaPersenTahunan), "Bunga harus bernilai antara 0 dan 100.");
        }

        // Penanganan kasus khusus bunga 0%
        if (bungaPersenTahunan == 0)
        {
            return Math.Round(plafon / tenorBulan, 2, MidpointRounding.AwayFromZero);
        }

        // r = bunga tahunan / 12 / 100
        decimal r = bungaPersenTahunan / 12m / 100m;

        // Menghitung (1 + r)^n menggunakan perkalian decimal berulang untuk menjaga presisi finansial
        // (menghindari double floating-point error dari Math.Pow).
        decimal satuPlusRPangkatN = PangkatDecimal(1m + r, tenorBulan);

        // Rumus anuitas: A = P * r / (1 - (1 + r)^-n)
        // Karena (1 + r)^-n = 1 / (1 + r)^n, maka:
        // A = (P * r) / (1 - (1 / satuPlusRPangkatN))
        decimal penyebut = 1m - (1m / satuPlusRPangkatN);
        decimal pembilang = plafon * r;

        decimal angsuran = pembilang / penyebut;

        return Math.Round(angsuran, 2, MidpointRounding.AwayFromZero);
    }

    /// <summary>
    /// Menghitung basis^pangkat secara eksak menggunakan tipe decimal.
    /// Menggunakan algoritma binary exponentiation untuk efisiensi O(log n).
    /// </summary>
    private static decimal PangkatDecimal(decimal basis, int pangkat)
    {
        if (pangkat == 0) return 1m;
        if (pangkat < 0) return 1m / PangkatDecimal(basis, -pangkat);

        decimal hasil = 1m;
        decimal faktor = basis;

        while (pangkat > 0)
        {
            if ((pangkat & 1) == 1)
            {
                hasil *= faktor;
            }
            pangkat >>= 1;
            if (pangkat > 0)
            {
                faktor *= faktor;
            }
        }

        return hasil;
    }
}

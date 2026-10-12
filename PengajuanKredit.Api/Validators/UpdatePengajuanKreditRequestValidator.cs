using FluentValidation;
using PengajuanKredit.Api.Dtos;

namespace PengajuanKredit.Api.Validators;

public class UpdatePengajuanKreditRequestValidator : AbstractValidator<UpdatePengajuanKreditRequest>
{
    public UpdatePengajuanKreditRequestValidator()
    {
        RuleFor(x => x.Plafon)
            .GreaterThan(0)
            .WithMessage("Plafon harus lebih besar dari 0.");

        RuleFor(x => x.Bunga)
            .GreaterThan(0)
            .WithMessage("Bunga harus lebih besar dari 0%.")
            .LessThanOrEqualTo(100)
            .WithMessage("Bunga maksimal 100%.");

        RuleFor(x => x.Tenor)
            .GreaterThan(0)
            .WithMessage("Tenor harus lebih besar dari 0 bulan.");
    }
}

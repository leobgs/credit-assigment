using Microsoft.AspNetCore.Mvc;
using PengajuanKredit.Api.Dtos;
using PengajuanKredit.Api.Services;

namespace PengajuanKredit.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AngsuranController : ControllerBase
{
    private readonly IAngsuranCalculator _calculator;

    public AngsuranController(IAngsuranCalculator calculator)
    {
        _calculator = calculator;
    }

    [HttpGet("hitung")]
    public ActionResult<HitungAngsuranResponse> Hitung(
        [FromQuery] decimal plafon,
        [FromQuery] decimal bunga,
        [FromQuery] int tenor)
    {
        decimal angsuranPerBulan = _calculator.HitungAngsuranPerBulan(plafon, bunga, tenor);

        return Ok(new HitungAngsuranResponse(
            plafon,
            bunga,
            tenor,
            angsuranPerBulan
        ));
    }
}

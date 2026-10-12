using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PengajuanKredit.Api.Dtos;
using PengajuanKredit.Api.Services;

namespace PengajuanKredit.Api.Controllers;

[Authorize]
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
    public ActionResult<HitungAngsuranResponse> Hitung([FromQuery] HitungAngsuranRequest request)
    {
        decimal angsuranPerBulan = _calculator.HitungAngsuranPerBulan(request.Plafon, request.Bunga, request.Tenor);

        return Ok(new HitungAngsuranResponse(
            request.Plafon,
            request.Bunga,
            request.Tenor,
            angsuranPerBulan
        ));
    }
}

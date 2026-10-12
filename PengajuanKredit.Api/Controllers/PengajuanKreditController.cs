using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PengajuanKredit.Api.Dtos;
using PengajuanKredit.Api.Exceptions;
using PengajuanKredit.Api.Services;

namespace PengajuanKredit.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/pengajuan-kredit")]
public class PengajuanKreditController : ControllerBase
{
    private readonly IPengajuanKreditService _service;

    public PengajuanKreditController(IPengajuanKreditService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<ActionResult<PengajuanKreditResponse>> Create(
        [FromBody] CreatePengajuanKreditRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _service.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<PengajuanKreditResponse>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await _service.GetAllAsync(page, pageSize, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PengajuanKreditResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.GetByIdAsync(id, cancellationToken);
            return Ok(result);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Data Tidak Ditemukan",
                Detail = ex.Message
            });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<PengajuanKreditResponse>> Update(
        Guid id,
        [FromBody] UpdatePengajuanKreditRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _service.UpdateAsync(id, request, cancellationToken);
            return Ok(result);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Data Tidak Ditemukan",
                Detail = ex.Message
            });
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            await _service.DeleteAsync(id, cancellationToken);
            return NoContent();
        }
        catch (NotFoundException ex)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Data Tidak Ditemukan",
                Detail = ex.Message
            });
        }
    }
}

using DataKendaraan.Api.Dtos;
using DataKendaraan.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace DataKendaraan.Api.Controllers;

[ApiController]
[Route("api/kendaraan")]
public class KendaraanController(KendaraanService service) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<KendaraanDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<KendaraanDto>>> GetAll(
        [FromQuery] string? noRegistrasi,
        [FromQuery] string? namaPemilik,
        CancellationToken cancellationToken)
    {
        var items = await service.CariAsync(noRegistrasi, namaPemilik, cancellationToken);
        return Ok(items);
    }

    [HttpGet("{noRegistrasi}")]
    [ProducesResponseType(typeof(KendaraanDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<KendaraanDto>> GetById(
        string noRegistrasi,
        CancellationToken cancellationToken)
    {
        var data = await service.AmbilAsync(noRegistrasi, cancellationToken);

        return data is null
            ? NotFound(new ApiError("Data kendaraan tidak ditemukan."))
            : Ok(data);
    }

    [HttpPost]
    [ProducesResponseType(typeof(KendaraanDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<KendaraanDto>> Create(
        [FromBody] KendaraanRequest request,
        CancellationToken cancellationToken)
    {
        var (hasil, data) = await service.TambahAsync(request, cancellationToken);

        if (hasil == HasilSimpan.Bentrok)
        {
            return Conflict(new ApiError("Nomor registrasi sudah terdaftar."));
        }

        return CreatedAtAction(
            nameof(GetById),
            new { noRegistrasi = data!.NoRegistrasi },
            data);
    }

    [HttpPut("{noRegistrasi}")]
    [ProducesResponseType(typeof(KendaraanDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<KendaraanDto>> Update(
        string noRegistrasi,
        [FromBody] KendaraanRequest request,
        CancellationToken cancellationToken)
    {
        var (hasil, data) = await service.UbahAsync(noRegistrasi, request, cancellationToken);

        return hasil switch
        {
            HasilSimpan.TidakDitemukan => NotFound(new ApiError("Data kendaraan tidak ditemukan.")),
            HasilSimpan.Bentrok => Conflict(new ApiError("Nomor registrasi sudah terdaftar.")),
            _ => Ok(data)
        };
    }

    [HttpDelete("{noRegistrasi}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        string noRegistrasi,
        CancellationToken cancellationToken)
    {
        var terhapus = await service.HapusAsync(noRegistrasi, cancellationToken);

        return terhapus
            ? NoContent()
            : NotFound(new ApiError("Data kendaraan tidak ditemukan."));
    }
}

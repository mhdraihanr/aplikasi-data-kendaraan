using System.Data.Common;
using DataKendaraan.Api.Data;
using DataKendaraan.Api.Dtos;
using DataKendaraan.Api.Models;
using DataKendaraan.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace DataKendaraan.Api.Controllers;

[ApiController]
[Route("api/kendaraan")]
public class KendaraanController : ControllerBase
{
    private readonly AppDbContext _db;

    public KendaraanController(AppDbContext db) => _db = db;

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<KendaraanDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<KendaraanDto>>> GetAll(
        [FromQuery] string? noRegistrasi,
        [FromQuery] string? namaPemilik,
        CancellationToken cancellationToken)
    {
        var query = _db.Kendaraan.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(noRegistrasi))
        {
            var term = noRegistrasi.Trim().ToLowerInvariant();
            query = query.Where(k => k.NoRegistrasi.ToLower().Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(namaPemilik))
        {
            var term = namaPemilik.Trim().ToLowerInvariant();
            query = query.Where(k => k.NamaPemilik.ToLower().Contains(term));
        }

        var items = await query
            .OrderBy(k => k.NoRegistrasi)
            .ToListAsync(cancellationToken);

        return Ok(items.Select(ToDto).ToList());
    }

    [HttpGet("{noRegistrasi}")]
    [ProducesResponseType(typeof(KendaraanDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<KendaraanDto>> GetById(
        string noRegistrasi,
        CancellationToken cancellationToken)
    {
        var key = noRegistrasi.Trim();

        var entity = await _db.Kendaraan
            .AsNoTracking()
            .FirstOrDefaultAsync(k => k.NoRegistrasi == key, cancellationToken);

        if (entity is null)
        {
            return NotFound(new ApiError("Data kendaraan tidak ditemukan."));
        }

        return Ok(ToDto(entity));
    }

    [HttpPost]
    [ProducesResponseType(typeof(KendaraanDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<KendaraanDto>> Create(
        [FromBody] KendaraanRequest request,
        CancellationToken cancellationToken)
    {
        var noRegistrasi = KendaraanNormalizer.NullIfEmpty(request.NoRegistrasi)!;
        var exists = await _db.Kendaraan
            .AnyAsync(k => k.NoRegistrasi == noRegistrasi, cancellationToken);

        if (exists)
        {
            return Conflict(new ApiError("Nomor registrasi sudah terdaftar."));
        }

        var entity = new Kendaraan
        {
            NoRegistrasi = noRegistrasi,
            NamaPemilik = KendaraanNormalizer.CollapseSpaces(request.NamaPemilik)!,
            Alamat = KendaraanNormalizer.NullIfEmpty(request.Alamat),
            MerkKendaraan = KendaraanNormalizer.NullIfEmpty(request.MerkKendaraan),
            TahunPembuatan = request.TahunPembuatan,
            KapasitasSilinder = request.KapasitasSilinder,
            Warna = KendaraanNormalizer.NormalizeWarna(request.Warna),
            BahanBakar = KendaraanNormalizer.NullIfEmpty(request.BahanBakar)
        };

        _db.Kendaraan.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetById), new { noRegistrasi = entity.NoRegistrasi }, ToDto(entity));
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
        var keyLama = noRegistrasi.Trim();
        var keyBaru = KendaraanNormalizer.NullIfEmpty(request.NoRegistrasi)!;

        var adaLama = await _db.Kendaraan
            .AnyAsync(k => k.NoRegistrasi == keyLama, cancellationToken);

        if (!adaLama)
        {
            return NotFound(new ApiError("Data kendaraan tidak ditemukan."));
        }

        if (keyBaru != keyLama)
        {
            var bentrok = await _db.Kendaraan
                .AnyAsync(k => k.NoRegistrasi == keyBaru, cancellationToken);

            if (bentrok)
            {
                return Conflict(new ApiError("Nomor registrasi sudah terdaftar."));
            }
        }

        var namaPemilik = KendaraanNormalizer.CollapseSpaces(request.NamaPemilik)!;
        var alamat = KendaraanNormalizer.NullIfEmpty(request.Alamat);
        var merk = KendaraanNormalizer.NullIfEmpty(request.MerkKendaraan);
        var warna = KendaraanNormalizer.NormalizeWarna(request.Warna);
        var bahanBakar = KendaraanNormalizer.NullIfEmpty(request.BahanBakar);

        try
        {
            var terpengaruh = await _db.Kendaraan
                .Where(k => k.NoRegistrasi == keyLama)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(k => k.NoRegistrasi, keyBaru)
                    .SetProperty(k => k.NamaPemilik, namaPemilik)
                    .SetProperty(k => k.Alamat, alamat)
                    .SetProperty(k => k.MerkKendaraan, merk)
                    .SetProperty(k => k.TahunPembuatan, request.TahunPembuatan)
                    .SetProperty(k => k.KapasitasSilinder, request.KapasitasSilinder)
                    .SetProperty(k => k.Warna, warna)
                    .SetProperty(k => k.BahanBakar, bahanBakar),
                    cancellationToken);

            if (terpengaruh == 0)
            {
                return NotFound(new ApiError("Data kendaraan tidak ditemukan."));
            }
        }
        catch (DbException ex) when (ex is SqliteException { SqliteErrorCode: 19 })
        {
            return Conflict(new ApiError("Nomor registrasi sudah terdaftar."));
        }

        var entity = new Kendaraan
        {
            NoRegistrasi = keyBaru,
            NamaPemilik = namaPemilik,
            Alamat = alamat,
            MerkKendaraan = merk,
            TahunPembuatan = request.TahunPembuatan,
            KapasitasSilinder = request.KapasitasSilinder,
            Warna = warna,
            BahanBakar = bahanBakar
        };

        return Ok(ToDto(entity));
    }

    [HttpDelete("{noRegistrasi}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiError), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        string noRegistrasi,
        CancellationToken cancellationToken)
    {
        var key = noRegistrasi.Trim();
        var terhapus = await _db.Kendaraan
            .Where(k => k.NoRegistrasi == key)
            .ExecuteDeleteAsync(cancellationToken);

        if (terhapus == 0)
        {
            return NotFound(new ApiError("Data kendaraan tidak ditemukan."));
        }

        return NoContent();
    }

    private static KendaraanDto ToDto(Kendaraan k) => new(
        k.NoRegistrasi,
        k.NamaPemilik,
        k.Alamat,
        k.MerkKendaraan,
        k.TahunPembuatan,
        k.KapasitasSilinder,
        k.Warna,
        k.BahanBakar);
}

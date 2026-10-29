using System.Data.Common;
using DataKendaraan.Api.Data;
using DataKendaraan.Api.Dtos;
using DataKendaraan.Api.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace DataKendaraan.Api.Services;

public enum HasilSimpan
{
    Berhasil,
    TidakDitemukan,
    Bentrok
}

public sealed class KendaraanService(AppDbContext db)
{
    public async Task<List<KendaraanDto>> CariAsync(
        string? noRegistrasi,
        string? namaPemilik,
        CancellationToken cancellationToken)
    {
        var query = db.Kendaraan.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(noRegistrasi))
        {
            var term = KendaraanNormalizer.CollapseSpaces(noRegistrasi);
            if (term is not null)
            {
                query = query.Where(k => k.NoRegistrasi.ToLower().Contains(term.ToLower()));
            }
        }

        if (!string.IsNullOrWhiteSpace(namaPemilik))
        {
            var term = KendaraanNormalizer.CollapseSpaces(namaPemilik);
            if (term is not null)
            {
                query = query.Where(k => k.NamaPemilik.ToLower().Contains(term.ToLower()));
            }
        }

        var items = await query
            .OrderBy(k => k.NoRegistrasi)
            .ToListAsync(cancellationToken);

        return items.Select(ToDto).ToList();
    }

    public async Task<KendaraanDto?> AmbilAsync(
        string noRegistrasi,
        CancellationToken cancellationToken)
    {
        var key = KendaraanNormalizer.NullIfEmpty(noRegistrasi) ?? noRegistrasi;

        var entity = await db.Kendaraan
            .AsNoTracking()
            .FirstOrDefaultAsync(k => k.NoRegistrasi == key, cancellationToken);

        return entity is null ? null : ToDto(entity);
    }

    public async Task<(HasilSimpan Hasil, KendaraanDto? Data)> TambahAsync(
        KendaraanRequest request,
        CancellationToken cancellationToken)
    {
        var noRegistrasi = KendaraanNormalizer.NullIfEmpty(request.NoRegistrasi)!;

        var sudahAda = await db.Kendaraan
            .AnyAsync(k => k.NoRegistrasi == noRegistrasi, cancellationToken);

        if (sudahAda)
        {
            return (HasilSimpan.Bentrok, null);
        }

        var entity = new Kendaraan
        {
            NoRegistrasi = noRegistrasi,
            NamaPemilik = KendaraanNormalizer.CollapseSpaces(request.NamaPemilik)!,
            Alamat = KendaraanNormalizer.CollapseSpaces(request.Alamat),
            MerkKendaraan = KendaraanNormalizer.CollapseSpaces(request.MerkKendaraan),
            TahunPembuatan = request.TahunPembuatan,
            KapasitasSilinder = request.KapasitasSilinder,
            Warna = KendaraanNormalizer.NormalizeWarna(request.Warna),
            BahanBakar = KendaraanNormalizer.CollapseSpaces(request.BahanBakar)
        };

        db.Kendaraan.Add(entity);
        await db.SaveChangesAsync(cancellationToken);

        return (HasilSimpan.Berhasil, ToDto(entity));
    }

    public async Task<(HasilSimpan Hasil, KendaraanDto? Data)> UbahAsync(
        string noRegistrasi,
        KendaraanRequest request,
        CancellationToken cancellationToken)
    {
        var keyLama = KendaraanNormalizer.NullIfEmpty(noRegistrasi) ?? noRegistrasi;
        var keyBaru = KendaraanNormalizer.NullIfEmpty(request.NoRegistrasi)!;

        var adaLama = await db.Kendaraan
            .AnyAsync(k => k.NoRegistrasi == keyLama, cancellationToken);

        if (!adaLama)
        {
            return (HasilSimpan.TidakDitemukan, null);
        }

        if (keyBaru != keyLama)
        {
            var bentrok = await db.Kendaraan
                .AnyAsync(k => k.NoRegistrasi == keyBaru, cancellationToken);

            if (bentrok)
            {
                return (HasilSimpan.Bentrok, null);
            }
        }

        var namaPemilik = KendaraanNormalizer.CollapseSpaces(request.NamaPemilik)!;
        var alamat = KendaraanNormalizer.CollapseSpaces(request.Alamat);
        var merk = KendaraanNormalizer.CollapseSpaces(request.MerkKendaraan);
        var warna = KendaraanNormalizer.NormalizeWarna(request.Warna);
        var bahanBakar = KendaraanNormalizer.CollapseSpaces(request.BahanBakar);

        try
        {
            var terpengaruh = await db.Kendaraan
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
                return (HasilSimpan.TidakDitemukan, null);
            }
        }
        catch (DbException ex) when (ex is SqliteException { SqliteErrorCode: 19 })
        {
            return (HasilSimpan.Bentrok, null);
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

        return (HasilSimpan.Berhasil, ToDto(entity));
    }

    public async Task<bool> HapusAsync(
        string noRegistrasi,
        CancellationToken cancellationToken)
    {
        var key = KendaraanNormalizer.NullIfEmpty(noRegistrasi) ?? noRegistrasi;

        var terhapus = await db.Kendaraan
            .Where(k => k.NoRegistrasi == key)
            .ExecuteDeleteAsync(cancellationToken);

        return terhapus > 0;
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

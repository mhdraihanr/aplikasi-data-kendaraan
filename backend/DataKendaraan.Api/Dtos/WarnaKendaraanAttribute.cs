using System.ComponentModel.DataAnnotations;
using DataKendaraan.Api.Services;

namespace DataKendaraan.Api.Dtos;

[AttributeUsage(AttributeTargets.Property)]
public sealed class WarnaKendaraanAttribute : ValidationAttribute
{
    public WarnaKendaraanAttribute()
        : base("Warna Kendaraan tidak valid.")
    {
    }

    public override bool IsValid(object? value)
    {
        if (value is null) return true;
        if (value is not string warna) return false;
        if (string.IsNullOrWhiteSpace(warna)) return true;

        return KendaraanNormalizer.NormalizeWarna(warna) is not null;
    }
}

using System.ComponentModel.DataAnnotations;

namespace DataKendaraan.Api.Dtos;

[AttributeUsage(AttributeTargets.Property)]
public sealed class TahunPembuatanAttribute : ValidationAttribute
{
    public const int TahunMinimal = 1900;

    public TahunPembuatanAttribute()
        : base("Tahun Pembuatan harus antara 1900 dan {0}.")
    {
    }

    public override bool IsValid(object? value)
    {
        if (value is null) return true;
        if (value is not int tahun) return false;

        return tahun >= TahunMinimal && tahun <= DateTime.Now.Year;
    }

    public override string FormatErrorMessage(string name) =>
        string.Format(ErrorMessageString, DateTime.Now.Year);
}

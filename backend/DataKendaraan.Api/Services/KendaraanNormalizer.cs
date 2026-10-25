namespace DataKendaraan.Api.Services;

public static class KendaraanNormalizer
{
    public static readonly string[] WarnaValid = ["Merah", "Hitam", "Biru", "Abu-Abu"];

    public static string? NormalizeWarna(string? warna)
    {
        var trimmed = NullIfEmpty(warna);
        if (trimmed is null) return null;

        return WarnaValid.FirstOrDefault(valid =>
            string.Equals(valid, trimmed, StringComparison.OrdinalIgnoreCase));
    }

    public static string? NullIfEmpty(string? value)
    {
        var trimmed = value?.Trim();
        return string.IsNullOrEmpty(trimmed) ? null : trimmed;
    }

    public static string? CollapseSpaces(string? value)
    {
        var trimmed = NullIfEmpty(value);
        if (trimmed is null) return null;

        return string.Join(' ', trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }
}

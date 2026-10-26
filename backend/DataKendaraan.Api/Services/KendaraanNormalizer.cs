using System.Text;

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

    // IsWhiteSpace menutup NBSP, U+2009, dan U+3000 yang lolos string.Trim().
    public static string? NullIfEmpty(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;

        var trimmed = TrimUnicodeWhitespace(value);
        return trimmed.Length == 0 ? null : trimmed;
    }

    public static string? CollapseSpaces(string? value)
    {
        var trimmed = NullIfEmpty(value);
        if (trimmed is null) return null;

        var builder = new StringBuilder(trimmed.Length);
        var sesudahSpasi = false;

        foreach (var rune in trimmed.EnumerateRunes())
        {
            if (Rune.IsWhiteSpace(rune))
            {
                if (!sesudahSpasi)
                {
                    builder.Append(' ');
                    sesudahSpasi = true;
                }
                continue;
            }

            builder.Append(rune);
            sesudahSpasi = false;
        }

        return builder.ToString();
    }

    private static string TrimUnicodeWhitespace(string value)
    {
        var awal = 0;
        var akhir = value.Length;

        while (awal < akhir && char.IsWhiteSpace(value[awal])) awal++;

        while (akhir > awal && char.IsWhiteSpace(value[akhir - 1])) akhir--;

        return value[awal..akhir];
    }
}

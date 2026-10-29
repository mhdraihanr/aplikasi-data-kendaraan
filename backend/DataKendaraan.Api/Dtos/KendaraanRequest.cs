using System.ComponentModel.DataAnnotations;

namespace DataKendaraan.Api.Dtos;

public class KendaraanRequest
{
    [Required(ErrorMessage = "No. Registrasi Kendaraan wajib diisi.")]
    [MaxLength(20, ErrorMessage = "No. Registrasi Kendaraan maksimal 20 karakter.")]
    public string NoRegistrasi { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nama Pemilik wajib diisi.")]
    [MaxLength(100, ErrorMessage = "Nama Pemilik maksimal 100 karakter.")]
    public string NamaPemilik { get; set; } = string.Empty;

    [MaxLength(255, ErrorMessage = "Alamat maksimal 255 karakter.")]
    public string? Alamat { get; set; }

    [MaxLength(100, ErrorMessage = "Merk Kendaraan maksimal 100 karakter.")]
    public string? MerkKendaraan { get; set; }

    [TahunPembuatan]
    public int? TahunPembuatan { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Kapasitas Silinder harus lebih dari 0.")]
    public int? KapasitasSilinder { get; set; }

    [WarnaKendaraan]
    public string? Warna { get; set; }

    [MaxLength(50, ErrorMessage = "Bahan Bakar maksimal 50 karakter.")]
    public string? BahanBakar { get; set; }
}

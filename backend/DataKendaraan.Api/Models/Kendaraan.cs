using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataKendaraan.Api.Models;

[Table("kendaraan")]
public class Kendaraan
{
    [Key]
    [Column("no_registrasi")]
    [MaxLength(20)]
    [Required]
    public string NoRegistrasi { get; set; } = string.Empty;

    [Column("nama_pemilik")]
    [MaxLength(100)]
    [Required]
    public string NamaPemilik { get; set; } = string.Empty;

    [Column("alamat")]
    [MaxLength(255)]
    public string? Alamat { get; set; }

    [Column("merk_kendaraan")]
    [MaxLength(100)]
    public string? MerkKendaraan { get; set; }

    [Column("tahun_pembuatan")]
    public int? TahunPembuatan { get; set; }

    [Column("kapasitas_silinder")]
    public int? KapasitasSilinder { get; set; }

    [Column("warna")]
    [MaxLength(20)]
    public string? Warna { get; set; }

    [Column("bahan_bakar")]
    [MaxLength(50)]
    public string? BahanBakar { get; set; }
}
namespace DataKendaraan.Api.Dtos;

public record KendaraanDto(
    string NoRegistrasi,
    string NamaPemilik,
    string? Alamat,
    string? MerkKendaraan,
    int? TahunPembuatan,
    int? KapasitasSilinder,
    string? Warna,
    string? BahanBakar);

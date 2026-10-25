using DataKendaraan.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace DataKendaraan.Api.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(AppDbContext db)
    {
        if (await db.Kendaraan.AnyAsync())
        {
            return;
        }

        db.Kendaraan.AddRange(
            new Kendaraan
            {
                NoRegistrasi = "B-7763-TXY",
                NamaPemilik = "Lionel Messi",
                Alamat = "Jl. Achmad Yani No 89 Jakarta Pusat",
                MerkKendaraan = "Honda PCX",
                TahunPembuatan = 2018,
                KapasitasSilinder = 150,
                Warna = "Hitam",
                BahanBakar = "Bensin"
            },
            new Kendaraan
            {
                NoRegistrasi = "B-1678-BDG",
                NamaPemilik = "Cristiano Ronaldo",
                Alamat = "Jl. Sudirman No 12 Bandung",
                MerkKendaraan = "Honda Vario",
                TahunPembuatan = 2020,
                KapasitasSilinder = 150,
                Warna = "Merah",
                BahanBakar = "Bensin"
            },
            new Kendaraan
            {
                NoRegistrasi = "B-3461-UPQ",
                NamaPemilik = "Bambang Pamungkas",
                Alamat = "Jl. Gatot Subroto No 5 Jakarta Selatan",
                MerkKendaraan = "Honda Beat",
                TahunPembuatan = 2019,
                KapasitasSilinder = 125,
                Warna = "Biru",
                BahanBakar = "Bensin"
            },
            new Kendaraan
            {
                NoRegistrasi = "B-3110-BGT",
                NamaPemilik = "Natasha Romanov",
                Alamat = "Jl. Kemang Raya No 44 Jakarta Selatan",
                MerkKendaraan = "Honda Scoopy",
                TahunPembuatan = 2020,
                KapasitasSilinder = 125,
                Warna = "Hitam",
                BahanBakar = "Bensin"
            },
            new Kendaraan
            {
                NoRegistrasi = "B-7829-TYP",
                NamaPemilik = "Entis Siti Jubaedah",
                Alamat = "Jl. Diponegoro No 7 Bandung",
                MerkKendaraan = "Honda Beat",
                TahunPembuatan = 2019,
                KapasitasSilinder = 125,
                Warna = "Merah",
                BahanBakar = "Bensin"
            });

        await db.SaveChangesAsync();
    }
}

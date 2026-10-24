using DataKendaraan.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace DataKendaraan.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Kendaraan> Kendaraan => Set<Kendaraan>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<Kendaraan>();

        entity.HasKey(x => x.NoRegistrasi);
        entity.HasIndex(x => x.NamaPemilik);

        entity.ToTable(table =>
        {
            table.HasCheckConstraint(
                "ck_kendaraan_warna",
                "warna IS NULL OR warna IN ('Merah','Hitam','Biru','Abu-Abu')");
            table.HasCheckConstraint(
                "ck_kendaraan_tahun",
                "tahun_pembuatan IS NULL OR tahun_pembuatan BETWEEN 1900 AND 2100");
            table.HasCheckConstraint(
                "ck_kendaraan_kapasitas",
                "kapasitas_silinder IS NULL OR kapasitas_silinder > 0");
        });
    }
}

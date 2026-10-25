using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataKendaraan.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "kendaraan",
                columns: table => new
                {
                    no_registrasi = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    nama_pemilik = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    alamat = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    merk_kendaraan = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    tahun_pembuatan = table.Column<int>(type: "INTEGER", nullable: true),
                    kapasitas_silinder = table.Column<int>(type: "INTEGER", nullable: true),
                    warna = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    bahan_bakar = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_kendaraan", x => x.no_registrasi);
                    table.CheckConstraint("ck_kendaraan_kapasitas", "kapasitas_silinder IS NULL OR kapasitas_silinder > 0");
                    table.CheckConstraint("ck_kendaraan_tahun", "tahun_pembuatan IS NULL OR tahun_pembuatan BETWEEN 1900 AND 2100");
                    table.CheckConstraint("ck_kendaraan_warna", "warna IS NULL OR warna IN ('Merah','Hitam','Biru','Abu-Abu')");
                });

            migrationBuilder.CreateIndex(
                name: "IX_kendaraan_nama_pemilik",
                table: "kendaraan",
                column: "nama_pemilik");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "kendaraan");
        }
    }
}

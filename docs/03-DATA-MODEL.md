# DATA MODEL — Aplikasi Data Kendaraan

Stack: **C# ASP.NET Core Web API (.NET 8) + EF Core 8**, database SQLite untuk
development dan PostgreSQL untuk production.

---

## 1. Entity Relationship

Aplikasi ini **satu tabel saja**. Tidak ada relasi.

```
┌──────────────────────────────────────────────┐
│                kendaraan                     │
├──────────────────────────────────────────────┤
│ PK  no_registrasi       VARCHAR(20)  NOT NULL│
│     nama_pemilik        VARCHAR(100) NOT NULL│
│     alamat              VARCHAR(255) NULL    │
│     merk_kendaraan      VARCHAR(100) NULL    │
│     tahun_pembuatan     INTEGER      NULL    │
│     kapasitas_silinder  INTEGER      NULL    │
│     warna               VARCHAR(20)  NULL    │
│     bahan_bakar         VARCHAR(50)  NULL    │
└──────────────────────────────────────────────┘
```

## 2. Tabel `kendaraan`

| Kolom                | Tipe           | Null | Constraint                                            | Keterangan                              |
| -------------------- | -------------- | ---- | ----------------------------------------------------- | --------------------------------------- |
| `no_registrasi`      | `VARCHAR(20)`  | NO   | **PRIMARY KEY**                                       | Nomor registrasi kendaraan (Natural PK) |
| `nama_pemilik`       | `VARCHAR(100)` | NO   | —                                                     | Wajib diisi                             |
| `alamat`             | `VARCHAR(255)` | YES  | —                                                     | Alamat pemilik                          |
| `merk_kendaraan`     | `VARCHAR(100)` | YES  | —                                                     | Contoh: Honda PCX                       |
| `tahun_pembuatan`    | `INTEGER`      | YES  | `CHECK (tahun_pembuatan BETWEEN 1900 AND 2100)`       | 4 digit                                 |
| `kapasitas_silinder` | `INTEGER`      | YES  | `CHECK (kapasitas_silinder > 0)`                      | cc                                      |
| `warna`              | `VARCHAR(20)`  | YES  | `CHECK (warna IN ('Merah','Hitam','Biru','Abu-Abu'))` | Dropdown                                |
| `bahan_bakar`        | `VARCHAR(50)`  | YES  | —                                                     | Text bebas, contoh: Bensin              |

**Catatan desain:**

- Nama kolom DB pakai **snake_case**, nama properti C# pakai **PascalCase**,
  nama field JSON API pakai **camelCase** (lihat `docs/02-DESIGN.md` §12
  untuk mapping). Ini yang diekspektasikan frontend.
- Semua kolom nullable kecuali PK & nama_pemilik, sesuai requirement
  ("Mandatory" hanya tertulis pada No Registrasi dan Nama Pemilik).
- `warna` diberi CHECK constraint karena sumbernya dropdown (nilai terbatas).

## 3. DDL (referensi)

```sql
CREATE TABLE kendaraan (
    no_registrasi      VARCHAR(20)  NOT NULL,
    nama_pemilik       VARCHAR(100) NOT NULL,
    alamat             VARCHAR(255)     NULL,
    merk_kendaraan     VARCHAR(100)     NULL,
    tahun_pembuatan    INTEGER          NULL,
    kapasitas_silinder INTEGER          NULL,
    warna              VARCHAR(20)      NULL,
    bahan_bakar        VARCHAR(50)      NULL,
    CONSTRAINT pk_kendaraan PRIMARY KEY (no_registrasi),
    CONSTRAINT ck_kendaraan_tahun CHECK (tahun_pembuatan IS NULL OR tahun_pembuatan BETWEEN 1900 AND 2100),
    CONSTRAINT ck_kendaraan_kapasitas CHECK (kapasitas_silinder IS NULL OR kapasitas_silinder > 0),
    CONSTRAINT ck_kendaraan_warna CHECK (warna IS NULL OR warna IN ('Merah','Hitam','Biru','Abu-Abu'))
);

CREATE INDEX ix_kendaraan_nama_pemilik ON kendaraan (nama_pemilik);
```

> Di EF Core, index `nama_pemilik` dibuat karena filter pencarian memakai
> `LIKE '%x%'` pada kolom nama pemilik. (Prefix index tetap membantu untuk
> sorting; pada SQLite `LIKE '%x%'` tetap full scan — ini dapat diterima
> untuk skala data kecil.)

## 4. Entity C# (EF Core)

```csharp
// Models/Kendaraan.cs
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
```

```csharp
// Data/AppDbContext.cs
using Microsoft.EntityFrameworkCore;
using DataKendaraan.Api.Models;

namespace DataKendaraan.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Kendaraan> Kendaraan => Set<Kendaraan>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        var e = b.Entity<Kendaraan>();
        e.HasKey(x => x.NoRegistrasi);
        e.HasIndex(x => x.NamaPemilik);
        // CHECK constraint warna
        e.ToTable(t => t.HasCheckConstraint(
            "ck_kendaraan_warna",
            "warna IS NULL OR warna IN ('Merah','Hitam','Biru','Abu-Abu')"));
        e.ToTable(t => t.HasCheckConstraint(
            "ck_kendaraan_tahun",
            "tahun_pembuatan IS NULL OR tahun_pembuatan BETWEEN 1900 AND 2100"));
    }
}
```

## 5. DTO (Data Transfer Object)

Entity TIDAK boleh langsung dipakai sebagai model request/response
(mencegah over-posting & menutup celah validasi).

```csharp
// Dtos/KendaraanDto.cs — bentuk response
public record KendaraanDto(
    string NoRegistrasi,
    string NamaPemilik,
    string? Alamat,
    string? MerkKendaraan,
    int? TahunPembuatan,
    int? KapasitasSilinder,
    string? Warna,
    string? BahanBakar);
```

```csharp
// Dtos/KendaraanRequest.cs — bentuk request create/update
using System.ComponentModel.DataAnnotations;

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

    [Range(1900, 2100, ErrorMessage = "Tahun Pembuatan harus antara 1900 dan 2100.")]
    public int? TahunPembuatan { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Kapasitas Silinder harus lebih dari 0.")]
    public int? KapasitasSilinder { get; set; }

    [RegularExpression("^(Merah|Hitam|Biru|Abu-Abu)?$", ErrorMessage = "Warna Kendaraan tidak valid.")]
    public string? Warna { get; set; }

    [MaxLength(50, ErrorMessage = "Bahan Bakar maksimal 50 karakter.")]
    public string? BahanBakar { get; set; }
}
```

## 6. Mapping Nama

| DB (snake_case)      | C# property         | JSON API (camelCase) | HTML form `name`    |
| -------------------- | ------------------- | -------------------- | ------------------- |
| `no_registrasi`      | `NoRegistrasi`      | `noRegistrasi`       | `noRegistrasi`      |
| `nama_pemilik`       | `NamaPemilik`       | `namaPemilik`        | `namaPemilik`       |
| `alamat`             | `Alamat`            | `alamat`             | `alamat`            |
| `merk_kendaraan`     | `MerkKendaraan`     | `merkKendaraan`      | `merkKendaraan`     |
| `tahun_pembuatan`    | `TahunPembuatan`    | `tahunPembuatan`     | `tahunPembuatan`    |
| `kapasitas_silinder` | `KapasitasSilinder` | `kapasitasSilinder`  | `kapasitasSilinder` |
| `warna`              | `Warna`             | `warna`              | `warna`             |
| `bahan_bakar`        | `BahanBakar`        | `bahanBakar`         | `bahanBakar`        |

> ASP.NET Core default men-serialize ke camelCase, jadi `KendaraanDto`
> otomatis keluar sebagai `noRegistrasi`, `namaPemilik`, dst. Jangan
> tambahkan `[JsonPropertyName]` kecuali memang perlu override.

## 7. Seed Data

Wajib di-seed saat pertama kali jalan (dipakai frontend & demo). Data diambil
dari mockup PDF:

| no_registrasi | nama_pemilik        | alamat                                 | merk         | tahun | kapasitas | warna | bahan_bakar |
| ------------- | ------------------- | -------------------------------------- | ------------ | ----- | --------- | ----- | ----------- |
| B-7763-TXY    | Lionel Messi        | Jl. Achmad Yani No 89 Jakarta Pusat    | Honda PCX    | 2018  | 150       | Hitam | Bensin      |
| B-1678-BDG    | Cristiano Ronaldo   | Jl. Sudirman No 12 Bandung             | Honda Vario  | 2020  | 150       | Merah | Bensin      |
| B-3461-UPQ    | Bambang Pamungkas   | Jl. Gatot Subroto No 5 Jakarta Selatan | Honda Beat   | 2019  | 125       | Biru  | Bensin      |
| B-3110-BGT    | Natasha Romanov     | Jl. Kemang Raya No 44 Jakarta Selatan  | Honda Scoopy | 2020  | 125       | Hitam | Bensin      |
| B-7829-TYP    | Entis Siti Jubaedah | Jl. Diponegoro No 7 Bandung            | Honda Beat   | 2019  | 125       | Merah | Bensin      |

Cara seed: `DbInitializer.SeedAsync(context)` dipanggil dari `Program.cs`
setelah `db.Database.Migrate()` — hanya menyisipkan jika tabel masih kosong.

## 8. Strategi Database

| Lingkungan  | Provider                                 | Connection string                                       |
| ----------- | ---------------------------------------- | ------------------------------------------------------- |
| Development | SQLite                                   | `Data Source=kendaraan.db`                              |
| Testing     | SQLite in-memory (`DataSource=:memory:`) | `Data Source=:memory:`                                  |
| Production  | **PostgreSQL**                           | `Host=...;Database=kendaraan;Username=...;Password=...` |

Gunakan **EF Core Migrations** (`dotnet ef migrations add InitialCreate`),
bukan `EnsureCreated()`, supaya skema bisa di-evolve nanti.

Dua provider berarti dua set migration (lihat `docs/07-DECISIONS.md` D-15).
Jangan memakai migration SQLite untuk production PostgreSQL.

## 9. Aturan Integritas

1. **PK unik**: Insert dengan `no_registrasi` yang sudah ada → tolak dengan
   HTTP `409 Conflict`, jangan silent-overwrite.
2. **Check constraint**: Divalidasi di dua lapis — DataAnnotations di DTO
   (lapis aplikasi) dan CHECK constraint di DB (lapis terakhir).
3. **Update PK**: Perubahan nomor registrasi diimplementasikan sebagai
   `Find(oldId)` → ubah properti → `SaveChanges()`; EF akan menerbitkan
   `UPDATE ... SET no_registrasi = @new WHERE no_registrasi = @old`.
   Lihat `docs/01-PRD.md` §6.3 (Business Rules).

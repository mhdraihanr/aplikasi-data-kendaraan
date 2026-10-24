# IMPLEMENTATION GUIDE — Aplikasi Data Kendaraan

Panduan langkah demi langkah untuk mengeksekusi checklist di
[`04-TASKS.md`](04-TASKS.md). Dokumen ini **hanya berisi urutan kerja dan cara
verifikasi**; sumber kebenaran tetap:

`07-DECISIONS` → `01-PRD` → `05-BUSINESS-RULES` → `03-DATA-MODEL` →
`06-API-CONTRACT` → `02-DESIGN` → `04-TASKS`.

**Status repo saat panduan dibuat (2026-10-24):** `backend/` dan `frontend/`
masih kosong; seluruh task M0–M5 belum dikerjakan. Perbarui catatan status ini
setiap kali satu milestone selesai.

---

## 0. Persiapan Sebelum Mulai

### 0.1 Prasyarat

| Tool        | Versi                       | Cek                   |
| ----------- | --------------------------- | --------------------- |
| .NET SDK    | 8.x                         | `dotnet --version`    |
| EF Core CLI | 8.x                         | `dotnet ef --version` |
| Python      | 3.x                         | `python --version`    |
| Browser     | Chrome/Edge/Firefox terbaru | —                     |

Jika `dotnet ef` belum ada:

```bash
dotnet tool install --global dotnet-ef --version 8.*
```

Pastikan folder `%USERPROFILE%\.dotnet\tools` ada di `PATH`, lalu buka ulang terminal.

### 0.2 Port dan URL

| Komponen    | URL                                      |
| ----------- | ---------------------------------------- |
| Backend API | `http://localhost:5000`                  |
| Swagger     | `http://localhost:5000/swagger`          |
| Frontend    | `http://localhost:5500`                  |
| Database    | `backend/DataKendaraan.Api/kendaraan.db` |

Jangan memakai `https://localhost:5001` untuk frontend. CORS dan `API_BASE`
disiapkan untuk `http://localhost:5000`.

### 0.3 Aturan kerja

1. Kerjakan **berurutan**: M0 → M1 → M2 → M3.1 → M3.2 → M3.3 → M3.4 → M4 → M5.
2. Jangan lanjut milestone sebelum **Gate/DoD** milestone sebelumnya lulus.
3. Backend diverifikasi dengan `curl.exe`/Swagger sebelum frontend memakainya.
4. Setiap kali task selesai, ubah `[ ]` menjadi `[x]` di `docs/04-TASKS.md`.
5. Jangan commit `bin/`, `obj/`, `*.db`, `.vs/`, atau `node_modules/`.
6. Jika menemukan keputusan baru, catat sebagai `D-xx` di `docs/07-DECISIONS.md`.
7. Satu milestone sebaiknya satu commit agar mudah di-rollback.

### 0.4 Struktur target

```text
data-kendaraan/
├── backend/
│   ├── DataKendaraan.sln
│   ├── DataKendaraan.Api/
│   │   ├── Program.cs
│   │   ├── appsettings.json
│   │   ├── appsettings.Development.json
│   │   ├── Controllers/KendaraanController.cs
│   │   ├── Models/Kendaraan.cs
│   │   ├── Data/AppDbContext.cs
│   │   ├── Data/DbInitializer.cs
│   │   ├── Dtos/KendaraanDto.cs
│   │   ├── Dtos/KendaraanRequest.cs
│   │   └── Migrations/
│   └── DataKendaraan.Api.Tests/        ← dibuat di M4
├── frontend/
│   ├── index.html
│   ├── tambah.html
│   ├── edit.html
│   ├── detail.html
│   ├── css/app.css
│   └── js/api.js, list.js, form.js, toast.js
└── docs/
```

---

## M0 — Bootstrap Project

**Target checklist:** M0-1 s/d M0-5 di `04-TASKS.md`, plus M0-0 (di luar
checklist, tapi wajib dilakukan lebih dulu).

### M0-0 — Inisialisasi Git (sebelum menulis kode)

Lakukan **sebelum M0-1**, bukan setelahnya. Tujuannya agar `bin/`, `obj/`, dan
`*.db` yang muncul saat `dotnet build/run` tidak pernah masuk staging.

```powershell
cd d:\Projects\Fullstack\data-kendaraan
git init
git add .
git commit -m "chore: dokumentasi awal dan .gitignore"
```

**Verifikasi:**

```powershell
git status --short
git ls-files | Select-String -Pattern "(^|/)(bin|obj)/|\.db$"
```

- `git status --short` harus bersih setelah commit.
- Perintah kedua **tidak boleh** menghasilkan apa pun.
- Jika `git ls-files` menampilkan `bin/`/`obj/`, berarti file sempat ter-track
  sebelum `.gitignore` ada. Perbaiki dengan:
  ```powershell
  git rm -r --cached .
  git add .
  git commit -m "chore: terapkan .gitignore"
  ```

> Repo ini **belum** punya `.git` (dicek 2026-10-24). `.gitignore` di root
> sudah mencakup `bin/`, `obj/`, `*.db`, `.vs/`, dan `node_modules/`.

### M0-1 — Buat solution dan project

```powershell
cd d:\Projects\Fullstack\data-kendaraan\backend

dotnet new webapi -n DataKendaraan.Api -f net8.0 --use-controllers
dotnet new sln -n DataKendaraan
dotnet sln add DataKendaraan.Api/DataKendaraan.Api.csproj
```

> Folder `backend/` sudah ada (berisi `.gitkeep`), jadi langsung masuk ke folder
> tersebut — tidak perlu `mkdir`.

> **Penting:** gunakan `--use-controllers`. Tanpa flag ini, template .NET 8
> membuat **minimal API**, bukan `KendaraanController` seperti yang diminta task.

Setelah project dibuat, hapus file contoh bila ada:

```powershell
Remove-Item DataKendaraan.Api/WeatherForecast.cs -ErrorAction SilentlyContinue
Remove-Item DataKendaraan.Api/Controllers/WeatherForecastController.cs -ErrorAction SilentlyContinue
```

**Verifikasi:**

```powershell
dotnet build
```

Harus muncul `Build succeeded` dan file `backend/DataKendaraan.sln` ada.

### M0-2 — Tambah paket

```powershell
cd d:\Projects\Fullstack\data-kendaraan\backend\DataKendaraan.Api

dotnet add package Microsoft.EntityFrameworkCore.Sqlite --version 8.*
dotnet add package Microsoft.EntityFrameworkCore.Design --version 8.*
```

Jika `--version 8.*` tidak diterima oleh NuGet CLI, gunakan versi `8.0.x`
terbaru yang tersedia, misalnya `--version 8.0.11`.

**Verifikasi:**

```powershell
dotnet restore
dotnet build
```

### M0-3 — Buat folder

Dari folder project `backend/DataKendaraan.Api`:

```powershell
New-Item -ItemType Directory -Force Models, Data, Dtos, Controllers
```

Folder boleh kosong dulu; file diisi pada M1 dan M2.

### M0-4 — Pastikan `.gitignore`

File `.gitignore` sudah ada di root repo. Isinya minimal harus mencakup:

```gitignore
bin/
obj/
.vs/
*.user
*.db
*.db-shm
*.db-wal
node_modules/
```

**Verifikasi:**

```powershell
# dari backend/DataKendaraan.Api
Get-Content ..\..\.gitignore
```

Tidak perlu membuat ulang jika isinya sudah sesuai.

### M0-5 — Konfigurasi koneksi, port, dan `Program.cs`

**a. Connection string** — tambahkan ke `appsettings.Development.json`
(pertahankan konfigurasi `Logging` yang sudah ada):

```json
{
  "ConnectionStrings": {
    "Default": "Data Source=kendaraan.db"
  }
}
```

**b. Port** — buka `Properties/launchSettings.json`, ubah profile HTTP menjadi:

```json
"applicationUrl": "http://localhost:5000"
```

Hapus/abaikan profile HTTPS agar tidak ada redirect ke `https://localhost:5001`.

**c. `Program.cs`** — hapus baris `app.UseHttpsRedirection();` jika ada, dan
tambahkan di paling bawah:

```csharp
public partial class Program { }
```

`public partial class Program` diperlukan agar `WebApplicationFactory<Program>`
bisa dipakai di M4.

**Verifikasi:**

```powershell
dotnet run
```

Buka `http://localhost:5000/swagger`. Swagger harus terbuka tanpa redirect HTTPS.
Hentikan server dengan `Ctrl+C`.

### Gate M0

- [ ] `dotnet build` sukses.
- [ ] `dotnet run` listens di `http://localhost:5000`.
- [ ] `/swagger` terbuka.
- [ ] Tidak ada redirect ke HTTPS.
- [ ] Checklist M0 di `04-TASKS.md` dicentang.

---

## M1 — Data Layer

**Target checklist:** M1-1 s/d M1-7 di `04-TASKS.md`.

### M1-1 — Entity `Kendaraan`

Buat `Models/Kendaraan.cs` sesuai `docs/03-DATA-MODEL.md` §4.

Yang wajib diperhatikan:

- Nama tabel: `kendaraan`.
- PK natural: `NoRegistrasi`, **tanpa** kolom `Id` auto-increment.
- `NoRegistrasi` dan `NamaPemilik` wajib; enam field lain nullable.
- Semua properti memakai `[Column("snake_case")]`.
- Panjang maksimum: 20 / 100 / 255 / 100 / 50 sesuai data model.
- Jangan menambahkan `[Required]` pada field opsional.

### M1-2 — `AppDbContext`

Buat `Data/AppDbContext.cs`:

```csharp
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Kendaraan> Kendaraan => Set<Kendaraan>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        var e = b.Entity<Kendaraan>();
        e.HasKey(x => x.NoRegistrasi);
        e.HasIndex(x => x.NamaPemilik);

        e.ToTable(t =>
        {
            t.HasCheckConstraint(
                "ck_kendaraan_warna",
                "warna IS NULL OR warna IN ('Merah','Hitam','Biru','Abu-Abu')");
            t.HasCheckConstraint(
                "ck_kendaraan_tahun",
                "tahun_pembuatan IS NULL OR tahun_pembuatan BETWEEN 1900 AND 2100");
            t.HasCheckConstraint(
                "ck_kendaraan_kapasitas",
                "kapasitas_silinder IS NULL OR kapasitas_silinder > 0");
        });
    }
}
```

Ketiga CHECK constraint diambil dari DDL `03-DATA-MODEL.md` §3. Jangan hanya
membuat constraint warna dan tahun.

### M1-3 — DTO

Buat dua file:

- `Dtos/KendaraanDto.cs` — `record` response, 8 field camelCase.
- `Dtos/KendaraanRequest.cs` — class request dengan DataAnnotations.

Aturan DTO:

- `[Required]` hanya untuk `NoRegistrasi` dan `NamaPemilik`.
- `[MaxLength]` sesuai kolom DB.
- `TahunPembuatan` memakai `[Range(1900, 2100)]` sebagai jaring pengaman;
  batas tahun berjalan dicek lagi di controller (BR-04).
- `KapasitasSilinder` memakai `[Range(1, int.MaxValue)]`.
- `Warna` memakai regex yang **mengizinkan huruf kecil dan spasi** agar
  normalisasi BR-02 bisa berjalan:

```csharp
[RegularExpression(@"(?i)^\s*(Merah|Hitam|Biru|Abu-Abu)?\s*$",
    ErrorMessage = "Warna Kendaraan tidak valid.")]
public string? Warna { get; set; }
```

Tanpa `(?i)` dan `\s*`, nilai `merah` atau `Merah` akan ditolak sebelum
controller sempat menormalisasinya.

### M1-4 — `DbInitializer`

Buat `Data/DbInitializer.cs` dengan method:

```csharp
public static async Task SeedAsync(AppDbContext db)
{
    if (await db.Kendaraan.AnyAsync()) return;

    db.Kendaraan.AddRange(
        // 5 baris dari docs/03-DATA-MODEL.md §7
    );
    await db.SaveChangesAsync();
}
```

Seed **hanya** jika tabel kosong. Jangan memakai `EnsureCreated()`.

### M1-5 — Migration

Dari folder project `backend/DataKendaraan.Api`:

```powershell
dotnet ef migrations add InitialCreate
```

**Verifikasi:**

- Folder `Migrations/` berisi file migration dan snapshot.
- `dotnet build` sukses.
- Tidak ada error `dotnet ef` tentang startup project.

### M1-6 — Registrasi DbContext dan startup

Di `Program.cs`, sebelum `builder.Build()`:

```csharp
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default")));
```

Setelah `var app = builder.Build();`:

```csharp
if (!app.Environment.IsEnvironment("Testing"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
    await DbInitializer.SeedAsync(db);
}
```

Tambahkan `using DataKendaraan.Api.Data;` dan
`using Microsoft.EntityFrameworkCore;` bila belum ada.

Guard `Testing` membuat M4 bisa memakai database in-memory tanpa ikut
menjalankan migration/seed produksi.

### M1-7 — Verifikasi database dan seed

Hentikan server, hapus DB lama, lalu jalankan ulang:

```powershell
Remove-Item kendaraan.db* -ErrorAction SilentlyContinue
dotnet run
```

**Verifikasi:**

```powershell
# Jika sqlite3 tersedia
sqlite3 kendaraan.db "SELECT COUNT(*) FROM kendaraan;"
```

Hasil harus `5`. Jika `sqlite3` tidak tersedia, lanjutkan ke M2-2 lalu cek
`GET /api/kendaraan`; hasilnya harus 5 baris.

Terakhir, restart server sekali lagi dan pastikan jumlah data tetap 5
(tidak ter-seed dua kali).

### Gate M1

- [ ] Tabel `kendaraan` terbentuk.
- [ ] 5 baris seed ada.
- [ ] Constraint warna, tahun, dan kapasitas ada.
- [ ] Restart tidak menduplikasi seed.
- [ ] Checklist M1 dicentang.

---

## M2 — REST API

**Target checklist:** seluruh bullet M2 di `04-TASKS.md`.

Semua endpoint harus muncul di Swagger dan lulus smoke test `curl.exe`.

### M2-1 — Skeleton controller

Buat `Controllers/KendaraanController.cs`:

```csharp
[ApiController]
[Route("api/kendaraan")]
public class KendaraanController : ControllerBase
{
    private readonly AppDbContext _db;

    public KendaraanController(AppDbContext db) => _db = db;

    private static KendaraanDto ToDto(Kendaraan k) => new(
        k.NoRegistrasi, k.NamaPemilik, k.Alamat, k.MerkKendaraan,
        k.TahunPembuatan, k.KapasitasSilinder, k.Warna, k.BahanBakar);
}
```

Jangan return entity langsung. Selalu map ke `KendaraanDto`.

### M2-2 — `GET /api/kendaraan`

Query parameter: `noRegistrasi`, `namaPemilik` (keduanya opsional, AND).

Implementasi:

```csharp
[HttpGet]
public async Task<ActionResult<IEnumerable<KendaraanDto>>> GetAll(
    [FromQuery] string? noRegistrasi,
    [FromQuery] string? namaPemilik)
{
    var query = _db.Kendaraan.AsNoTracking();

    if (!string.IsNullOrWhiteSpace(noRegistrasi))
    {
        var term = noRegistrasi.Trim().ToLower();
        query = query.Where(k => k.NoRegistrasi.ToLower().Contains(term));
    }

    if (!string.IsNullOrWhiteSpace(namaPemilik))
    {
        var term = namaPemilik.Trim().ToLower();
        query = query.Where(k => k.NamaPemilik.ToLower().Contains(term));
    }

    var items = await query
        .OrderBy(k => k.NoRegistrasi)
        .ToListAsync();

    return Ok(items.Select(ToDto).ToList());
}
```

Catatan:

- Filter kosong/whitespace diabaikan.
- `ToLower().Contains()` dipakai agar case-insensitive dan aman di SQLite.
- Urutan selalu `noRegistrasi` ASC.
- Tanpa hasil tetap `200` dengan `[]`.

### M2-3 — `GET /api/kendaraan/{noRegistrasi}`

```csharp
[HttpGet("{noRegistrasi}")]
public async Task<ActionResult<KendaraanDto>> GetById(string noRegistrasi)
{
    var entity = await _db.Kendaraan
        .AsNoTracking()
        .FirstOrDefaultAsync(k => k.NoRegistrasi == noRegistrasi);

    if (entity is null)
        return NotFound(new { message = "Data kendaraan tidak ditemukan." });

    return Ok(ToDto(entity));
}
```

### M2-4 — `POST /api/kendaraan`

Urutan kerja:

1. Normalisasi request (lihat M2-8).
2. Validasi warna dan tahun berjalan.
3. Cek duplikat PK dengan `AnyAsync`.
4. Map request → entity.
5. `Add` + `SaveChangesAsync`.
6. Return `201`:

```csharp
return CreatedAtAction(
    nameof(GetById),
    new { noRegistrasi = entity.NoRegistrasi },
    ToDto(entity));
```

7. Tangkap `DbUpdateException` (race condition/unique constraint) dan return
   `409` dengan `{ "message": "Nomor registrasi sudah terdaftar." }`.

Jangan silent-overwrite data lama.

### M2-5 — `PUT /api/kendaraan/{oldNoRegistrasi}`

Urutan kerja:

1. Cari entity **tracked** berdasarkan PK lama.
2. Jika tidak ada → `404`.
3. Normalisasi request.
4. Validasi warna/tahun.
5. Jika `newNoRegistrasi != oldNoRegistrasi`, cek apakah PK baru sudah dipakai
   record **lain**. Jika ya → `409`.
6. Ubah semua properti pada entity yang sama, termasuk `NoRegistrasi`.
7. Satu kali `SaveChangesAsync()` → `200` + DTO terbaru.

```csharp
var entity = await _db.Kendaraan
    .FirstOrDefaultAsync(k => k.NoRegistrasi == oldNoRegistrasi);
```

Jangan memakai `AsNoTracking()` lalu `Update()` untuk perubahan PK; gunakan
entity tracked agar EF menghasilkan `UPDATE ... WHERE no_registrasi = @old`.

Operasi update PK harus atomik. Satu `SaveChangesAsync()` sudah dibungkus
transaksi oleh EF Core; jika perlu, tambahkan `BeginTransactionAsync`.

Tetap tangkap `DbUpdateException` sebagai pengaman race condition:

```csharp
try
{
    await _db.SaveChangesAsync();
}
catch (DbUpdateException)
{
    return Conflict(new { message = "Nomor registrasi sudah terdaftar." });
}
```

### M2-6 — `DELETE /api/kendaraan/{noRegistrasi}`

```csharp
var entity = await _db.Kendaraan
    .FirstOrDefaultAsync(k => k.NoRegistrasi == noRegistrasi);

if (entity is null)
    return NotFound(new { message = "Data kendaraan tidak ditemukan." });

_db.Kendaraan.Remove(entity);
await _db.SaveChangesAsync();
return NoContent();
```

Hard delete; tanpa soft delete.

### M2-7 — Format error seragam

`[ApiController]` otomatis mengembalikan `ProblemDetails`, bukan format
kontrak. Ubah di `Program.cs`:

```csharp
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Where(kv => kv.Value?.Errors.Count > 0)
            .ToDictionary(
                kv =>
                {
                    // Key bisa "NamaPemilik", "$.namaPemilik", atau
                    // "request.NamaPemilik" — ambil segmen terakhir saja.
                    var key = kv.Key;
                    var lastDot = key.LastIndexOf('.');
                    if (lastDot >= 0) key = key[(lastDot + 1)..];
                    return JsonNamingPolicy.CamelCase.ConvertName(key);
                },
                kv => kv.Value!.Errors
                    .Select(e => string.IsNullOrWhiteSpace(e.ErrorMessage)
                        ? "Nilai tidak valid."
                        : e.ErrorMessage)
                    .ToArray());

        return new BadRequestObjectResult(new
        {
            message = "Data tidak valid.",
            errors
        });
    };
});
```

Untuk error manual (warna/tahun), gunakan bentuk yang sama:

```csharp
return BadRequest(new
{
    message = "Data tidak valid.",
    errors = new Dictionary<string, string[]>
    {
        ["warna"] = new[] { "Warna Kendaraan tidak valid." }
    }
});
```

Format akhir:

```json
{
  "message": "Data tidak valid.",
  "errors": {
    "namaPemilik": ["Nama Pemilik wajib diisi."]
  }
}
```

`errors` hanya untuk `400`. `404`/`409` cukup `message`.

### M2-8 — Normalisasi dan validasi server

Terapkan sebelum validasi/penyimpanan (BR-02):

| Field                                   | Perlakuan                                                            |
| --------------------------------------- | -------------------------------------------------------------------- |
| `noRegistrasi`                          | `Trim()`; jangan auto-uppercase                                      |
| `namaPemilik`                           | `Trim()`, kolaps spasi ganda jadi satu                               |
| `alamat`, `merkKendaraan`, `bahanBakar` | `Trim()`                                                             |
| `warna`                                 | `Trim()`, cocokkan case-insensitive ke 4 opsi, simpan bentuk kanonik |
| Field opsional kosong                   | Simpan `NULL`, bukan `""`                                            |

Validasi tambahan:

- `noRegistrasi`/`namaPemilik` kosong setelah trim → `400`.
- `tahunPembuatan > DateTime.Now.Year` → `400` dengan pesan
  `Tahun Pembuatan harus antara 1900 dan {tahunIni}.`
- `warna` di luar 4 opsi → `400` dengan key `warna`.
- `kapasitasSilinder <= 0` sudah ditangani `[Range]`, tetapi tetap defensif.

Contoh helper warna:

```csharp
// true = nilai valid (termasuk kosong); false = warna tidak dikenal
private static bool TryNormalizeWarna(string? value, out string? normalized)
{
    normalized = null;
    if (string.IsNullOrWhiteSpace(value)) return true;

    normalized = value.Trim().ToLowerInvariant() switch
    {
        "merah" => "Merah",
        "hitam" => "Hitam",
        "biru" => "Biru",
        "abu-abu" => "Abu-Abu",
        _ => null
    };

    return normalized is not null;
}
```

Pemakaian di controller:

```csharp
if (!TryNormalizeWarna(request.Warna, out var warna))
{
    return BadRequest(new
    {
        message = "Data tidak valid.",
        errors = new Dictionary<string, string[]>
        {
            ["warna"] = new[] { "Warna Kendaraan tidak valid." }
        }
    });
}

entity.Warna = warna;   // null jika input kosong
```

### M2-9 — CORS

Di `Program.cs`:

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
        policy
            .WithOrigins(
                "http://localhost:5500",
                "http://127.0.0.1:5500")
            .AllowAnyHeader()
            .AllowAnyMethod());
});
```

Lalu sebelum `app.MapControllers()`:

```csharp
app.UseCors("AllowFrontend");
```

Tidak perlu `AllowCredentials()`.

### M2-10 — Smoke test dengan `curl.exe`

Jalankan server, lalu dari terminal lain:

```powershell
# List semua
curl.exe -i http://localhost:5000/api/kendaraan

# Filter nama (case-insensitive)
curl.exe -i "http://localhost:5000/api/kendaraan?namaPemilik=messi"

# Filter no registrasi
curl.exe -i "http://localhost:5000/api/kendaraan?noRegistrasi=b-7763"

# Detail
curl.exe -i http://localhost:5000/api/kendaraan/B-7763-TXY

# Create valid
curl.exe -i -X POST http://localhost:5000/api/kendaraan `
  -H "Content-Type: application/json" `
  -d '{"noRegistrasi":"B-1234-ABC","namaPemilik":"Budi","alamat":"Jl. Mawar 1","merkKendaraan":"Yamaha NMAX","tahunPembuatan":2021,"kapasitasSilinder":155,"warna":"Biru","bahanBakar":"Bensin"}'

# Create duplikat -> 409
curl.exe -i -X POST http://localhost:5000/api/kendaraan `
  -H "Content-Type: application/json" `
  -d '{"noRegistrasi":"B-7763-TXY","namaPemilik":"Duplikat"}'

# Create tanpa nama -> 400
curl.exe -i -X POST http://localhost:5000/api/kendaraan `
  -H "Content-Type: application/json" `
  -d '{"noRegistrasi":"B-9999-ZZZ"}'

# Update
curl.exe -i -X PUT http://localhost:5000/api/kendaraan/B-1234-ABC `
  -H "Content-Type: application/json" `
  -d '{"noRegistrasi":"B-1234-XYZ","namaPemilik":"Budi Updated","warna":"merah"}'

# Delete
curl.exe -i -X DELETE http://localhost:5000/api/kendaraan/B-1234-XYZ

# Delete tidak ada -> 404
curl.exe -i -X DELETE http://localhost:5000/api/kendaraan/TIDAK-ADA
```

Cek CORS:

```powershell
curl.exe -i -H "Origin: http://localhost:5500" http://localhost:5000/api/kendaraan
```

Response harus memiliki header `Access-Control-Allow-Origin:
http://localhost:5500`.

### Gate M2

- [ ] Kelima endpoint muncul di Swagger.
- [ ] List + filter case-insensitive benar.
- [ ] Create valid `201` + `Location`.
- [ ] Duplikat `409`.
- [ ] Tanpa nama `400` dengan `errors.namaPemilik`.
- [ ] PUT PK bentrok `409`.
- [ ] DELETE sukses `204`, tidak ada `404`.
- [ ] CORS mengizinkan origin 5500.
- [ ] Checklist M2 dicentang.

---

## M3 — Frontend

**Target checklist:** M3.1 s/d M3.4 di `04-TASKS.md`.

### M3.1 — Kerangka

**M3.1-1 — `frontend/css/app.css`**

Isi minimal:

- CSS custom properties dari `docs/02-DESIGN.md` §1:
  `--primary`, `--primary-hover`, `--panel-bg`, `--secondary`, `--danger`,
  `--warning`, `--text`, `--muted`, `--border`, `--success`, `--error`.
- `.app-header`, `.app-logo`, `.app-title`.
- `.container` dengan `max-width: 1200px`, padding 24px.
- `.filter-panel`, `.filter-fields`, `.filter-actions`.
- Table: header biru, zebra row, hover, kolom `No`/`Kapasitas` rata kanan.
- `.empty-state`.
- Style field disabled agar teks tetap gelap.
- Style toast sukses/error.
- Media query: filter stack `< 768px`, tabel scroll `< 992px`.

**M3.1-2 — `frontend/js/api.js`**

Ekspos `window.VehicleApi` dengan fungsi:

```js
window.VehicleApi = {
  getList(filters),   // GET /kendaraan?...
  getOne(id),         // GET /kendaraan/{id}
  create(payload),    // POST /kendaraan
  update(oldId, payload), // PUT /kendaraan/{oldId}
  remove(id)          // DELETE /kendaraan/{id}
};
```

Ketentuan:

- `const API_BASE = window.API_BASE || 'http://localhost:5000/api';`
- Selalu `encodeURIComponent(id)`.
- Query filter hanya dikirim jika tidak kosong.
- Semua request lewat `api.js`; `list.js`/`form.js` tidak boleh `$.ajax` langsung.
- Jangan memaksa `dataType: 'json'` untuk DELETE `204` tanpa body.

**M3.1-3 — `frontend/js/toast.js`**

- Sediakan container toast di setiap halaman.
- `showToast(message, type)` dengan `type = 'success' | 'error'`.
- Auto-hide 4 detik, tombol tutup `×` dengan `aria-label="Tutup"`.
- Gunakan `.text()` untuk pesan, bukan `.html()`.

**M3.1-4 — Skeleton `index.html`**

Urutan pemuatan:

```html
<link
  href="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/css/bootstrap.min.css"
  rel="stylesheet"
/>
...
<script src="https://code.jquery.com/jquery-3.7.1.min.js"></script>
<script src="https://cdn.jsdelivr.net/npm/bootstrap@5.3.3/dist/js/bootstrap.bundle.min.js"></script>
<script src="js/toast.js"></script>
<script src="js/api.js"></script>
<script src="js/list.js"></script>
```

Sertakan header, container, filter panel, tabel, empty state, modal delete,
dan container toast.

### M3.2 — Halaman Monitoring

**M3.2-1 — Panel filter**

- Dua input: `#filterNoRegistrasi` (`name="noRegistrasi"`) dan
  `#filterNamaPemilik` (`name="namaPemilik"`).
- Tombol `Search` tipe `submit`, tombol `Add` berupa link ke
  `tambah.html?mode=tambah`.
- Setiap input punya `<label for>`.
- Form submit menangani Enter sekaligus klik Search.

**M3.2-2 — Render tabel**

- Saat `DOMContentLoaded`, panggil `VehicleApi.getList()`.
- Render 9 kolom: `No | No Registrasi | Nama Pemilik | Merk Kendaraan |
Tahun Pembuatan | Kapasitas | Warna | Bahan Bakar | Action`.
- `No` = index + 1.
- `Kapasitas` = `{n} cc`.
- Nilai `null`/kosong → `-`.
- **Alamat tidak ditampilkan.**
- Semua nilai di-escape sebelum masuk `innerHTML`.

Contoh pola baris:

```js
const escapeHtml = (value) =>
  String(value ?? "")
    .replace(/&/g, "&amp;")
    .replace(/</g, "&lt;")
    .replace(/>/g, "&gt;")
    .replace(/"/g, "&quot;")
    .replace(/'/g, "&#39;");

const formatValue = (value) =>
  value === null || value === undefined || value === ""
    ? "-"
    : escapeHtml(value);

const rows = items
  .map(
    (item, index) => `
  <tr>
    <td class="text-end">${index + 1}</td>
    <td class="fw-semibold">${escapeHtml(item.noRegistrasi)}</td>
    <td>${escapeHtml(item.namaPemilik)}</td>
    <td>${formatValue(item.merkKendaraan)}</td>
    <td>${formatValue(item.tahunPembuatan)}</td>
    <td class="text-end">${item.kapasitasSilinder ? `${item.kapasitasSilinder} cc` : "-"}</td>
    <td>${formatValue(item.warna)}</td>
    <td>${formatValue(item.bahanBakar)}</td>
    <td>
      <a class="action-detail" href="detail.html?mode=detail&id=${escapeHtml(encodeURIComponent(item.noRegistrasi))}">Detail</a>
      <a class="action-edit" href="edit.html?mode=edit&id=${escapeHtml(encodeURIComponent(item.noRegistrasi))}">Edit</a>
      <a class="action-delete" href="#" data-id="${escapeHtml(item.noRegistrasi)}">Delete</a>
    </td>
  </tr>
`,
  )
  .join("");
```

**M3.2-3 — Perilaku filter**

- `submit` → `preventDefault()`.
- Ambil nilai, trim, panggil `getList({ noRegistrasi, namaPemilik })`.
- Render ulang; nomor urut otomatis reset karena index dimulai dari 0.
- Jika hasil `[]`: sembunyikan tabel, tampilkan
  `Data kendaraan tidak ditemukan`.
- Jika ada hasil: tampilkan tabel, sembunyikan empty state.
- Jika request gagal: toast error, tabel lama tetap.

**M3.2-4 — Action link**

- `Detail` → `--warning`, `Edit` → `--primary`, `Delete` → `--danger`.
- Tanpa underline default; underline saat hover.
- Gunakan event delegation untuk klik Delete.

**M3.2-5 — Modal delete**

Markup Bootstrap:

```html
<div class="modal fade" id="deleteModal" tabindex="-1" aria-hidden="true">
  <div class="modal-dialog modal-dialog-centered">
    <div class="modal-content">
      <div class="modal-body">
        <p id="deleteModalText" class="mb-0"></p>
      </div>
      <div class="modal-footer border-0 justify-content-end">
        <button type="button" class="btn btn-primary" id="btnConfirmDelete">
          OK
        </button>
        <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">
          Batal
        </button>
      </div>
    </div>
  </div>
</div>
```

- Tanpa header/modal-title.
- Teks persis: `Anda yakin menghapus data {noReg} ?`
- `noReg` di-escape sebelum dibungkus `<strong>`.
- Tombol `OK` di kiri, `Batal` di kanan.
- Esc dan klik luar menutup modal (default Bootstrap, jangan set `keyboard:false`).

**M3.2-6 — Alur delete**

1. Klik Delete → isi teks modal → `modal.show()`.
2. Klik `OK` → disable tombol, teks `Menghapus…`.
3. `VehicleApi.remove(id)`.
4. Sukses → `modal.hide()`, reload daftar dengan filter aktif, toast
   `Data kendaraan berhasil dihapus.`
5. Gagal → `modal.hide()`, toast error, reload daftar.
6. `Batal`/Esc → tidak ada request.

**M3.2-7 — Toast dari query string**

Saat load:

```js
const status = new URLSearchParams(location.search).get("status");
const messages = {
  created: "Data kendaraan berhasil disimpan.",
  updated: "Data kendaraan berhasil diubah.",
  deleted: "Data kendaraan berhasil dihapus.",
};
if (messages[status]) {
  showToast(messages[status], "success");
  history.replaceState({}, "", location.pathname);
}
```

Bersihkan query setelah toast agar refresh tidak menampilkan toast lagi.

### M3.3 — Form Tambah/Edit/Detail

**M3.3-1 — Satu template form**

Rekomendasi: simpan template HTML sebagai string di `form.js`, render ke
`<div id="form-root"></div>`. Ini memenuhi opsi "markup dibagi via JS template"
dan menghindari duplikasi markup 3×.

Alternatif: file `form-kendaraan.html` lalu `$('#form-root').load(...)`.
Jika memilih alternatif ini, pastikan load selesai sebelum binding event.

**M3.3-2 — Mode dan ID**

Setiap halaman form wajib punya atribut `data-mode` pada `<body>`:

```html
<body data-mode="tambah">
  <!-- tambah.html -->
  <body data-mode="edit">
    <!-- edit.html -->
    <body data-mode="detail">
      <!-- detail.html -->
    </body>
  </body>
</body>
```

Lalu di `form.js`:

```js
const params = new URLSearchParams(location.search);
const mode = params.get("mode") || document.body.dataset.mode;
const id = params.get("id");
```

- `tambah.html` → form kosong (query `?mode=tambah` opsional).
- `edit.html?id=B-7763-TXY` → load data, field aktif.
- `detail.html?id=B-7763-TXY` → load data, field disabled.
- Jika `mode` edit/detail tetapi `id` kosong → tampilkan error dan kembali ke
  `index.html`.

**M3.3-3 — Layout dan field**

- 4 baris, 2 kolom (`row` + `col-md-6`), urutan DOM kiri lalu kanan:
  1. No. Registrasi Kendaraan | Tahun Pembuatan
  2. Nama Pemilik | Kapasitas Silinder
  3. Merk Kendaraan | Warna Kendaraan
  4. Alamat Pemilik Kendaraan (textarea rows=4) | Bahan Bakar
- Label di atas input, `*` merah hanya untuk No Registrasi dan Nama Pemilik.
- `Warna` = `<select>` dengan placeholder `Pilih Warna` dan 4 opsi.
- `Bahan Bakar` = `<input type="text">`, bukan dropdown.
- Setiap field punya `id`, `name` camelCase, `maxlength`, dan
  `<div data-error-for="...">` di bawahnya.

**M3.3-4 — Mode Tambah**

- Form kosong.
- Tombol primary: `Simpan`.
- Submit → `POST /api/kendaraan`.
- Sukses → `index.html?status=created`.

**M3.3-5 — Mode Edit**

- `GET /api/kendaraan/{id}` → prefill semua field.
- Tombol primary: `Ubah`.
- Submit → `PUT /api/kendaraan/{idLama}` dengan body berisi `noRegistrasi`
  baru (jika diubah).
- Sukses → `index.html?status=updated`.

**M3.3-6 — Mode Detail**

- `GET /api/kendaraan/{id}` → prefill.
- Semua input/select/textarea `disabled`.
- Sembunyikan tombol primary; hanya tampilkan `Kembali`.
- Tidak ada request `PUT` dalam kondisi apa pun.
- Style disabled tetap terbaca gelap.

**M3.3-7 — Validasi client**

| Field         | Aturan                                    | Pesan                                                            |
| ------------- | ----------------------------------------- | ---------------------------------------------------------------- |
| No Registrasi | wajib, maks 20                            | `No. Registrasi Kendaraan wajib diisi.`                          |
| Nama Pemilik  | wajib, maks 100                           | `Nama Pemilik wajib diisi.`                                      |
| Alamat        | maks 255                                  | `Alamat maksimal 255 karakter.`                                  |
| Merk          | maks 100                                  | `Merk Kendaraan maksimal 100 karakter.`                          |
| Tahun         | jika diisi: bilangan bulat 1900–tahun ini | `Tahun Pembuatan harus berupa angka antara 1900 dan {tahunIni}.` |
| Kapasitas     | jika diisi: bilangan bulat > 0            | `Kapasitas Silinder harus berupa angka lebih dari 0.`            |
| Bahan Bakar   | maks 50                                   | `Bahan Bakar maksimal 50 karakter.`                              |

- Jalankan saat `blur` dan saat submit.
- Tambahkan `is-invalid` pada input dan tampilkan pesan di
  `[data-error-for="{name}"]`.
- Hapus pesan saat user mengetik.
- Jangan hanya mengandalkan `required` HTML5.

**M3.3-8 — Submit dan error server**

- Bangun payload: trim string, kosong → `null`, angka → `Number`.
- `POST`/`PUT` lewat `VehicleApi`.
- Sukses → redirect + query status.
- `400` → map `responseJSON.errors` ke field terkait.
- `409` → tampilkan `Nomor registrasi sudah terdaftar.` di field
  `noRegistrasi` (dan/atau toast).
- `404`/`500`/network → toast
  `Terjadi kesalahan. Silakan coba lagi.` atau
  `Tidak dapat terhubung ke server. Silakan coba lagi.`
- Jangan tampilkan stack trace.

**M3.3-9 — Tombol Kembali**

- Selalu ke `index.html`.
- Opsional: jika form berubah dan mode bukan detail, konfirmasi
  `Data yang belum disimpan akan hilang. Lanjutkan?`
  Ini nice-to-have, bukan syarat DoD.

### M3.4 — Polish

- **Responsif:** `.table-responsive` untuk tabel; filter stack penuh di
  `< 768px`.
- **Aksesibilitas:** `<label for>` di semua input; `aria-label` pada tombol
  ikon; modal bisa ditutup Esc.
- **Console bersih:** tidak ada error 404 aset, JS error, atau CORS error.
- **XSS:** semua nilai API di-escape sebelum `innerHTML`; pesan toast memakai
  `.text()`.

### Gate M3

- [ ] Monitoring menampilkan 5 data seed.
- [ ] Search `b-` dan `messi` bekerja.
- [ ] Search `zzz` menampilkan empty state.
- [ ] Tambah data berhasil + toast.
- [ ] Edit data berhasil + toast.
- [ ] Detail semua field disabled, hanya Kembali.
- [ ] Delete Batal tidak menghapus; Delete OK menghapus + toast.
- [ ] Tidak ada error console.
- [ ] Checklist M3 dicentang.

---

## M4 — Testing

**Target checklist:** seluruh bullet M4 di `04-TASKS.md`.

### M4-1 — Smoke test manual 12 skenario

| #   | Skenario               | Ekspektasi                                 |
| --- | ---------------------- | ------------------------------------------ |
| 1   | Buka Monitoring        | 5 baris seed tampil                        |
| 2   | Search `b-`            | semua baris `B-` tampil (case-insensitive) |
| 3   | Search `messi`         | 1 baris Lionel Messi                       |
| 4   | Search `zzz`           | `Data kendaraan tidak ditemukan`           |
| 5   | Add data lengkap       | redirect + toast sukses, data muncul       |
| 6   | Add tanpa Nama Pemilik | pesan wajib isi, tidak submit              |
| 7   | Add No Reg duplikat    | `Nomor registrasi sudah terdaftar.`        |
| 8   | Edit tahun `abc`       | pesan tahun tidak valid                    |
| 9   | Detail                 | semua field disabled, tanpa tombol Ubah    |
| 10  | Delete → Batal         | modal tutup, data masih ada                |
| 11  | Delete → OK            | modal tutup, baris hilang, toast sukses    |
| 12  | Restart server         | data tetap ada (SQLite persisten)          |

Catatan skenario 8: input `type="number"` di browser dapat menolak huruf.
Uji juga lewat `PUT` langsung dengan `tahunPembuatan` string/angka di luar
rentang untuk memastikan validasi server.

### M4-2 — Project test

Dari root repo:

```powershell
cd backend
dotnet new xunit -n DataKendaraan.Api.Tests -f net8.0
dotnet sln add DataKendaraan.Api.Tests/DataKendaraan.Api.Tests.csproj
dotnet add DataKendaraan.Api.Tests reference DataKendaraan.Api/DataKendaraan.Api.csproj
dotnet add DataKendaraan.Api.Tests package Microsoft.AspNetCore.Mvc.Testing --version 8.*
```

### M4-3 — Test factory

Gunakan SQLite in-memory dengan koneksi yang tetap terbuka:

```csharp
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

public class ApiFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureServices(services =>
        {
            // Ganti DbContext produksi dengan SQLite in-memory
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<AppDbContext>();

            _connection.Open();
            services.AddDbContext<AppDbContext>(o => o.UseSqlite(_connection));
        });
    }

    // Panggil sekali per test class, sebelum test pertama
    public async Task InitializeDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.EnsureCreatedAsync();
        await DbInitializer.SeedAsync(db);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing) _connection.Dispose();
    }
}
```

Contoh pemakaian di test class:

```csharp
public class KendaraanApiTests : IAsyncLifetime
{
    private readonly ApiFactory _factory = new();
    private HttpClient _client = null!;

    public async Task InitializeAsync()
    {
        await _factory.InitializeDatabaseAsync();
        _client = _factory.CreateClient();
    }

    public Task DisposeAsync()
    {
        _client.Dispose();
        _factory.Dispose();
        return Task.CompletedTask;
    }
}
```

> Jika test masih membaca `kendaraan.db` (bukan in-memory), berarti konfigurasi
> DbContext produksi belum tergantikan. Pada EF Core 8 tambahkan
> `services.RemoveAll<DbContextOptions<AppDbContext>>();` **dan**
> `services.RemoveAll<AppDbContext>();`. Pada EF Core 9+ hapus juga
> `IDbContextOptionsConfiguration<AppDbContext>`
> (`using Microsoft.EntityFrameworkCore.Infrastructure;`).

Karena `Program.cs` melewati migrate/seed saat environment `Testing`, test
harus memanggil `EnsureCreated()` dan seed sendiri.

### M4-4 — Enam test minimal

| #   | Test                             | Assert                              |
| --- | -------------------------------- | ----------------------------------- |
| 1   | `POST` valid                     | `201`, `Location`, body sesuai      |
| 2   | `POST` duplikat `B-7763-TXY`     | `409`                               |
| 3   | `POST` tanpa `namaPemilik`       | `400`, ada `errors.namaPemilik`     |
| 4   | `PUT` PK baru yang sudah dipakai | `409`                               |
| 5   | `DELETE` ID tidak ada            | `404`                               |
| 6   | `GET ?namaPemilik=messi`         | `200`, 1 hasil, nama `Lionel Messi` |

Gunakan `JsonSerializerOptions(JsonSerializerDefaults.Web)` saat membaca
response. Jalankan:

```powershell
cd backend
dotnet test
```

Semua test harus hijau. Jika test saling memengaruhi, buat factory baru per
test class atau nonaktifkan paralelisasi collection.

### Gate M4

- [ ] 12 skenario manual lulus.
- [ ] Minimal 6 test otomatis hijau.
- [ ] `dotnet test` sukses tanpa warning kritis.
- [ ] Checklist M4 dicentang.

---

## M5 — Delivery

**Target checklist:** seluruh bullet M5 di `04-TASKS.md`.

### M5-1 — Update `README.md`

README saat ini masih menyatakan "belum ada kode". Setelah implementasi,
perbarui dengan:

- Prasyarat (.NET 8, EF tools, Python).
- Cara menjalankan backend:
  ```powershell
  cd backend/DataKendaraan.Api
  dotnet ef database update
  dotnet run
  ```
- Cara menjalankan frontend:
  ```powershell
  cd frontend
  python -m http.server 5500
  ```
- URL: `http://localhost:5500`, Swagger `http://localhost:5000/swagger`.
- Daftar endpoint dan contoh request.
- Catatan seed 5 data.
- Struktur folder singkat.

### M5-2 — PDF sumber

PDF sudah ada di `docs/source/Application Data Kendaraan.pdf`. Cukup verifikasi
keberadaannya; tidak perlu menyalin ulang.

### M5-3 — Screenshot

Ambil 4 layar dan simpan ke `docs/screenshots/`:

1. Monitoring (dengan data seed).
2. Tambah Data.
3. Edit Data.
4. Detail Data.

Opsional: modal konfirmasi delete. Gunakan lebar desktop ~1440px agar layout
sesuai mockup.

### M5-4 — Konsistensi

- Tahun ditampilkan apa adanya, 4 digit.
- Kapasitas selalu `{n} cc` di tabel, angka saja di form.
- Format tanggal/waktu tidak relevan karena tidak ada field tanggal.
- Pesan error konsisten Bahasa Indonesia dan diakhiri titik.

### M5-5 — Pemeriksaan akhir Git

```powershell
git status --short
git ls-files | Select-String -Pattern "(^|/)(bin|obj)/|\.db$"
```

Tidak boleh ada `bin/`, `obj/`, `*.db`, atau `node_modules/` yang ter-track.

### Gate M5

- [ ] README bisa diikuti dari nol.
- [ ] PDF sumber ada.
- [ ] 4 screenshot ada.
- [ ] Tidak ada artefak build/database di Git.
- [ ] Checklist M5 dicentang.

---

## Lampiran A — Peta Task → Langkah

| Task di `04-TASKS.md`  | Langkah panduan   |
| ---------------------- | ----------------- |
| — (prasyarat)          | M0-0 (git init)   |
| M0 — Bootstrap Project | M0-1 s/d M0-5     |
| M1 — Data Layer        | M1-1 s/d M1-7     |
| M2 — REST API          | M2-1 s/d M2-10    |
| M3.1 — Kerangka        | M3.1-1 s/d M3.1-4 |
| M3.2 — Monitoring      | M3.2-1 s/d M3.2-7 |
| M3.3 — Form            | M3.3-1 s/d M3.3-9 |
| M3.4 — Polish          | M3.4              |
| M4 — Testing           | M4-1 s/d M4-4     |
| M5 — Delivery          | M5-1 s/d M5-5     |

---

## Lampiran B — Jebakan Umum

| Gejala                          | Penyebab                                   | Solusi                                                              |
| ------------------------------- | ------------------------------------------ | ------------------------------------------------------------------- |
| `KendaraanController` tidak ada | Template .NET 8 memakai minimal API        | Buat project dengan `--use-controllers`                             |
| Swagger tidak terbuka           | Port bukan 5000 / HTTPS redirect           | Set `applicationUrl`, hapus `UseHttpsRedirection`                   |
| `dotnet ef` not found           | Global tool belum di PATH                  | Install `dotnet-ef`, buka ulang terminal                            |
| Frontend kena CORS              | Origin tidak persis / server bukan 5500    | Tambahkan `http://localhost:5500` dan `http://127.0.0.1:5500`       |
| `PUT` PK berubah jadi insert    | Entity detached + `Update()`               | Load tracked entity, ubah properti, `SaveChanges()`                 |
| Filter `messi` tidak menemukan  | Case-sensitive                             | Gunakan `ToLower().Contains()`                                      |
| `merah` ditolak `400`           | Regex DTO case-sensitive                   | Tambahkan `(?i)` dan izinkan whitespace, atau validasi setelah trim |
| Error 400 tidak sesuai format   | `[ApiController]` memakai `ProblemDetails` | Set `InvalidModelStateResponseFactory`                              |
| DELETE memicu parse error       | `dataType: 'json'` pada `204`              | Jangan paksa JSON untuk DELETE                                      |
| Data seed dobel                 | Seed tidak mengecek tabel kosong           | `if (await db.Kendaraan.AnyAsync()) return;`                        |
| Test saling memengaruhi         | Database in-memory dibagi                  | Factory/connection baru per test class                              |
| XSS di tabel                    | Nilai API langsung masuk `innerHTML`       | `escapeHtml()` / `textContent`                                      |

---

## Lampiran C — Urutan Commit yang Disarankan

```text
chore: bootstrap .NET solution dan konfigurasi SQLite
feat(api): entity, dbcontext, seed, dan migration awal
feat(api): endpoint CRUD kendaraan + validasi + CORS
feat(web): halaman monitoring, filter, dan modal delete
feat(web): form tambah/edit/detail
style(web): responsive dan polish aksesibilitas
test(api): integration test controller
docs: update README dan screenshot
```

---

## Lampiran D — Checklist Akhir Proyek

- [ ] Semua task M0–M5 di `04-TASKS.md` berstatus `[x]`.
- [ ] 5 data seed tampil setelah clone + run pertama.
- [ ] CRUD berjalan end-to-end lewat browser.
- [ ] Validasi client dan server aktif.
- [ ] Tidak ada error console/CORS.
- [ ] 12 skenario manual lulus.
- [ ] 6 test otomatis hijau.
- [ ] README dapat diikuti dari nol.
- [ ] Tidak ada `bin/`, `obj/`, `*.db` di commit.

# Aplikasi Data Kendaraan

Aplikasi web untuk mencatat dan mengelola data kendaraan: daftar + pencarian,
tambah, ubah, detail (read-only), dan hapus dengan konfirmasi.

Dokumen ini dihasilkan dari analisis `Application Data Kendaraan.pdf`.
Backend sudah berjalan sampai M1 (entity, `AppDbContext`, DTO); API dan frontend
belum. Lihat [`docs/04-TASKS.md`](docs/04-TASKS.md) untuk progres terbaru.

---

## 📚 Peta Dokumen

| #   | Dokumen                                                              | Isi                                                                      | Baca ini kalau…                               |
| --- | -------------------------------------------------------------------- | ------------------------------------------------------------------------ | --------------------------------------------- |
| 01  | [`docs/01-PRD.md`](docs/01-PRD.md)                                   | Requirement, user story, acceptance criteria, field spec, NFR            | Butuh tahu **apa** yang dibangun              |
| 02  | [`docs/02-DESIGN.md`](docs/02-DESIGN.md)                             | Wireframe per layar, markup Bootstrap, palet warna, pesan validasi       | Butuh tahu **tampilannya seperti apa**        |
| 03  | [`docs/03-DATA-MODEL.md`](docs/03-DATA-MODEL.md)                     | Skema tabel, DDL, entity C#/EF Core, DTO, seed data                      | Butuh tahu **struktur datanya**               |
| 04  | [`docs/04-TASKS.md`](docs/04-TASKS.md)                               | Checklist milestone M0–M5 + test case                                    | Mau **mulai implementasi**                    |
| 05  | [`docs/05-BUSINESS-RULES.md`](docs/05-BUSINESS-RULES.md)             | 15 aturan bisnis bernomor (BR-01…BR-15): validasi, PK, pencarian, delete | Butuh tahu **perilaku detail di balik layar** |
| 06  | [`docs/06-API-CONTRACT.md`](docs/06-API-CONTRACT.md)                 | Endpoint, request/response JSON, status code                             | Butuh **menghubungkan frontend ↔ backend**    |
| 07  | [`docs/07-DECISIONS.md`](docs/07-DECISIONS.md)                       | Semua ambiguitas dokumen asli + keputusannya                             | **Ragu soal requirement** (baca dulu!)        |
| 08  | [`docs/08-IMPLEMENTATION-GUIDE.md`](docs/08-IMPLEMENTATION-GUIDE.md) | Panduan langkah per milestone + verifikasi + jebakan umum                | **Mulai coding step-by-step**                 |

**Alur baca yang disarankan:** `07-DECISIONS` → `01-PRD` → `05-BUSINESS-RULES`
→ `03-DATA-MODEL` → `06-API-CONTRACT` → `02-DESIGN` → `04-TASKS` →
`08-IMPLEMENTATION-GUIDE` (saat mulai coding).

---

## 🎯 Ringkasan Aplikasi

**Stack:** C# ASP.NET Core Web API (.NET 8) + EF Core · HTML/jQuery/Bootstrap 5
**Database:** SQLite untuk development, **PostgreSQL untuk production**

**Layar:**

| Layar       | Fungsi                                                                     |
| ----------- | -------------------------------------------------------------------------- |
| Monitoring  | Tabel kendaraan + filter No Registrasi & Nama Pemilik, tombol Search & Add |
| Tambah Data | Form 2 kolom, tombol Simpan & Kembali                                      |
| Edit Data   | Form sama, terisi, tombol Ubah & Kembali                                   |
| Detail Data | Form sama, semua field disabled, hanya tombol Kembali                      |
| Modal Hapus | "Anda yakin menghapus data {noReg} ?" → OK / Batal                         |

**Field:**

| Field                      | Tipe                                     | Wajib |
| -------------------------- | ---------------------------------------- | ----- |
| Nomor Registrasi Kendaraan | Text (PK)                                | ✅    |
| Nama Pemilik               | Text                                     | ✅    |
| Alamat                     | Textarea                                 | —     |
| Merk Kendaraan             | Text                                     | —     |
| Tahun Pembuatan            | Numeric, 4 digit                         | —     |
| Kapasitas Silinder         | Numeric                                  | —     |
| Warna Kendaraan            | Dropdown: Merah / Hitam / Biru / Abu-Abu | —     |
| Bahan Bakar                | Text (contoh: Bensin)                    | —     |

> ⚠️ Dua hal di dokumen asli tertukar (Bahan Bakar berisi nama warna, Warna
> tertulis "Text"). Lihat [`docs/07-DECISIONS.md`](docs/07-DECISIONS.md) D-01 & D-02.

---

## 🚀 Rencana Menjalankan (setelah kode dibuat)

**Development — SQLite:**

```bash
# Backend — port 5000
cd backend/DataKendaraan.Api
dotnet ef database update
dotnet run

# Frontend — port 5500
cd frontend
python -m http.server 5500
```

Buka `http://localhost:5500`.

**Production — PostgreSQL:**

```bash
# Set connection string lewat environment variable, jangan commit kredensial
export ConnectionStrings__Default="Host=localhost;Database=kendaraan;Username=app;Password=***"
export ASPNETCORE_ENVIRONMENT=Production

cd backend/DataKendaraan.Api
dotnet ef database update
dotnet run
```

> Status: dukungan PostgreSQL **belum diimplementasikan**. Saat ini project hanya
> mereferensikan `Microsoft.EntityFrameworkCore.Sqlite`. Untuk mengaktifkannya
> perlu paket `Npgsql.EntityFrameworkCore.PostgreSQL`, `appsettings.Production.json`,
> dan pemilihan provider di `Program.cs`. Lihat
> [`docs/04-TASKS.md`](docs/04-TASKS.md) M1-6.

---

## 📂 Struktur Target

```
data-kendaraan/
├── README.md
├── docs/
│   ├── 01-PRD.md
│   ├── 02-DESIGN.md
│   ├── 03-DATA-MODEL.md
│   ├── 04-TASKS.md
│   ├── 05-BUSINESS-RULES.md
│   ├── 06-API-CONTRACT.md
│   ├── 07-DECISIONS.md
│   ├── 08-IMPLEMENTATION-GUIDE.md
│   ├── source/                 ← PDF asli
│   └── screenshots/            ← mockup hasil render
├── backend/
│   ├── DataKendaraan.sln
│   └── DataKendaraan.Api/
│       ├── Models/             ← entity Kendaraan
│       ├── Data/               ← AppDbContext
│       ├── Dtos/               ← request & response
│       ├── Controllers/        ← belum diisi (M2)
│       └── Migrations/         ← belum dibuat (M1-5)
└── frontend/                   ← belum dibuat (lihat 04-TASKS.md M3)
```

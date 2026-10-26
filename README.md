# Aplikasi Data Kendaraan

Aplikasi web untuk mencatat dan mengelola data kendaraan. Mendukung daftar dan
pencarian, tambah, ubah, detail read-only, serta hapus dengan konfirmasi.

| Lapisan  | Teknologi                                                |
| -------- | -------------------------------------------------------- |
| Backend  | ASP.NET Core Web API .NET 8, EF Core 8,                  |
| Frontend | HTML, jQuery 3.7.1, Bootstrap 5.3.3 dari CDN jsDelivr    |
| Database | SQLite lewat `Microsoft.EntityFrameworkCore.Sqlite` 8.\* |
| Test     | xUnit 2.5.3, `Microsoft.AspNetCore.Mvc.Testing` 8.\*     |

## Showcase

![Showcase Aplikasi Data Kendaraan Dashboard, empat layar: List Data, Tambah Data, Edit Data, Hapus Data](docs/screenshots/source-mockup/Showcase%20Aplikasi%20Data%20Kendaraan%20Dashboard.png)

Gambar mockup sumber untuk layar 01 List Data, 02 Tambah Data, 03 Edit Data, dan
04 Hapus Data. Rincian wireframe ada di
[`docs/02-DESIGN.md`](docs/02-DESIGN.md).

## Peta Dokumen

| #   | Dokumen                                                              | Isi                                                             |
| --- | -------------------------------------------------------------------- | --------------------------------------------------------------- |
| 01  | [`docs/01-PRD.md`](docs/01-PRD.md)                                   | Requirement, user story, acceptance criteria, field, NFR        |
| 02  | [`docs/02-DESIGN.md`](docs/02-DESIGN.md)                             | Wireframe per layar, markup Bootstrap, palet warna, pesan error |
| 03  | [`docs/03-DATA-MODEL.md`](docs/03-DATA-MODEL.md)                     | Skema tabel, DDL, entity C#/EF Core, DTO, seed data             |
| 04  | [`docs/04-TASKS.md`](docs/04-TASKS.md)                               | Checklist milestone M0 sampai M5 beserta test case              |
| 05  | [`docs/05-BUSINESS-RULES.md`](docs/05-BUSINESS-RULES.md)             | 15 aturan bisnis BR-01 sampai BR-15                             |
| 06  | [`docs/06-API-CONTRACT.md`](docs/06-API-CONTRACT.md)                 | Endpoint, request/response JSON, status code                    |
| 08  | [`docs/08-IMPLEMENTATION-GUIDE.md`](docs/08-IMPLEMENTATION-GUIDE.md) | Panduan per milestone, verifikasi, jebakan umum                 |

Urutan baca yang disarankan: `01-PRD` lalu `05-BUSINESS-RULES`, `03-DATA-MODEL`,
`06-API-CONTRACT`, `02-DESIGN`, `04-TASKS`, dan `08-IMPLEMENTATION-GUIDE` saat
mulai coding.

## Ringkasan Aplikasi

Stack: ASP.NET Core Web API (.NET 8) dengan EF Core, frontend HTML dengan jQuery
dan Bootstrap 5. Database memakai SQLite untuk development dan direncanakan
PostgreSQL untuk production.

| Layar       | Fungsi                                                                    |
| ----------- | ------------------------------------------------------------------------- |
| Monitoring  | Tabel kendaraan, filter No Registrasi dan Nama Pemilik, tombol Search/Add |
| Tambah Data | Form 2 kolom, tombol Simpan dan Kembali                                   |
| Edit Data   | Form sama dengan data terisi, tombol Ubah dan Kembali                     |
| Detail Data | Form sama, semua field disabled, hanya tombol Kembali                     |
| Modal Hapus | Teks "Anda yakin menghapus data {noReg} ?" dengan tombol OK dan Batal     |

| Field                      | Tipe                                  | Wajib |
| -------------------------- | ------------------------------------- | ----- |
| Nomor Registrasi Kendaraan | Text, primary key                     | Ya    |
| Nama Pemilik               | Text                                  | Ya    |
| Alamat                     | Textarea                              | Tidak |
| Merk Kendaraan             | Text                                  | Tidak |
| Tahun Pembuatan            | Numeric, 4 digit                      | Tidak |
| Kapasitas Silinder         | Numeric                               | Tidak |
| Warna Kendaraan            | Dropdown: Merah, Hitam, Biru, Abu-Abu | Tidak |
| Bahan Bakar                | Text, contoh Bensin                   | Tidak |

Dua hal pada dokumen asli tertukar. Tabel requirement menaruh nama warna pada
field Bahan Bakar dan menulis Warna sebagai Text. Aplikasi ini mengikuti mockup,
sehingga Warna berupa dropdown `Merah / Hitam / Biru / Abu-Abu` dan Bahan Bakar
berupa teks bebas.

---

## Cara Menjalankan

Prasyarat:

- .NET 8 SDK, wajib
- Node.js, dipakai untuk menjalankan frontend lewat `npx http-server`
- Koneksi internet, karena Bootstrap 5.3.3 dan jQuery 3.7.1 dimuat dari CDN
  jsDelivr

Langkah 1, backend pada port 5000:

```bash
cd backend/DataKendaraan.Api
dotnet tool restore
dotnet run
```

`dotnet tool restore` cukup dijalankan sekali untuk memasang dotnet-ef 8.0.32.
Pada saat pertama kali jalan, `kendaraan.db` dibuat otomatis dan diisi 5 data
contoh. Swagger tersedia di <http://localhost:5000/swagger>.

Langkah 2, frontend pada port 5500. Buka terminal baru:

```bash
cd frontend
npx http-server . -p 5500 -c-1
```

Buka <http://localhost:5500/index.html>. Port harus 5500 karena backend hanya
mengizinkan CORS dari `http://localhost:5500` dan `http://127.0.0.1:5500`.

Alternatif menjalankan frontend, pilih salah satu dan tetap pada port 5500:

| Cara                | Perintah                                                      | Kebutuhan                           |
| ------------------- | ------------------------------------------------------------- | ----------------------------------- |
| `npx http-server`   | `npx http-server . -p 5500 -c-1`                              | Node.js                             |
| Python              | `py -m http.server 5500`                                      | Python, gunakan `py` bukan `python` |
| VS Code Live Server | klik kanan `frontend/index.html`, pilih Open with Live Server | VS Code dan ekstensinya             |

Catatan untuk Live Server: URL menjadi `/frontend/index.html` karena ekstensi
melayani root repo. Halaman juga auto-reload saat `kendaraan.db` berubah,
sehingga toast setelah simpan atau hapus bisa hilang. `npx http-server` tidak
memiliki efek ini.

Langkah 3, menjalankan test:

```bash
cd backend
dotnet test
```

Untuk production dengan PostgreSQL:

```bash
export ConnectionStrings__Default="Host=localhost;Database=kendaraan;Username=app;Password=***"
export ASPNETCORE_ENVIRONMENT=Production

cd backend/DataKendaraan.Api
dotnet ef database update
dotnet run
```

Dukungan PostgreSQL belum diimplementasikan. Project saat ini hanya
mereferensikan `Microsoft.EntityFrameworkCore.Sqlite`. Untuk mengaktifkannya
perlu paket `Npgsql.EntityFrameworkCore.PostgreSQL`, `appsettings.Production.json`,
dan pemilihan provider di `Program.cs`. Lihat M1-6 pada
[`docs/04-TASKS.md`](docs/04-TASKS.md).

---

## Endpoint API

Base URL: `http://localhost:5000/api`

| Method   | Path                        | Fungsi                                    | Sukses               | Gagal               |
| -------- | --------------------------- | ----------------------------------------- | -------------------- | ------------------- |
| `GET`    | `/kendaraan`                | Daftar kendaraan, bisa difilter           | `200`                |                     |
| `GET`    | `/kendaraan/{noRegistrasi}` | Detail satu kendaraan                     | `200`                | `404`               |
| `POST`   | `/kendaraan`                | Tambah kendaraan                          | `201` dan `Location` | `400`, `409`        |
| `PUT`    | `/kendaraan/{noRegistrasi}` | Ubah kendaraan, primary key boleh berubah | `200`                | `400`, `404`, `409` |
| `DELETE` | `/kendaraan/{noRegistrasi}` | Hapus kendaraan                           | `204`                | `404`               |

Filter memakai query string, case-insensitive, dan mencocokkan sebagian:

```bash
curl "http://localhost:5000/api/kendaraan?noRegistrasi=b-&namaPemilik=messi"
```

Contoh menambah data:

```bash
curl -X POST http://localhost:5000/api/kendaraan \
  -H "Content-Type: application/json" \
  -d '{"noRegistrasi":"B-1234-XYZ","namaPemilik":"Budi","tahunPembuatan":2021,"kapasitasSilinder":150,"merkKendaraan":"Honda Vario","warna":"Merah","bahanBakar":"Bensin"}'
```

Semua endpoint memakai bentuk error yang sama:

```json
{
  "message": "Data tidak valid.",
  "errors": { "namaPemilik": ["Nama Pemilik wajib diisi."] }
}
```

Field `errors` hanya muncul pada status 400. Kontrak lengkap ada di
[`docs/06-API-CONTRACT.md`](docs/06-API-CONTRACT.md).

---

## Struktur Folder

```text
aplikasi-data-kendaraan/
├── backend/
│   ├── DataKendaraan.sln
│   ├── DataKendaraan.Api/            # Web API .NET 8
│   │   ├── Controllers/              # KendaraanController, 5 endpoint
│   │   ├── Data/                     # AppDbContext, DbInitializer
│   │   ├── Dtos/                     # KendaraanRequest, KendaraanDto, ApiError
│   │   ├── Models/                   # entity Kendaraan
│   │   ├── Services/                 # KendaraanNormalizer
│   │   ├── Validation/               # TahunPembuatan, WarnaKendaraan
│   │   ├── Migrations/
│   │   └── kendaraan.db              # SQLite, tidak di-commit
│   └── DataKendaraan.Api.Tests/      # xUnit dan WebApplicationFactory
├── frontend/                         # statis, HTML/jQuery/Bootstrap 5
│   ├── index.html                    # Monitoring
│   ├── form.html                     # Tambah, Edit, Detail lewat ?mode=
│   ├── css/app.css
│   └── js/                           # api, list, form, toast, pesan
└── docs/                             # PRD, DESIGN, DATA-MODEL, TASKS, dll
```

---

## Data Contoh

Database diisi 5 kendaraan saat pertama kali dijalankan.

| No Registrasi | Nama Pemilik        | Merk         | Tahun | Kapasitas | Warna |
| ------------- | ------------------- | ------------ | ----- | --------- | ----- |
| `B-1678-BDG`  | Cristiano Ronaldo   | Honda Vario  | 2020  | 150 cc    | Merah |
| `B-3110-BGT`  | Natasha Romanov     | Honda Scoopy | 2020  | 125 cc    | Hitam |
| `B-3461-UPQ`  | Bambang Pamungkas   | Honda Beat   | 2019  | 125 cc    | Biru  |
| `B-7763-TXY`  | Lionel Messi        | Honda PCX    | 2018  | 150 cc    | Hitam |
| `B-7829-TYP`  | Entis Siti Jubaedah | Honda Beat   | 2019  | 125 cc    | Merah |

Seed hanya dijalankan bila tabel masih kosong, jadi data tidak akan dobel.

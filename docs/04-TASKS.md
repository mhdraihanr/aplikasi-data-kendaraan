# TASKS — Aplikasi Data Kendaraan

Checklist implementasi. Kerjakan **berurutan per milestone**; setiap milestone
harus bisa di-demo end-to-end sebelum lanjut.

Legend: `[ ]` belum · `[x]` selesai · `[~]` sedang dikerjakan

---

## M0 — Bootstrap Project

- [ ] Buat solution & project
      ```bash
      mkdir backend && cd backend
      dotnet new webapi -n DataKendaraan.Api -f net8.0
      dotnet new sln -n DataKendaraan
      dotnet sln add DataKendaraan.Api/DataKendaraan.Api.csproj
      ```
- [ ] Tambah paket
      ```bash
      dotnet add DataKendaraan.Api package Microsoft.EntityFrameworkCore.Sqlite
      dotnet add DataKendaraan.Api package Microsoft.EntityFrameworkCore.Design
      dotnet tool install --global dotnet-ef   # jika belum ada
      ```
- [ ] Buat folder: `Models/`, `Data/`, `Dtos/`, `Controllers/`
- [ ] `.gitignore` (bin, obj, *.db, *.db-shm, *.db-wal)
- [ ] `appsettings.Development.json` → `"ConnectionStrings": { "Default": "Data Source=kendaraan.db" }`

**DoD M0:** `dotnet run` menyalakan server, `/swagger` terbuka.

---

## M1 — Data Layer

- [ ] `Models/Kendaraan.cs` — sesuai `docs/03-DATA-MODEL.md` §4
- [ ] `Data/AppDbContext.cs` — DbSet + OnModelCreating (PK, index nama_pemilik, CHECK warna & tahun)
- [ ] `Data/DbInitializer.cs` — seed 5 data dari §7, hanya jika tabel kosong
- [ ] `Dtos/KendaraanDto.cs` + `Dtos/KendaraanRequest.cs`
- [ ] Migration: `dotnet ef migrations add InitialCreate`
- [ ] `Program.cs`: register `AppDbContext` (SQLite), panggil `Migrate()` + `SeedAsync()` saat startup
- [ ] Cek: `kendaraan.db` terbentuk, 5 baris ter-seed

**DoD M1:** tabel `kendaraan` ada di DB dengan 5 baris seed dan constraint lengkap.

---

## M2 — REST API

Semua endpoint di bawah harus muncul di Swagger dan berfungsi lewat curl/Postman.

- [ ] `KendaraanController` (route `api/kendaraan`)
- [ ] `GET /api/kendaraan?noRegistrasi=&namaPemilik=`
      - filter contains + case-insensitive (`EF.Functions.Like` atau `ToLower().Contains()`)
      - kosong = semua data, urut `no_registrasi` ASC
      - response `200` + `KendaraanDto[]`
- [ ] `GET /api/kendaraan/{noRegistrasi}`
      - `200` + dto · `404` jika tidak ada
- [ ] `POST /api/kendaraan`
      - `201 Created` + `Location` header + dto
      - `400` validasi gagal · `409` duplikat PK
- [ ] `PUT /api/kendaraan/{oldNoRegistrasi}`
      - `200` + dto · `404` not found · `400` validasi · `409` jika newNoReg sudah dipakai record lain
- [ ] `DELETE /api/kendaraan/{noRegistrasi}`
      - `204 NoContent` · `404` not found
- [ ] Error response format seragam:
      ```json
      { "message": "Nomor registrasi sudah terdaftar.", "errors": { "noRegistrasi": ["..."] } }
      ```
- [ ] CORS policy `AllowFrontend` untuk origin `http://localhost:5500` (Live Server) & `http://127.0.0.1:5500`
- [ ] Trim whitespace pada `noRegistrasi` & `namaPemilik` sebelum simpan
- [ ] Normalisasi: `Warna` case-insensitive match ke opsi valid, atau tolak 400

**DoD M2:** Semua endpoint lulus smoke test manual (lihat M4 test cases).

---

## M3 — Frontend

### M3.1 Kerangka
- [ ] `frontend/css/app.css` — CSS variable warna (§1 DESIGN), style header, filter panel, tabel, empty-state
- [ ] `frontend/js/api.js` — `getList(filters)`, `getOne(id)`, `create(payload)`, `update(id, payload)`, `remove(id)`; base URL configurable (`const API_BASE = 'http://localhost:5000/api'`)
- [ ] `frontend/js/toast.js` — `showToast(message, type)`
- [ ] Skeleton `index.html` dengan header + container

### M3.2 Halaman Monitoring (`index.html` + `js/list.js`)
- [ ] Panel filter: 2 input + tombol Search & Add (§2 DESIGN)
- [ ] Render tabel 9 kolom dari `GET /api/kendaraan`
- [ ] Format: `No` = index, `Kapasitas` = `{n} cc`, null → `-`
- [ ] Submit filter → panggil API dengan query, render ulang, reset numbering
- [ ] Empty state saat 0 hasil
- [ ] Action link Detail/Edit/Delete per baris dengan warna sesuai DESIGN
- [ ] Modal konfirmasi delete: teks persis `Anda yakin menghapus data {noReg} ?`, tombol OK & Batal
- [ ] OK → `DELETE`, tutup modal, reload tabel, toast sukses; gagal → toast error
- [ ] Baca `?status=created|updated|deleted` saat load → tampilkan toast lalu bersihkan query string (`history.replaceState`)

### M3.3 Form Tambah/Edit/Detail
- [ ] Satu partial `form-kendaraan.html` (atau markup dibagi via JS template) dipakai 3 halaman
- [ ] Mode ditentukan `?mode=tambah|edit|detail` + `?id=`
- [ ] Layout 2 kolom sesuai urutan DESIGN §3
- [ ] Field Warna = `<select>` dengan opsi Merah/Hitam/Biru/Abu-Abu + placeholder "Pilih Warna"
- [ ] Field Bahan Bakar = `<input type="text">`
- [ ] `tambah.html`: form kosong, tombol **Simpan**
- [ ] `edit.html`: load data via `GET /{id}`, prefill, tombol **Ubah**
- [ ] `detail.html`: load data, semua field `disabled`, hanya tombol **Kembali**
- [ ] Validasi client-side: required (noReg, namaPemilik), pola tahun, kapasitas > 0, maxlength
- [ ] Pesan error per field di bawah input (`is-invalid` + `[data-error-for]`)
- [ ] Submit → POST/PUT → redirect `/index.html?status=...`; error server → tampilkan di field terkait
- [ ] Tombol Kembali → `/index.html`

### M3.4 Polish
- [ ] Responsif: tabel scroll horizontal < 992px, filter panel stack < 768px
- [ ] `aria-label` pada tombol ikon, `<label for>` di semua input
- [ ] Modal bisa ditutup Esc
- [ ] Tidak ada error di console browser

**DoD M3:** Keenam user story bisa dilakukan penuh lewat browser tanpa menyentuh Swagger.

---

## M4 — Testing

- [ ] Smoke test manual untuk 12 skenario di bawah
- [ ] Unit/integration test minimal untuk controller (xUnit + `WebApplicationFactory`):
      - [ ] POST valid → 201
      - [ ] POST duplikat → 409
      - [ ] POST tanpa namaPemilik → 400
      - [ ] PUT dengan PK baru yang sudah dipakai → 409
      - [ ] DELETE id tidak ada → 404
      - [ ] GET dengan filter nama (case-insensitive) → hasil sesuai

| # | Skenario | Ekspektasi |
|---|---|---|
| 1 | Buka Monitoring | 5 baris seed tampil |
| 2 | Search "b-" | semua baris B- tampil (case-insensitive) |
| 3 | Search "messi" | 1 baris Lionel Messi |
| 4 | Search "zzz" | pesan "Data kendaraan tidak ditemukan" |
| 5 | Add data lengkap | redirect + toast sukses, data muncul |
| 6 | Add tanpa Nama Pemilik | pesan wajib isi, tidak submit |
| 7 | Add dengan No Reg yang sudah ada | pesan "Nomor registrasi sudah terdaftar." |
| 8 | Edit tahun jadi "abc" | pesan tahun tidak valid |
| 9 | Detail | semua field disabled, tanpa tombol Ubah |
| 10 | Delete → Batal | modal tutup, data masih ada |
| 11 | Delete → OK | modal tutup, baris hilang, toast sukses |
| 12 | Restart server | data tetap ada (SQLite persisten) |

**DoD M4:** Semua 12 skenario lulus + minimal 6 test otomatis hijau.

---

## M5 — Delivery (opsional, jika ini dikumpulkan sebagai soal)

- [ ] `README.md`: cara run backend (`dotnet run`), frontend (Live Server / `python -m http.server`), endpoint API
- [ ] Salin PDF asli ke `docs/source/Application Data Kendaraan.pdf`
- [ ] Screenshot 4 layar (`docs/screenshots/`)
- [ ] Zona waktu & format tahun konsisten
- [ ] Cek tidak ada `bin/`, `obj/`, `*.db` di commit akhir

---

## Urutan Kerja yang Disarankan untuk AI Coding Agent

```
M0 → M1 → M2 (verifikasi dengan curl) → M3.1 → M3.2 → M3.3 → M3.4 → M4 → M5
```

**Aturan:** jangan lanjut milestone jika DoD milestone sebelumnya belum terpenuhi.
Jika ada dokumen yang belum jelas, baca `docs/07-DECISIONS.md` SEBELUM bertanya.

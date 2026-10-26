# TASKS — Aplikasi Data Kendaraan

Checklist implementasi. Kerjakan **berurutan per milestone**; setiap milestone
harus bisa di-demo end-to-end sebelum lanjut.

Legend: `[ ]` belum · `[x]` selesai · `[~]` sedang dikerjakan

---

## M0 — Bootstrap Project

- [x] Buat solution & project

      ```bash
      mkdir backend && cd backend
      dotnet new webapi -n DataKendaraan.Api -f net8.0
      dotnet new sln -n DataKendaraan
      dotnet sln add DataKendaraan.Api/DataKendaraan.Api.csproj
      ```

- [x] Tambah paket

      ```bash
      dotnet add DataKendaraan.Api package Microsoft.EntityFrameworkCore.Sqlite
      dotnet add DataKendaraan.Api package Microsoft.EntityFrameworkCore.Design
      dotnet tool install --global dotnet-ef
      ```

- [x] Buat folder: `Models/`, `Data/`, `Dtos/`, `Controllers/`
- [x] `.gitignore` (bin, obj, _.db, _.db-shm, \*.db-wal)
- [x] `appsettings.Development.json` → `"ConnectionStrings": { "Default": "Data Source=kendaraan.db" }`

**DoD M0:** `dotnet run` menyalakan server, `/swagger` terbuka.

---

## M1 — Data Layer

- [x] `Models/Kendaraan.cs` — sesuai `docs/03-DATA-MODEL.md` §4
- [x] `Data/AppDbContext.cs` — DbSet + OnModelCreating (PK, index nama_pemilik, CHECK warna & tahun)
- [x] `Data/DbInitializer.cs` — seed 5 data dari §7, hanya jika tabel kosong
- [x] `Dtos/KendaraanDto.cs` + `Dtos/KendaraanRequest.cs`
- [x] Migration: `dotnet ef migrations add InitialCreate`
- [x] `Program.cs`: register `AppDbContext` (SQLite di dev, PostgreSQL di prod), panggil `Migrate()` + `SeedAsync()` saat startup
- [x] Cek: `kendaraan.db` terbentuk, 5 baris ter-seed

**DoD M1:** tabel `kendaraan` ada di DB dengan 5 baris seed dan constraint lengkap.

---

## M2 — REST API

Semua endpoint di bawah harus muncul di Swagger dan berfungsi lewat curl/Postman.

- [x] `KendaraanController` (route `api/kendaraan`)
- [x] `GET /api/kendaraan?noRegistrasi=&namaPemilik=` - filter contains + case-insensitive (`EF.Functions.Like` atau `ToLower().Contains()`) - kosong = semua data, urut `no_registrasi` ASC - response `200` + `KendaraanDto[]`
- [x] `GET /api/kendaraan/{noRegistrasi}` - `200` + dto · `404` jika tidak ada
- [x] `POST /api/kendaraan` - `201 Created` + `Location` header + dto - `400` validasi gagal · `409` duplikat PK
- [x] `PUT /api/kendaraan/{oldNoRegistrasi}` - `200` + dto · `404` not found · `400` validasi · `409` jika newNoReg sudah dipakai record lain
- [x] `DELETE /api/kendaraan/{noRegistrasi}` - `204 NoContent` · `404` not found
- [x] Error response format seragam:

      ```json
      { "message": "Nomor registrasi sudah terdaftar.", "errors": { "noRegistrasi": ["..."] } }
      ```

- [x] CORS policy `AllowFrontend` untuk origin `http://localhost:5500` (Live Server) & `http://127.0.0.1:5500`
- [x] Trim whitespace pada `noRegistrasi` & `namaPemilik` sebelum simpan
- [x] Normalisasi: `Warna` case-insensitive match ke opsi valid, atau tolak 400

**DoD M2:** Semua endpoint lulus smoke test manual (lihat M4 test cases).

---

## M3 — Frontend

### M3.1 Kerangka

- [x] `frontend/css/app.css` — CSS variable warna (§1 DESIGN), style header, filter panel, tabel, empty-state
- [x] `frontend/js/api.js` — `getList(filters)`, `getOne(id)`, `create(payload)`, `update(id, payload)`, `remove(id)`; base URL configurable (`const API_BASE = 'http://localhost:5000/api'`)
- [x] `frontend/js/toast.js` — `showToast(message, type)`
- [x] Skeleton `index.html` dengan header + container

### M3.2 Halaman Monitoring (`index.html` + `js/list.js`)

- [x] Panel filter: 2 input + tombol Search & Add (§2 DESIGN)
- [x] Render tabel 9 kolom dari `GET /api/kendaraan`
- [x] Format: `No` = index, `Kapasitas` = `{n} cc`, null → `-`
- [x] Submit filter → panggil API dengan query, render ulang, reset numbering
- [x] Empty state saat 0 hasil
- [x] Action link Detail/Edit/Delete per baris dengan warna sesuai DESIGN
- [x] Modal konfirmasi delete: teks persis `Anda yakin menghapus data {noReg} ?`, tombol OK & Batal
- [x] OK → `DELETE`, tutup modal, reload tabel, toast sukses; gagal → toast error
- [x] Baca `?status=created|updated|deleted` saat load → tampilkan toast lalu bersihkan query string (`history.replaceState`)

### M3.3 Form Tambah/Edit/Detail

- [x] Satu partial `form-kendaraan.html` (atau markup dibagi via JS template) dipakai 3 halaman
- [x] Mode ditentukan `?mode=tambah|edit|detail` + `?id=`
- [x] Layout 2 kolom sesuai urutan DESIGN §3
- [x] Field Warna = `<select>` dengan opsi Merah/Hitam/Biru/Abu-Abu + placeholder "Pilih Warna"
- [x] Field Bahan Bakar = `<input type="text">`
- [x] `tambah.html`: form kosong, tombol **Simpan**
- [x] `edit.html`: load data via `GET /{id}`, prefill, tombol **Ubah**
- [x] `detail.html`: load data, semua field `disabled`, hanya tombol **Kembali**
- [x] Validasi client-side: required (noReg, namaPemilik), pola tahun, kapasitas > 0, maxlength
- [x] Pesan error per field di bawah input (`is-invalid` + `[data-error-for]`)
- [x] Submit → POST/PUT → redirect `/index.html?status=...`; error server → tampilkan di field terkait
- [x] Tombol Kembali → `/index.html`

### M3.4 Polish

- [x] Responsif: tabel scroll horizontal < 992px, filter panel stack < 768px
- [x] `aria-label` pada tombol ikon, `<label for>` di semua input
- [x] Modal bisa ditutup Esc
- [x] Tidak ada error di console browser

**Bukti verifikasi M3.4** (Playwright, Chromium):

| Kriteria | Hasil |
| --- | --- |
| 1366px | tabel tidak scroll, `docOverflowX: false`, panel 1152px |
| 900px | `tableScrollable: true`, `overflow-x: auto`, `docOverflowX: false` |
| 400px | `tableScrollable: true`, `.filter-actions` `flex-direction: column`, `docOverflowX: false` |
| `aria-label` aksi | `Detail data B-1678-BDG`, `Edit data B-1678-BDG`, `Hapus data B-1678-BDG` |
| `aria-label` modal | `role="dialog"`, `aria-modal="true"`, `aria-labelledby="deleteModalText"` |
| Label input | `index.html` 2/2, `form.html` 8/8 di ketiga mode — `inputTanpaLabel: []` |
| Esc | `keydown` diterima modal → `Modal.hide()` terpanggil → `show` class lepas, `display: none` |
| Console | 0 error di `index.html`, `form.html?mode=tambah`, `?mode=edit`, `?mode=detail` |

Catatan pengujian Esc: `page.keyboard.press('Escape')` Playwright tidak
menyampaikan event karena jendela browser headless kehilangan fokus OS
(`document.hasFocus() === false`, `visibilityState === "hidden"`). Verifikasi
dilakukan dengan `keydown` pada `document.activeElement` di dalam modal —
jalur kode yang sama dengan Esc sungguhan. Konfigurasi instance:
`{ backdrop: true, focus: true, keyboard: true }`.

**DoD M3:** Keenam user story bisa dilakukan penuh lewat browser tanpa menyentuh Swagger.

**Bukti DoD M3** (E2E Playwright, tanpa Swagger):

| # | User story | Hasil |
| --- | --- | --- |
| 1 | Lihat daftar | 5 baris seed tampil |
| 2 | Filter | `GET /api/kendaraan?noRegistrasi=...` terpanggil |
| 3 | Tambah | toast "Data kendaraan berhasil ditambahkan.", baris ke-6 muncul |
| 4 | Detail | 8 field `disabled`, hanya tombol Kembali |
| 5 | Edit | prefill benar, toast "Data kendaraan berhasil diperbarui.", baris ter-update |
| 6 | Hapus | modal konfirmasi → OK → toast, baris hilang, data kembali 5 seed |

---

## M4 — Testing

- [x] Smoke test manual untuk 12 skenario di bawah
- [x] Unit/integration test minimal untuk controller (xUnit + `WebApplicationFactory`): - [x] POST valid → 201 - [x] POST duplikat → 409 - [x] POST tanpa namaPemilik → 400 - [x] PUT dengan PK baru yang sudah dipakai → 409 - [x] DELETE id tidak ada → 404 - [x] GET dengan filter nama (case-insensitive) → hasil sesuai

| #   | Skenario                         | Ekspektasi                                |
| --- | -------------------------------- | ----------------------------------------- |
| 1   | Buka Monitoring                  | 5 baris seed tampil                       |
| 2   | Search "b-"                      | semua baris B- tampil (case-insensitive)  |
| 3   | Search "messi"                   | 1 baris Lionel Messi                      |
| 4   | Search "zzz"                     | pesan "Data kendaraan tidak ditemukan"    |
| 5   | Add data lengkap                 | redirect + toast sukses, data muncul      |
| 6   | Add tanpa Nama Pemilik           | pesan wajib isi, tidak submit             |
| 7   | Add dengan No Reg yang sudah ada | pesan "Nomor registrasi sudah terdaftar." |
| 8   | Edit tahun jadi "abc"            | pesan tahun tidak valid                   |
| 9   | Detail                           | semua field disabled, tanpa tombol Ubah   |
| 10  | Delete → Batal                   | modal tutup, data masih ada               |
| 11  | Delete → OK                      | modal tutup, baris hilang, toast sukses   |
| 12  | Restart server                   | data tetap ada (SQLite persisten)         |

**DoD M4:** Semua 12 skenario lulus + minimal 6 test otomatis hijau.

**Bukti DoD M4** (2026-10-25):

Test otomatis — `dotnet test` di `backend/`:

```
Passed! - Failed: 0, Passed: 6, Skipped: 0, Total: 6
```

| # | Test | Assert | Hasil |
| --- | --- | --- | --- |
| 1 | `POST` valid | `201` + `Location` + body sesuai | ✅ |
| 2 | `POST` duplikat `B-7763-TXY` | `409`, pesan "Nomor registrasi sudah terdaftar." | ✅ |
| 3 | `POST` tanpa `namaPemilik` | `400`, `errors.namaPemilik` terisi | ✅ |
| 4 | `PUT` PK baru sudah dipakai | `409` | ✅ |
| 5 | `DELETE` id tidak ada | `404`, pesan "Data kendaraan tidak ditemukan." | ✅ |
| 6 | `GET ?namaPemilik=messi` | `200`, 1 hasil, `Lionel Messi` | ✅ |

12 skenario manual (Playwright + `curl`):

| # | Skenario | Hasil terukur |
| --- | --- | --- |
| 1 | Buka Monitoring | 5 baris seed tampil |
| 2 | Search `b-` | 5 baris, case-insensitive |
| 3 | Search `messi` | 1 baris → `Lionel Messi` |
| 4 | Search `zzz` | 0 baris + "Data kendaraan tidak ditemukan." |
| 5 | Add data lengkap | redirect + toast "berhasil ditambahkan", baris muncul |
| 6 | Add tanpa Nama Pemilik | tetap di form, pesan "Nama Pemilik wajib diisi.", `is-invalid` |
| 7 | Add No Reg duplikat | tetap di form, pesan "Nomor registrasi sudah terdaftar." |
| 8 | Edit tahun tidak valid | UI: "Tahun Pembuatan harus antara 1900 dan 2026."; API `3000` → `400`; API `"abc"` → `400` |
| 9 | Detail | 8/8 field `disabled`, tanpa tombol submit/Ubah |
| 10 | Delete → Batal | modal tertutup, baris masih ada |
| 11 | Delete → OK | baris hilang, `GET` kembali 5 baris |
| 12 | Restart server | data uji `B-9999-PRS` tetap ada setelah restart (SQLite persisten) |

Catatan skenario 8: input `type="number"` membuat browser menolak huruf `abc`
sebelum submit (`The specified value "abc" cannot be parsed`), sehingga
validasi tahun diuji lewat dua jalur tambahan — nilai `1800` di UI dan
`PUT` langsung dengan `3000` serta `"abc"`. Ketiganya ditolak dengan benar.

Catatan isolasi test: MD5 `kendaraan.db` sebelum dan sesudah `dotnet test`
identik (`dbc5cff0…`), membuktikan test memakai SQLite in-memory dan tidak
menyentuh database development.

---

## M5 — Delivery (opsional, jika ini dikumpulkan sebagai soal)

- [x] `README.md`: cara run backend (`dotnet run`), frontend (Live Server / `python -m http.server`), endpoint API
- [x] Salin PDF asli ke `docs/source/Application Data Kendaraan.pdf`
- [ ] Screenshot 4 layar (`docs/screenshots/`)
- [x] Zona waktu & format tahun konsisten
- [x] Cek tidak ada `bin/`, `obj/`, `*.db` di commit akhir

**Bukti M5** (2026-10-25):

| Butir | Hasil |
| --- | --- |
| README | Ditulis ulang: prasyarat, cara run backend/frontend, 3 alternatif server, daftar endpoint + contoh `curl`, bentuk error, struktur folder, tabel 5 data seed |
| PDF sumber | `docs/source/Application Data Kendaraan.pdf` ada (739.960 byte) |
| Screenshot | **Belum** — diambil manual oleh user |
| Konsistensi | `cc` hanya di tabel (`list.js:32`), form tanpa `cc`; tahun tampil apa adanya; 8 pesan validasi semuanya diakhiri titik |
| Artefak Git | `git ls-files` bersih — tidak ada `bin/`, `obj/`, `*.db`, `node_modules` |

Catatan verifikasi perintah README: `npx http-server . -p 5500 -c-1` diuji dan
melayani `index.html`, `form.html`, `css/app.css`, `js/list.js` dengan `200`.
Saat pengujian port 5500 sedang dipakai Live Server (PID `Code.exe`), sehingga
uji dijalankan di port lain (5511) dengan hasil sama. `dotnet tool restore`
juga diuji dan berhasil memasang `dotnet-ef 8.0.32`.

---

## Urutan Kerja yang Disarankan untuk AI Coding Agent

```
M0 → M1 → M2 (verifikasi dengan curl) → M3.1 → M3.2 → M3.3 → M3.4 → M4 → M5
```

**Aturan:** jangan lanjut milestone jika DoD milestone sebelumnya belum terpenuhi.
Jika ada dokumen yang belum jelas, baca `05-BUSINESS-RULES.md` dan `01-PRD.md`
SEBELUM bertanya.

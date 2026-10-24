# DESIGN — Aplikasi Data Kendaraan

Spesifikasi visual & interaksi. Wireframe diambil dari mockup PDF asli.
Sudah cukup detail untuk langsung diimplementasikan tanpa bertanya lagi.

---

## 1. Layout Global

Semua halaman memakai satu shell yang sama:

```
┌─────────────────────────────────────────────────────────┐
│  [■] Aplikasi Data Kendaraan                            │  ← header
├─────────────────────────────────────────────────────────┤
│                                                         │
│  ┌─── Kartu panel (background krem) ──────────────────┐ │
│  │  ... isi halaman ...                               │ │
│  └────────────────────────────────────────────────────┘ │
│                                                         │
└─────────────────────────────────────────────────────────┘
```

- **Header**: tinggi 64px, background putih, ikon persegi 28×28px (`#4A90D9`,
  border-radius 4px) + judul `Aplikasi Data Kendaraan` bold 20px, `#1a1a1a`.
  Header di kiri atas, tidak full-width-bar (mengikuti mockup: ikon & judul
  menempel kiri, bukan di tengah).
- **Container konten**: `max-width: 1200px`, `margin: 0 auto`, padding 24px.
- **Warna brand**
  | Token | Hex | Pemakaian |
  |---|---|---|
  | `--primary` | `#2F6FB5` | tombol utama, header tabel |
  | `--primary-hover` | `#255A94` | hover tombol |
  | `--panel-bg` | `#FBEBDD` | panel filter (krem) |
  | `--secondary` | `#9AA0A6` | tombol Kembali |
  | `--danger` | `#D9363E` | tautan Delete |
  | `--warning` | `#E08A2B` | tautan Detail |
  | `--text` | `#1a1a1a` | teks utama |
  | `--muted` | `#6B7280` | placeholder / label sekunder |
  | `--border` | `#D6D6D6` | border input & card |
  | `--success` | `#1E874B` | notifikasi sukses |
  | `--error` | `#D9363E` | pesan validasi |

## 2. Halaman Monitoring (F1, F2, F6)

```
┌─────────────────────────────────────────────────────────────┐
│ [■] Aplikasi Data Kendaraan                                 │
│                                                             │
│  ┌── panel filter (bg #FBEBDD, padding 16px) ────────────┐  │
│  │ No Registrasi                     [Search]  [Add]     │  │
│  │ [_________________________]                            │  │
│  │ Nama Pemilik                                           │  │
│  │ [_________________________]                            │  │
│  └────────────────────────────────────────────────────────┘  │
│                                                             │
│  ┌────────┬──────────────┬───────────┬───────┬───┬────┬──┐  │
│  │ No │ Reg │ Nama Pemilik │ Merk │ Thn │ Kap │ … │Action│ │
│  ├────┼─────┼──────────────┼──────┼─────┼─────┼───┼──────┤ │
│  │ 1  │B-…  │ Lionel Messi │Honda…│2018 │150cc│ … │Detail│ │
│  │    │     │              │      │     │     │   │Edit  │ │
│  │    │     │              │      │     │     │   │Delete│ │
│  └────┴─────┴──────────────┴──────┴─────┴─────┴───┴──────┘ │
└─────────────────────────────────────────────────────────────┘
```

### Struktur markup (Bootstrap 5)

```html
<div class="app-header">
  <span class="app-logo"></span>
  <h1 class="app-title">Aplikasi Data Kendaraan</h1>
</div>

<div class="container">
  <form id="filterForm" class="filter-panel">
    <div class="filter-fields">
      <div class="mb-3">
        <label for="filterNoRegistrasi" class="form-label">No Registrasi</label>
        <input type="text" class="form-control" id="filterNoRegistrasi" name="noRegistrasi" maxlength="20">
      </div>
      <div class="mb-3">
        <label for="filterNamaPemilik" class="form-label">Nama Pemilik</label>
        <input type="text" class="form-control" id="filterNamaPemilik" name="namaPemilik" maxlength="100">
      </div>
    </div>
    <div class="filter-actions">
      <button type="submit" class="btn btn-primary" id="btnSearch">Search</button>
      <a href="/tambah.html" class="btn btn-primary" id="btnAdd">Add</a>
    </div>
  </form>

  <div class="table-responsive">
    <table class="table table-bordered align-middle" id="vehicleTable">
      <thead>
        <tr>
          <th>No</th><th>No Registrasi</th><th>Nama Pemilik</th><th>Merk Kendaraan</th>
          <th>Tahun Pembuatan</th><th>Kapasitas</th><th>Warna</th><th>Bahan Bakar</th><th>Action</th>
        </tr>
      </thead>
      <tbody id="vehicleTableBody"></tbody>
    </table>
  </div>
  <div id="emptyState" class="empty-state d-none">Data kendaraan tidak ditemukan</div>
</div>
```

### Gaya tabel
- `thead th`: background `--primary`, teks putih, bold, `text-align: left`, padding 10px 12px.
- `tbody tr:nth-child(even)`: `#F7F9FC`.
- `tbody tr:hover`: `#EDF3FB`.
- Kolom `No` dan `Kapasitas` rata kanan; sisanya rata kiri.
- Kolom `Action`: tiga link inline, dipisah spasi 12px.
  - `Detail` → `--warning`, `Edit` → `--primary`, `Delete` → `--danger`, semua `cursor:pointer`, tanpa underline default (underline saat hover).

### Panel filter
- Background `--panel-bg`, border-radius 6px, padding 16px 20px, margin-bottom 20px.
- Input dan tombol disusun: input menumpuk vertikal di kiri (maks 360px),
  tombol `Search` + `Add` sejajar di kanan bawah panel (sticky ke kanan).
- Di lebar < 768px, tombol pindah ke bawah input full-width.

## 3. Halaman Tambah Data (F2)

```
┌─────────────────────────────────────────────────────────┐
│ [■] Aplikasi Data Kendaraan                             │
│                                                         │
│  Tambah Data Kendaraan                                  │  ← h2
│                                                         │
│  No. Registrasi Kendaraan *      Tahun Pembuatan        │
│  [____________________]         [_______]               │
│  Nama Pemilik *                  Kapasitas Silinder     │
│  [____________________]         [_______]               │
│  Merk Kendaraan                  Warna Kendaraan        │
│  [____________________]         [Pilih Warna       ▾]   │
│  Alamat Pemilik Kendaraan        Bahan Bakar            │
│  [____________________]         [____________________]  │
│  [____________________]                                 │
│                                                         │
│  [Simpan]  [Kembali]                                    │
└─────────────────────────────────────────────────────────┘
```

### Spesifikasi form
- Layout: `row` + `col-md-6` per field, urutan DOM **Kiri lalu Kanan**
  (row1: NoReg, Tahun → row2: Nama, Kapasitas → row3: Merk, Warna →
  row4: Alamat, Bahan Bakar). Ini sama dengan urutan visual mockup.
- **Alamat**: `textarea rows="4"`. Kolom kanan di baris 4 hanya berisi
  Bahan Bakar dan bawahnya kosong (natural, tidak perlu di-stretch).
- Semua input `class="form-control"` (textarea juga), tinggi default ~38px.
- Label di atas input, 14px, `font-weight: 500`.
- Penanda wajib: `<span class="text-danger">*</span>` setelah label.
- Tombol: `<button type="submit" class="btn btn-primary px-4">Simpan</button>`
  dan `<a href="/index.html" class="btn btn-secondary px-4">Kembali</a>`,
  dibungkus `<div class="d-flex gap-2 mt-4">`.

### Markup field (template per field)

```html
<div class="col-md-6 mb-3">
  <label for="noRegistrasi" class="form-label">
    No. Registrasi Kendaraan <span class="text-danger">*</span>
  </label>
  <input type="text" class="form-control" id="noRegistrasi" name="noRegistrasi"
         maxlength="20" required autocomplete="off">
  <div class="form-text text-danger d-none" data-error-for="noRegistrasi"></div>
</div>
```

```html
<div class="col-md-6 mb-3">
  <label for="warna" class="form-label">Warna Kendaraan</label>
  <select class="form-select" id="warna" name="warna">
    <option value="">Pilih Warna</option>
    <option value="Merah">Merah</option>
    <option value="Hitam">Hitam</option>
    <option value="Biru">Biru</option>
    <option value="Abu-Abu">Abu-Abu</option>
  </select>
</div>
```

## 4. Halaman Edit Data (F3) & Detail Data (F4)

Struktur **identik** dengan halaman Tambah. Perbedaannya:

| Aspek | Tambah | Edit | Detail |
|---|---|---|---|
| Judul `h2` | Tambah Data Kendaraan | Edit Data Kendaraan | Detail Data Kendaraan |
| URL | `/tambah.html` | `/edit.html?id={noReg}` | `/detail.html?id={noReg}` |
| Field | kosong | terisi | terisi, **disabled** |
| Tombol primary | **Simpan** | **Ubah** | — |
| Tombol secondary | Kembali | Kembali | Kembali |
| No. Registrasi | editable | editable (update PK, lihat BR-6.3) | disabled |

> Satu partial HTML `form-kendaraan.html` boleh dipakai ulang oleh ketiga
> halaman; mode dikontrol lewat query string `?mode=tambah|edit|detail`.
> Ini menghindari duplikasi markup 3×.

Gaya detail read-only:
- Input `disabled` **plus** class `.form-control-plaintext` atau
  `style="background:#F5F6F8; color:var(--text)"` agar teks tetap hitam,
  bukan abu pucat, dan `cursor: not-allowed`.

## 5. Modal Konfirmasi Hapus (F5)

```
        ┌────────────────────────────────────────┐
        │ Anda yakin menghapus data B-7763-TXY ? │
        │                                        │
        │                        [OK]   [Batal]  │
        └────────────────────────────────────────┘
```

- Bootstrap modal, `id="deleteModal"`, `modal-dialog-centered`,
  ukuran `modal-dialog modal-dialog-centered` (lebar ~420px).
- Body: `<p id="deleteModalText">Anda yakin menghapus data <strong>{noReg}</strong> ?</p>`
  — **tanpa** header/modal-title, **tanpa** ikon warning, sesuai mockup.
- Footer: `OK` = `btn btn-primary`, `Batal` = `btn btn-secondary`,
  keduanya rata kanan (`justify-content-end`).
- `Batal` dan tombol Esc menutup tanpa aksi. `OK` memicu `DELETE /api/kendaraan/{noReg}`.
- Selama request berjalan, tombol OK disabled + teksnya "Menghapus…".

## 6. Notifikasi (Toast)

- Posisi: kanan atas, lebar ~320px, auto-hilang 4 detik, ada tombol ×.
- Sukses: background `--success` 10% + border kiri 4px `--success`, teks hijau gelap.
- Error: background `--error` 10% + border kiri 4px `--error`, teks merah.
- Sumber data: query param `?status=created|updated|deleted` setelah redirect,
  atau langsung dari handler JS untuk aksi di halaman yang sama.

## 7. Pesan Validasi (Bahasa Indonesia)

| Kondisi | Pesan |
|---|---|
| No Registrasi kosong | No. Registrasi Kendaraan wajib diisi. |
| Nama Pemilik kosong | Nama Pemilik wajib diisi. |
| No Registrasi duplikat (409) | Nomor registrasi sudah terdaftar. |
| Tahun bukan angka / <1900 / >tahun berjalan | Tahun Pembuatan harus berupa angka antara 1900 dan {tahunIni}. |
| Kapasitas bukan angka / ≤ 0 | Kapasitas Silinder harus berupa angka lebih dari 0. |
| Alamat > 255 char | Alamat maksimal 255 karakter. |
| Field lain > batas char | {Label} maksimal {n} karakter. |
| Gagal koneksi ke server | Tidak dapat terhubung ke server. Silakan coba lagi. |
| Error tak terduga | Terjadi kesalahan. Silakan coba lagi. |

Aturan tampilan: pesan muncul di bawah field sebagai teks merah 13px,
input bersangkutan diberi `class="is-invalid"` (border merah).
Validasi dijalankan saat blur dan saat submit; pesan hilang saat user mengetik.

## 8. State & Interaksi Tabel

| Event | Perilaku |
|---|---|
| Halaman load | `GET /api/kendaraan` tanpa filter → render tabel |
| Klik Search | `GET /api/kendaraan?noRegistrasi=..&namaPemilik=..` → render ulang |
| Enter di input filter | sama dengan klik Search (form submit) |
| 0 hasil | sembunyikan tabel, tampilkan `#emptyState` |
| Klik Delete | isi `#deleteModalText` dengan noReg baris → `modal.show()` |
| Klik Detail / Edit | `window.location = /detail.html?id=..` / `/edit.html?id=..` |
| Request gagal | toast error, tabel tetap seperti sebelumnya |

## 9. Format Tampilan Data Tabel

| Kolom | Sumber | Format |
|---|---|---|
| No | index | `i + 1` |
| No Registrasi | `noRegistrasi` | apa adanya, `font-weight:600` |
| Nama Pemilik | `namaPemilik` | apa adanya |
| Merk Kendaraan | `merkKendaraan` | apa adanya |
| Tahun Pembuatan | `tahunPembuatan` | apa adanya (4 digit) |
| Kapasitas | `kapasitasSilinder` | `{n} cc` |
| Warna | `warna` | apa adanya |
| Bahan Bakar | `bahanBakar` | apa adanya |
| Action | — | 3 link |

Nilai kosong/null ditampilkan sebagai `-` (dash), bukan "null" atau "undefined".

## 10. File Aset & Halaman

```
frontend/
├── index.html          Monitoring
├── tambah.html         Tambah
├── edit.html           Edit
├── detail.html         Detail
├── css/app.css         Style custom (token warna, header, filter panel, tabel)
└── js/
    ├── api.js          Wrapper fetch ke /api/*
    ├── list.js         Logika halaman Monitoring (render tabel, filter, delete modal)
    ├── form.js         Logika form Tambah/Edit/Detail (load, validate, submit)
    └── toast.js        Helper notifikasi
```

Bootstrap 5 & jQuery boleh via CDN, atau di-vendor ke `frontend/vendor/`
jika aplikasi harus jalan offline. **Default: CDN.**

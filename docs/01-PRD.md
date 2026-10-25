# PRD — Aplikasi Data Kendaraan

**Versi:** 1.0 · **Status:** Approved for development
**Sumber:** `Application Data Kendaraan.pdf` (soal test)
**Backend:** C# ASP.NET Core Web API + EF Core
**Frontend:** HTML + CSS + JavaScript (jQuery, Bootstrap 5)
**Database:** SQLite (file) untuk development, **PostgreSQL untuk production**

---

## 1. Latar Belakang & Tujuan

Dibutuhkan aplikasi web sederhana untuk mencatat dan mengelola data kendaraan
(roda dua/roda empat). Aplikasi harus mendukung operasi CRUD penuh dengan
tampilan tabel yang bisa difilter, form tambah/ubah, halaman detail read-only,
dan konfirmasi sebelum menghapus data.

**Tujuan terukur:**

- Pengguna dapat menambah, mencari, melihat, mengubah, dan menghapus data kendaraan.
- Nomor registrasi kendaraan bersifat unik dan menjadi identitas utama data.
- Tidak ada data terhapus tanpa konfirmasi eksplisit.

## 2. Ruang Lingkup

### In Scope

| #   | Fitur                                | Layar       |
| --- | ------------------------------------ | ----------- |
| F1  | Daftar data kendaraan + pencarian    | Monitoring  |
| F2  | Tambah data kendaraan                | Tambah Data |
| F3  | Ubah data kendaraan                  | Edit Data   |
| F4  | Lihat detail (read-only)             | Detail Data |
| F5  | Hapus data + modal konfirmasi        | Monitoring  |
| F6  | Validasi input & pesan error         | Semua form  |
| F7  | REST API untuk semua operasi di atas | —           |

### Out of Scope

- Login / autentikasi / otorisasi per-role
- Upload foto kendaraan
- Export Excel/PDF
- Pagination server-side (data diasumsikan < 500 baris; pagination client-side opsional)
- Audit trail / soft delete
- Multi-bahasa (UI cukup Bahasa Indonesia)

## 3. Persona

**Petugas Administrasi** — memasukkan data kendaraan baru dari berkas fisik,
mencari data lama saat pemilik datang, dan mengoreksi data yang salah ketik.
Tidak paham teknis; butuh form yang jelas dan pesan error berbahasa Indonesia.

## 4. User Stories & Acceptance Criteria

### US-1 — Melihat daftar kendaraan

> Sebagai petugas, saya ingin melihat semua data kendaraan dalam tabel agar
> saya tahu data apa saja yang sudah tercatat.

**AC:**

- [ ] Halaman Monitoring menampilkan tabel dengan kolom berurutan:
      `No | No Registrasi | Nama Pemilik | Merk Kendaraan | Tahun Pembuatan | Kapasitas | Warna | Bahan Bakar | Action`
- [ ] Kolom **No** adalah nomor urut tampilan (1,2,3…), bukan PK.
- [ ] Kolom **Kapasitas** ditampilkan dengan sufiks ` cc` (contoh `150 cc`).
- [ ] Kolom **Action** berisi tiga tautan: `Detail` (oranye), `Edit` (biru), `Delete` (merah).
- [ ] Jika tidak ada data, tampilkan baris kosong dengan teks "Data kendaraan tidak ditemukan".
- [ ] Di atas tabel ada dua input filter: **No Registrasi** dan **Nama Pemilik**, serta tombol **Search** dan **Add**.
- [ ] Judul halaman "Aplikasi Data Kendaraan" + ikon persegi biru di kiri atas.
- [ ] Kolom **Alamat** sengaja TIDAK ditampilkan di tabel (hanya di form/detail).

### US-2 — Mencari data kendaraan

> Sebagai petugas, saya ingin mencari kendaraan berdasarkan nomor registrasi
> atau nama pemilik agar tidak perlu scroll manual.

**AC:**

- [ ] Pencarian dijalankan saat tombol **Search** diklik (bukan live search).
- [ ] Filter **No Registrasi**: pencocokan _contains_, case-insensitive (`b-7763` menemukan `B-7763-TXY`).
- [ ] Filter **Nama Pemilik**: pencocokan _contains_, case-insensitive (`messi` menemukan `Lionel Messi`).
- [ ] Kedua filter dapat dikombinasikan dengan logika **AND**.
- [ ] Filter kosong = tampilkan semua data.
- [ ] Tombol Search dengan kedua filter kosong = reset / tampilkan semua data.
- [ ] Jika hasil kosong, tampilkan pesan "Data kendaraan tidak ditemukan".
- [ ] Kolom nomor urut (`No`) di-reset mulai dari 1 pada hasil filter.

### US-3 — Menambah data kendaraan

> Sebagai petugas, saya ingin menambah data kendaraan baru agar tercatat di sistem.

**AC:**

- [ ] Tombol **Add** membuka halaman Tambah Data Kendaraan.
- [ ] Form berisi 8 field dalam layout 2 kolom:
      | Kiri | Kanan |
      |---|---|
      | No. Registrasi Kendaraan | Tahun Pembuatan |
      | Nama Pemilik | Kapasitas Silinder |
      | Merk Kendaraan | Warna Kendaraan |
      | Alamat Pemilik Kendaraan (textarea) | Bahan Bakar |
- [ ] Tombol aksi di kiri bawah: **Simpan** (biru, primary) lalu **Kembali** (abu-abu, secondary).
- [ ] Semua field yang mandatory ditandai `*` merah pada label.
- [ ] Setelah Simpan berhasil → redirect ke halaman Monitoring dan tampilkan
      notifikasi sukses "Data kendaraan berhasil disimpan."
- [ ] **Kembali** membatalkan input dan kembali ke Monitoring tanpa menyimpan.
- [ ] Jika user menekan Kembali padahal form sudah diisi, tampilkan konfirmasi
      "Data yang belum disimpan akan hilang. Lanjutkan?" (opsional, nice-to-have).

### US-4 — Mengubah data kendaraan

> Sebagai petugas, saya ingin mengubah data kendaraan yang salah agar data tetap akurat.

**AC:**

- [ ] Tautan **Edit** di tabel membuka form Edit Data Kendaraan, terisi data yang ada.
- [ ] Semua field dapat diubah, termasuk **No Registrasi** (karena ini PK, lihat Business Rules §6.3).
- [ ] Tombol aksi: **Ubah** (biru, primary) lalu **Kembali** (abu-abu, secondary).
- [ ] Setelah Ubah berhasil → redirect ke Monitoring + notifikasi
      "Data kendaraan berhasil diubah."
- [ ] Validasi sama persis dengan form Tambah.

### US-5 — Melihat detail data kendaraan

> Sebagai petugas, saya ingin melihat detail data kendaraan secara lengkap
> (termasuk alamat) tanpa risiko tidak sengaja mengubahnya.

**AC:**

- [ ] Tautan **Detail** membuka form yang sama dengan mode Edit,
      **tetapi semua field disabled/readonly**.
- [ ] Tidak ada tombol **Ubah**; satu-satunya tombol adalah **Kembali**.
- [ ] Field readonly tetap terbaca jelas (teks tidak abu-abu terlalu pucat).
- [ ] Judul halaman: "Detail Data Kendaraan".

### US-6 — Menghapus data kendaraan

> Sebagai petugas, saya ingin menghapus data kendaraan yang salah masuk
> agar tabel tetap bersih, dengan konfirmasi agar tidak salah hapus.

**AC:**

- [ ] Tautan **Delete** membuka modal konfirmasi di atas halaman Monitoring.
- [ ] Teks modal persis: **"Anda yakin menghapus data {No Registrasi} ?"**
      contoh: `Anda yakin menghapus data B-7763-TXY ?`
- [ ] Tombol modal: **OK** (biru) di kiri, **Batal** (abu-abu) di kanan.
- [ ] **Batal** menutup modal tanpa menghapus apa pun.
- [ ] **OK** menghapus data dari server, menutup modal, me-reload tabel, dan
      menampilkan notifikasi "Data kendaraan berhasil dihapus."
- [ ] Jika penghapusan gagal (mis. data sudah tidak ada), tampilkan error dan reload tabel.
- [ ] Tidak ada checkbox/bulk delete.

## 5. Field Specification

| #   | Field                      | Label UI                 | Tipe Input              | Wajib    | Batasan                              |
| --- | -------------------------- | ------------------------ | ----------------------- | -------- | ------------------------------------ |
| 1   | Nomor Registrasi Kendaraan | No. Registrasi Kendaraan | `<input type="text">`   | ✅ Ya    | PK, unik, maks 20 char               |
| 2   | Nama Pemilik               | Nama Pemilik             | `<input type="text">`   | ✅ Ya    | maks 100 char                        |
| 3   | Alamat                     | Alamat Pemilik Kendaraan | `<textarea rows=4>`     | ❌ Tidak | maks 255 char                        |
| 4   | Merk Kendaraan             | Merk Kendaraan           | `<input type="text">`   | ❌ Tidak | maks 100 char                        |
| 5   | Tahun Pembuatan            | Tahun Pembuatan          | `<input type="number">` | ❌ Tidak | 4 digit, rentang 1900–tahun berjalan |
| 6   | Kapasitas Silinder         | Kapasitas Silinder       | `<input type="number">` | ❌ Tidak | bilangan bulat > 0                   |
| 7   | Warna Kendaraan            | Warna Kendaraan          | `<select>`              | ❌ Tidak | opsi: `Merah, Hitam, Biru, Abu-Abu`  |
| 8   | Bahan Bakar                | Bahan Bakar              | `<input type="text">`   | ❌ Tidak | maks 50 char                         |

> **Catatan keputusan:** Warna = dropdown dan Bahan Bakar = text input mengikuti
> mockup, bukan tabel requirement yang keliru (tabel itu menukar keduanya).

## 6. Non-Functional Requirements

| Aspek             | Target                                                          |
| ----------------- | --------------------------------------------------------------- |
| Browser           | Chrome/Edge/Firefox versi 2 tahun terakhir                      |
| Responsif         | Desktop-first; tabel scroll horizontal di lebar < 992px         |
| Bahasa UI         | Bahasa Indonesia, termasuk pesan validasi                       |
| Waktu muat daftar | < 1 detik untuk 500 baris                                       |
| Aksesibilitas     | Setiap input punya `<label for>`; modal bisa ditutup dengan Esc |
| Konvensi kode     | Lihat `docs/../.editorconfig` & standar di PRD ini              |

## 7. Definisi Selesai (DoD)

Sebuah fitur dianggap selesai jika:

1. Acceptance criteria di atas terpenuhi semua.
2. Berjalan end-to-end (FE → API → DB) tanpa error di console.
3. Validasi client-side **dan** server-side aktif.
4. `docs/04-TASKS.md` menandai task terkait sebagai `[x]`.

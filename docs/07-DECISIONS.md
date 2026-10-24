# DECISIONS — Ambiguitas & Keputusan

Dokumen sumber (`Application Data Kendaraan.pdf`) punya beberapa inkonsistensi.
Semua sudah diputuskan di bawah. **AI coding agent: jangan bertanya ulang soal ini.**

---

## D-01 — "Bahan Bakar" dropdown berisi nama warna

**Masalah:** Tabel requirement menulis `Bahan Bakar | Dropdown [Merah, Hitam, Biru, Abu-Abu]`,
tetapi empat nilai itu adalah nama **warna**, bukan bahan bakar. Mockup form dan
data contoh justru memakai **"Bensin"** pada field Bahan Bakar dan
**warna** (Merah/Hitam/Biru) pada field Warna.

**Keputusan (dikonfirmasi user):** Ikuti mockup.

- `Warna Kendaraan` = **dropdown** `Merah / Hitam / Biru / Abu-Abu`
- `Bahan Bakar` = **text input** bebas (data contoh: "Bensin")

**Alasan:** Tabel requirement jelas tertukar (typo). Mockup visual + 5 baris data
contoh konsisten satu sama lain: warna adalah nilai enumerasi, bahan bakar bukan.

**Dampak:** CHECK constraint DB hanya pada `warna`; `bahan_bakar` VARCHAR(50) bebas.

---

## D-02 — Warna Kendaraan: "Text" vs dropdown di mockup

**Masalah:** Tabel requirement menyebut Warna = `Text`, mockup form menampilkan
dropdown "Pilih Warna".

**Keputusan:** Pakai **dropdown** dengan 4 opsi tetap.

**Alasan:** Dropdown mencegah salah ketik, dan set nilai yang sama sudah
didefinisikan dokumen. Ini keputusan yang diambil bersama D-01.

---

## D-03 — "Maxlength (4)" menempel ke Kapasitas Silinder

**Masalah:** Ekstraksi tabel PDF menempatkan `Numeric Maxlength (4)` sejajar
dengan Kapasitas Silinder, seolah kapasitas juga maxlength 4.

**Keputusan:** `Maxlength (4)` hanya berlaku untuk **Tahun Pembuatan**.
Kapasitas Silinder = bilangan bulat > 0, tanpa batas 4 digit.

**Alasan:** Kapasitas silinder realistis bisa 1000–1300 cc (motor besar) atau

> 4000 cc (mobil). Membatasi 4 digit justru memotong nilai yang sah. Tahun
> pembuatan memang selalu 4 digit. Tabel PDF meleset satu baris saat ekstraksi.

---

## D-04 — Field wajib (mandatory)

**Masalah:** Hanya No Registrasi ("Primary Key Mandatory") dan Nama Pemilik
("Mandatory") yang ditandai.

**Keputusan:** Hanya **No Registrasi** dan **Nama Pemilik** yang wajib.
Enam field lainnya opsional (nullable di DB).

**Alasan:** Mengikuti dokumen secara literal; belum ada alasan bisnis untuk
mewajibkan alamat/merk/tahun.

---

## D-05 — Nomor Registrasi sebagai PK yang bisa diedit

**Masalah:** PK natural yang juga field form. Mockup Edit menampilkan
"No. Registrasi Kendaraan" sebagai input yang terisi — tidak dijelaskan
apakah boleh diubah.

**Keputusan:** **Boleh diubah.** Endpoint `PUT /api/kendaraan/{oldNoRegistrasi}`
menerima `noRegistrasi` baru dalam body dan melakukan update PK.

**Aturan:**

- Jika `newNoReg == oldNoReg` → update biasa.
- Jika `newNoReg` sudah dipakai record lain → `409 Conflict`.
- Frontend form Edit mengirim `PUT /api/kendaraan/B-7763-TXY` dengan body `{"noRegistrasi":"B-9999-XXX", ...}`.
- Request tanpa perubahan = idempoten, tetap `200`.

---

## D-06 — Pencarian: server-side atau client-side?

**Keputusan:** **Server-side** lewat query string `?noRegistrasi=&namaPemilik=`
dan `LIKE %x%` case-insensitive.

**Alasan:** Sesuai arsitektur REST + DB; data bisa bertambah melewati batas
yang nyaman di-load sekaligus.

---

## D-07 — Pagination

**Keputusan:** **Tidak ada pagination.** Seluruh hasil filter dikirim sekaligus.

**Alasan:** Tidak ada indikasi pagination di mockup manapun. Skala data soal
kecil. Jika nanti dibutuhkan, tambahkan `?page=&pageSize=` di endpoint list
tanpa mengubah kontrak lain.

---

## D-08 — Konfirmasi delete: modal vs `confirm()`

**Keputusan:** **Bootstrap modal** custom, dengan teks
`Anda yakin menghapus data {noReg} ?` dan tombol `OK` / `Batal`.
**Bukan** `window.confirm()`, karena teks tombolnya harus "OK"/"Batal"
(`window.confirm` hanya memberi OK/Cancel, dan di browser berbahasa Inggris).

---

## D-09 — Kapasitas ditampilkan dengan "cc"

**Keputusan:** Nilai disimpan sebagai `INTEGER` (angka saja, contoh `150`),
tetapi **ditampilkan** dengan sufiks ` cc` di tabel. Form input tanpa sufiks.

**Alasan:** Mockup tabel menampilkan "150 cc"; menyimpan sufiks di DB akan
merusak tipe Numeric.

---

## D-10 — Redirect setelah simpan/ubah

**Keputusan:** Redirect ke `/index.html` dengan query `?status=created|updated|deleted`
yang langsung dibaca untuk menampilkan toast lalu dibersihkan dari URL.

**Alasan:** Pola standar POST-redirect-GET; menghindari re-submit saat refresh.

---

## D-11 — Stack pilihan

**Masalah:** Dokumen memberi dua opsi backend: Java (Spring & Hibernate) **atau** C# (ASP.NET).

**Keputusan (dikonfirmasi user):** **C# ASP.NET Core Web API (.NET 8) + EF Core 8**,
DB **SQLite untuk development** dan **PostgreSQL untuk production**.

**Alternatif jika nanti diminta Java:** struktur docs tetap valid; ganti Entity
→ `@Entity` JPA, DTO → record Java, controller → `@RestController`,
migrasi → Flyway/Liquibase. Kontrak API (path, method, JSON) **tidak berubah**,
sehingga frontend tidak perlu disentuh.

---

## D-12 — Nama field di JSON

**Keputusan:** camelCase (`noRegistrasi`, `namaPemilik`, `tahunPembuatan`, …).

**Alasan:** Default ASP.NET Core `System.Text.Json`. Frontend mengikuti kontrak
ini; lihat tabel mapping di `docs/03-DATA-MODEL.md` §6.

---

## D-13 — Identitas visual

**Keputusan:** Ikon persegi `#4A90D9` 28×28px dengan border-radius 4px sebagai
logo placeholder. Tidak ada aset logo dari dokumen.

**Alasan:** Mockup hanya menampilkan kotak biru polos; jangan menebak-nebak
merek perusahaan.

---

## D-14 — Alamat tidak muncul di tabel

**Masalah:** Field Alamat ada di requirement tapi tidak terlihat di grid mockup.

**Keputusan:** Alamat **tidak** ditampilkan di tabel Monitoring; hanya di
form Tambah/Edit dan halaman Detail.

**Alasan:** Mengikuti mockup — teks panjang (255 char) akan merusak layout tabel.

---

## D-15 — PostgreSQL untuk production

**Masalah:** SQLite cocok untuk development, tetapi kurang tepat untuk
production: penulisan terkunci pada satu file, tanpa user/role, dan tanpa
koneksi jaringan.

**Keputusan (dikonfirmasi user):** **SQLite untuk development, PostgreSQL
untuk production.**

**Konsekuensi:**

- Tambah paket `Npgsql.EntityFrameworkCore.PostgreSQL` saat provider production
  diaktifkan.
- Connection string development di `appsettings.Development.json`, production
  lewat environment variable `ConnectionStrings__Default` — jangan commit
  kredensial.
- **Dua set migration** karena EF Core hanya membuat migration untuk provider
  aktif. Rujukan:
  [Migrations with Multiple Providers](https://learn.microsoft.com/ef/core/managing-schemas/migrations/providers).
- Nama tabel dan kolom snake_case tetap dipakai di kedua provider.

**Status:** baru keputusan dan dokumentasi. Kode masih SQLite-only sampai M1-6
dikerjakan.

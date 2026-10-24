# BUSINESS RULES — Aplikasi Data Kendaraan

Aturan perilaku sistem yang **tidak terlihat di wireframe** namun menentukan
benar/salahnya implementasi. Setiap aturan diberi ID agar bisa dirujuk dari
kode (`// BR-06`) dan dari test case.

---

## BR-01 — Uniknya Nomor Registrasi

`noRegistrasi` adalah **identitas tunggal** data kendaraan.

- Insert dengan nomor yang sudah ada → **ditolak** `409 Conflict`, bukan menimpa.
- Perbandingan unik **case-sensitive** di DB, tetapi saat pengecekan duplikat
  dilakukan **trim** dulu pada input. `B-7763-TXY` dan `B-7763-TXY ` (ada spasi)
  dianggap sama → ditolak.
- Respons error: `{ "message": "Nomor registrasi sudah terdaftar." }`

## BR-02 — Normalisasi Input

Diterapkan sebelum validasi & penyimpanan:

| Field                                   | Perlakuan                                                                                                                                         |
| --------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------- |
| `noRegistrasi`                          | `Trim()`. Simpan apa adanya (jangan auto-uppercase — data boleh berisi format lain).                                                              |
| `namaPemilik`                           | `Trim()`, kolaps spasi ganda jadi satu.                                                                                                           |
| `alamat`, `merkKendaraan`, `bahanBakar` | `Trim()`.                                                                                                                                         |
| `warna`                                 | `Trim()`, lalu pencocokan **case-insensitive** ke 4 opsi; simpan bentuk kanonik (`Merah`, `Hitam`, `Biru`, `Abu-Abu`). Nilai di luar itu → `400`. |
| Field kosong (`""`)                     | Disimpan sebagai `NULL`, bukan string kosong.                                                                                                     |

**Alasan:** string kosong vs NULL menimbulkan dua representasi "tidak ada nilai"
yang menyulitkan query dan tampilan.

## BR-03 — Field Wajib

Hanya dua field yang wajib: **No Registrasi Kendaraan** dan **Nama Pemilik**.

- Keduanya tidak boleh `null`, `""`, atau hanya whitespace.
- Enam field lainnya boleh dikosongkan → tersimpan `NULL`.
- Validasi berjalan di **client** (UX) dan **server** (otoritas). Server tidak
  boleh percaya client.

## BR-04 — Rentang Tahun Pembuatan

- Tipe: bilangan bulat (`int`).
- Rentang valid: **1900** s/d **tahun berjalan**.
- Batas atas **dinamis** (tahun berjalan), bukan konstanta — pada 2027 batas
  atas otomatis 2027. Implementasi: `DateTime.Now.Year`.
- Di atas batas → `400` dengan pesan
  `Tahun Pembuatan harus antara 1900 dan {tahunIni}.`
- Batas DB (`CHECK ... BETWEEN 1900 AND 2100`) hanya jaring pengaman terakhir;
  aturan bisnis yang sebenarnya ada di lapisan aplikasi.

## BR-05 — Kapasitas Silinder

- Tipe: bilangan bulat (`int`), **harus > 0**.
- Boleh kosong (`NULL`); bila diisi `0` atau negatif → `400`
  `Kapasitas Silinder harus lebih dari 0.`
- Tidak ada batas atas buatan (motor 125 cc sampai truk > 10.000 cc valid).
- Nilai bulat; input desimal (`150.5`) ditolak, bukan dibulatkan.

> Lihat `docs/07-DECISIONS.md` D-03: maxlength 4 di dokumen asli **tidak**
> berlaku untuk field ini.

## BR-06 — Warna Kendaraan (Enumerasi)

- Nilai sah **hanya**: `Merah`, `Hitam`, `Biru`, `Abu-Abu`.
- Boleh kosong (`NULL`) → form menampilkan placeholder "Pilih Warna".
- Validasi tiga lapis:
  1. Frontend: `<select>` hanya berisi 4 opsi.
  2. Server: `RegularExpression` di DTO.
  3. DB: `CHECK (warna IS NULL OR warna IN (...))`.
- Nilai selain 4 di atas (termasuk `merah` huruf kecil dari API langsung)
  dinormalisasi ke bentuk kanonik oleh BR-02; bila tidak bisa dipetakan → `400`.

## BR-07 — Bahan Bakar Bebas Teks

- `bahanBakar` adalah teks bebas maksimal 50 karakter, **bukan** enumerasi.
- Data contoh memakai `Bensin`; nilai lain (`Solar`, `Diesel`, `Listrik`, `Pertalite`)
  tetap sah.
- **Tidak boleh** divalidasi terhadap daftar warna — itu kesalahan dokumen asli
  (lihat `docs/07-DECISIONS.md` D-01).

## BR-08 — Mengubah Primary Key

Nomor registrasi boleh diubah lewat halaman Edit.

- Endpoint: `PUT /api/kendaraan/{noRegistrasiLama}`, body membawa `noRegistrasi` baru.
- Jika `baru == lama` → update biasa, `200`.
- Jika nomor baru sudah dipakai record **lain** → `409 Conflict`, data lama
  tidak berubah sama sekali.
- Operasi bersifat **atomik**: validasi + update dalam satu transaksi; tidak
  boleh ada kondisi data hilang bila gagal di tengah.
- Frontend wajib mengirim ID lama di URL agar server tahu record mana yang
  ditargetkan, meskipun field-nya diedit user.

## BR-09 — Penghapusan Data

- Hard delete (baris benar-benar hilang dari tabel). Tidak ada soft delete /
  kolom `deleted_at`.
- Wajib melewati **modal konfirmasi** dengan teks
  `Anda yakin menghapus data {noReg} ?` dan tombol `OK` / `Batal`.
- `Batal` (atau Esc / klik luar modal) → tidak ada request ke server.
- Menghapus nomor yang tidak ada → `404`, frontend menampilkan error lalu
  me-refresh tabel (data mungkin sudah dihapus user lain).
- Tidak ada bulk delete.

## BR-10 — Perilaku Pencarian

| Aspek           | Aturan                                                                                  |
| --------------- | --------------------------------------------------------------------------------------- |
| Trigger         | Hanya saat tombol **Search** diklik atau Enter di input (bukan live search per ketikan) |
| Operator        | `contains` (`LIKE %nilai%`), **case-insensitive**                                       |
| Kombinasi       | `noRegistrasi` DAN `namaPemilik` (AND, bukan OR)                                        |
| Filter kosong   | Diabaikan, bukan dianggap mencocokkan string kosong                                     |
| Keduanya kosong | Tampilkan **semua** data (fungsi reset)                                                 |
| Hasil kosong    | Tabel disembunyikan, tampil "Data kendaraan tidak ditemukan"                            |
| Urutan          | `noRegistrasi` ASC selalu                                                               |
| Nomor urut      | Di-reset mulai 1 pada tiap hasil filter                                                 |

## BR-11 — Mode Detail vs Edit

Keduanya memakai layout form yang **sama persis**, dibedakan oleh perilaku:

|             | Edit                      | Detail                    |
| ----------- | ------------------------- | ------------------------- |
| Field       | aktif                     | `disabled`                |
| Tombol aksi | `Ubah`, `Kembali`         | `Kembali` saja            |
| Sumber data | `GET /api/kendaraan/{id}` | `GET /api/kendaraan/{id}` |
| Aksi simpan | `PUT` lalu redirect       | tidak ada                 |

- Detail **tidak** boleh mengirim request `PUT` dalam kondisi apa pun.
- Field `disabled` (bukan `readonly`) dipakai agar nilai tidak ikut ter-submit.
- Teks pada field disabled harus tetap terbaca jelas (bukan abu pucat).

## BR-12 — Formatter Tampilan

Hanya berlaku di presentasi, **tidak** mengubah data tersimpan:

| Kolom                     | Aturan                                                  |
| ------------------------- | ------------------------------------------------------- |
| `No`                      | Nomor urut hasil query (1,2,3…), bukan nilai dari DB    |
| `Kapasitas`               | `{nilai} cc` — contoh `150 cc`                          |
| Semua field `NULL`/kosong | Ditampilkan `-` (dash), bukan `null`/`undefined`/kosong |
| `Tahun Pembuatan`         | Apa adanya, 4 digit                                     |

## BR-13 — Keamanan Input Tampilan

- Semua nilai dari API yang dirender ke tabel **wajib di-escape** sebelum
  masuk `innerHTML` (mis. `namaPemilik` berisi `<script>alert(1)</script>`).
- Gunakan `textContent` atau fungsi `escapeHtml()`; jangan string-concat mentah.
- Tag `<strong>` pada teks modal delete hanya membungkus `noReg` yang **sudah**
  di-escape.

## BR-14 — Konsistensi Pesan Error

- Semua pesan yang dilihat pengguna berbahasa **Indonesia** dan diakhiri titik.
- Pesan per field ditempelkan ke elemen `[data-error-for="{namaField}"]`.
- Pesan tingkat umum (404/409/500) muncul sebagai toast, bukan di field.
- Jangan pernah menampilkan stack trace / pesan exception mentah ke UI;
  log di server, kirim pesan ramah ke client.

## BR-15 — Siklus Hidup Data

- Data persisten di database (`kendaraan.db` untuk SQLite dev, PostgreSQL untuk
  production), bertahan setelah server restart.
- Seed 5 baris hanya dijalankan **jika tabel kosong**; restart tidak
  menduplikasi data.
- Tidak ada mekanisme backup/restore otomatis di versi ini.

---

## Matriks Aturan → Test Case

Rujukan silang ke `docs/04-TASKS.md` M4.

| Aturan | Test case M4                            |
| ------ | --------------------------------------- |
| BR-01  | #7 (Add No Reg duplikat)                |
| BR-03  | #6 (Add tanpa Nama Pemilik)             |
| BR-04  | #8 (Edit tahun "abc" / di luar rentang) |
| BR-05  | — (uji di unit test controller)         |
| BR-06  | — (uji unit: warna tidak valid → 400)   |
| BR-08  | — (uji unit: PUT PK bentrok → 409)      |
| BR-09  | #10, #11                                |
| BR-10  | #2, #3, #4                              |
| BR-11  | #9                                      |
| BR-15  | #12                                     |

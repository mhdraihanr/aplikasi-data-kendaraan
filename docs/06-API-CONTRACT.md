# API CONTRACT — Aplikasi Data Kendaraan

Base URL dev: `http://localhost:5000/api`
Format: JSON (`application/json`), field camelCase.

---

## Ringkasan Endpoint

| Method | Path                        | Fungsi                  | Sukses |
| ------ | --------------------------- | ----------------------- | ------ |
| GET    | `/kendaraan`                | List + filter           | `200`  |
| GET    | `/kendaraan/{noRegistrasi}` | Detail satu data        | `200`  |
| POST   | `/kendaraan`                | Tambah                  | `201`  |
| PUT    | `/kendaraan/{noRegistrasi}` | Ubah (PK boleh berubah) | `200`  |
| DELETE | `/kendaraan/{noRegistrasi}` | Hapus                   | `204`  |

---

## 1. `GET /api/kendaraan`

Query params (opsional, keduanya AND, contains + case-insensitive):

| Param          | Tipe   | Contoh   |
| -------------- | ------ | -------- |
| `noRegistrasi` | string | `b-7763` |
| `namaPemilik`  | string | `messi`  |

Response `200`:

```json
[
  {
    "noRegistrasi": "B-7763-TXY",
    "namaPemilik": "Lionel Messi",
    "alamat": "Jl. Achmad Yani No 89 Jakarta Pusat",
    "merkKendaraan": "Honda PCX",
    "tahunPembuatan": 2018,
    "kapasitasSilinder": 150,
    "warna": "Hitam",
    "bahanBakar": "Bensin"
  }
]
```

Urut `noRegistrasi` ASC. Tanpa hasil → `[]`.

---

## 2. `GET /api/kendaraan/{noRegistrasi}`

- `200` → satu object seperti di atas
- `404` → `{ "message": "Data kendaraan tidak ditemukan." }`

---

## 3. `POST /api/kendaraan`

Request:

```json
{
  "noRegistrasi": "B-1234-ABC",
  "namaPemilik": "Budi",
  "alamat": "Jl. Mawar 1",
  "merkKendaraan": "Yamaha NMAX",
  "tahunPembuatan": 2021,
  "kapasitasSilinder": 155,
  "warna": "Biru",
  "bahanBakar": "Bensin"
}
```

- `201` + header `Location: /api/kendaraan/B-1234-ABC` + object tersimpan
- `400` validasi gagal
- `409` PK sudah ada

---

## 4. `PUT /api/kendaraan/{noRegistrasi}`

`{noRegistrasi}` = PK **lama**. Body berisi seluruh field (termasuk
`noRegistrasi` baru bila PK diubah).

- `200` + object setelah update
- `400` validasi gagal
- `404` PK lama tidak ditemukan
- `409` `noRegistrasi` baru sudah dipakai record lain

---

## 5. `DELETE /api/kendaraan/{noRegistrasi}`

- `204` tanpa body
- `404` tidak ditemukan

---

## 6. Format Error

```json
{
  "message": "Data tidak valid.",
  "errors": {
    "namaPemilik": ["Nama Pemilik wajib diisi."],
    "tahunPembuatan": ["Tahun Pembuatan harus antara 1900 dan 2100."]
  }
}
```

`errors` hanya ada untuk `400`. Untuk `404`/`409` cukup `message`. Untuk `500`,
`ApiExceptionHandler` mengembalikan bentuk yang sama tanpa detail internal:

```json
{ "message": "Terjadi kesalahan pada server." }
```

Exception asli dicatat di log server beserta method, path, dan stack trace.
Frontend membaca `errors` untuk menempelkan pesan ke field yang sesuai
lewat atribut `[data-error-for="{namaField}"]`.

## 7. Status code ringkas

| Kode | Kapan                                                                     |
| ---- | ------------------------------------------------------------------------- |
| 200  | GET/PUT sukses                                                            |
| 201  | POST sukses                                                               |
| 204  | DELETE sukses                                                             |
| 400  | Validasi gagal / body tidak terbaca                                       |
| 404  | `noRegistrasi` tidak ada                                                  |
| 409  | PK duplikat                                                               |
| 500  | Error tak terduga, body `{ "message": "Terjadi kesalahan pada server." }` |

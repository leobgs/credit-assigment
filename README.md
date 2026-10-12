# Sistem Pengajuan Kredit - Backend REST API

REST API untuk mengelola entitas **pengajuan kredit** (CRUD), dilengkapi dengan JWT Authentication, input validation (FluentValidation), perhitungan angsuran anuitas presisi (decimal arithmetic), unit testing (xUnit), global error handling (RFC 7807 ProblemDetails), dan structured logging (Serilog).

Proyek ini dibangun berdasarkan spesifikasi **Take Home Test Senior Back-End Developer PT Skyworx Indonesia**.

---

## 1. Prerequisite

| Kebutuhan | Versi | Keterangan |
| --- | --- | --- |
| .NET SDK | 8.0.x | C# 12 / ASP.NET Core 8.0 |
| PostgreSQL | 16.3 / 16.x | Database relasional |
| dotnet-ef CLI | 8.x | EF Core Command-Line Tools |
| Git | Terbaru | Version control |
| Docker (Opsional) | Terbaru | Alternatif menjalankan PostgreSQL 16 |

---

## 2. Setup Database & Migrasi

### 2.1 Menggunakan PostgreSQL Lokal

1. Buat database `kredit_db`:
   ```bash
   psql -U postgres -c "CREATE DATABASE kredit_db;"
   ```

2. Jalankan migrasi Entity Framework Core:
   ```bash
   dotnet ef database update --project PengajuanKredit.Api
   ```

3. (Opsional) Mengisi data awal (seed):
   ```bash
   psql -U postgres -d kredit_db -f db/seed.sql
   ```

### 2.2 Alternatif Menggunakan Docker

Jalankan container PostgreSQL 16.3:
```bash
docker run --name pg16 -e POSTGRES_PASSWORD=postgres -p 5432:5432 -d postgres:16.3
docker exec -it pg16 psql -U postgres -c "CREATE DATABASE kredit_db;"
dotnet ef database update --project PengajuanKredit.Api
```

---

## 3. Konfigurasi Rahasia & JWT (User Secrets)

Kredensial dan secret JWT dapat diatur melalui `appsettings.json` atau secara aman menggunakan `dotnet user-secrets`:

```bash
cd PengajuanKredit.Api
dotnet user-secrets init
dotnet user-secrets set "Jwt:Key" "SuperSecretKeyForJwtAuthenticationMustBeAtLeast32CharsLong!"
dotnet user-secrets set "Auth:Username" "admin"
dotnet user-secrets set "Auth:Password" "Password123!"
```

**Kredensial Login Default:**
- **Username:** `admin`
- **Password:** `Password123!`

---

## 4. Cara Menjalankan Aplikasi & Swagger

Jalankan aplikasi dari root direktori:

```bash
dotnet run --project PengajuanKredit.Api
```

Aplikasi akan berjalan pada port lokal. Buka browser pada URL:
- **Swagger UI:** `http://localhost:5000/swagger` atau `https://localhost:5001/swagger`

Untuk mencoba endpoint di Swagger UI:
1. Panggil endpoint `POST /api/auth/login` dengan username `admin` dan password `Password123!`.
2. Salin token dari response.
3. Klik tombol **Authorize** di pojok kanan atas Swagger, masukkan `Bearer <token>`, lalu klik **Authorize**.

---

## 5. Cara Menjalankan Test (xUnit)

Jalankan seluruh rangkaian unit test dan integration test:

```bash
dotnet test
```

Seluruh 36 skenario test mencakup:
- Formula kalkulasi angsuran anuitas dan validasi rentang input.
- CRUD PengajuanKreditService dengan EF Core InMemory.
- Autentikasi dan otorisasi JWT (status 401 dan 200).
- ExceptionHandlingMiddleware (status 400, 404, dan 500 tanpa kebocoran stack trace).
- RequestLoggingMiddleware (pencatatan durasi dan proteksi token/password).

---

## 6. Daftar Endpoint REST API

| Method | Endpoint | Auth | Deskripsi |
| --- | --- | --- | --- |
| `POST` | `/api/auth/login` | Publik | Mendapatkan token JWT (`{ username, password }`) |
| `GET` | `/api/angsuran/hitung` | JWT | Hitung simulasi angsuran bulanan (`?plafon=&bunga=&tenor=`) |
| `POST` | `/api/pengajuan-kredit` | JWT | Membuat pengajuan kredit baru (`{ plafon, bunga, tenor }`) |
| `GET` | `/api/pengajuan-kredit` | JWT | Menampilkan daftar pengajuan terpaginasi (`?page=1&pageSize=10`) |
| `GET` | `/api/pengajuan-kredit/{id}` | JWT | Mendapatkan detail pengajuan berdasarkan GUID |
| `PUT` | `/api/pengajuan-kredit/{id}` | JWT | Memperbarui pengajuan kredit berdasarkan GUID |
| `DELETE` | `/api/pengajuan-kredit/{id}` | JWT | Menghapus pengajuan kredit berdasarkan GUID |

---

## 7. Ekspor Database

File backup database sudah diekspor ke `kredit_db.sql`:

```bash
pg_dump -U postgres -d kredit_db -F p -f kredit_db.sql
```

Untuk me-restore file dump ke database:
```bash
psql -U postgres -d kredit_db -f kredit_db.sql
```

---

## 8. Query SQL Khusus (Soal 1)

Dua query SQL yang diminta pada Soal 1 tersedia di file `db/schema.sql`:

1. **Pengajuan dengan tenor terpanjang dan plafon tertinggi:**
   ```sql
   SELECT * FROM pengajuan_kredit
   ORDER BY tenor DESC, plafon DESC
   LIMIT 1;
   ```
2. **Rata-rata bunga seluruh pengajuan:**
   ```sql
   SELECT AVG(bunga) AS rata_rata_bunga FROM pengajuan_kredit;
   ```

---

## 9. Checklist Pemenuhan PRD (Bagian 14)

| No | Kriteria PRD | Status | Bukti Verifikasi |
| --- | --- | :---: | --- |
| 1 | Project .NET 8 dalam 1 solution dan bisa dijalankan | **Selesai** | `dotnet build` sukses 0 error; solution `PengajuanKredit.sln` |
| 2 | Ekspor database (.sql) | **Selesai** | File `kredit_db.sql` dan `db/schema.sql` |
| 3 | Index plafon dan tenor ada | **Selesai** | `idx_pengajuan_plafon`, `idx_pengajuan_tenor`, `idx_pengajuan_tenor_plafon` di database |
| 4 | Query SQL tersedia | **Selesai** | Bagian 3 di `db/schema.sql` dan dieksekusi sukses |
| 5 | CRUD, JWT, dan validasi berfungsi | **Selesai** | `PengajuanKreditController`, `AuthController`, FluentValidation terpasang |
| 6 | API angsuran sesuai rumus anuitas | **Selesai** | `AngsuranCalculator` menghitung persis formula PRD dengan decimal O(log n) |
| 7 | Unit test lulus (`dotnet test`) | **Selesai** | 36 dari 36 test lulus 100% |
| 8 | Error handling dan Serilog aktif | **Selesai** | `ExceptionHandlingMiddleware` (ProblemDetails) & `RequestLoggingMiddleware` |
| 9 | Jawaban Soal 6 dan 7 (dengan diagram) | **Selesai** | Tersedia di `docs/JAWABAN_SOAL_6_7.md` |
| 10 | README berisi cara menjalankan | **Selesai** | Panduan lengkap di `README.md` |

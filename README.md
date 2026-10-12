# Sistem Pengajuan Kredit - Backend REST API

REST API untuk mengelola entitas **pengajuan kredit** (CRUD), dilengkapi dengan JWT Authentication, input validation (FluentValidation), perhitungan angsuran anuitas presisi (decimal arithmetic), unit testing (xUnit), global error handling (RFC 7807 ProblemDetails), dan structured logging (Serilog).

Proyek ini dibangun berdasarkan spesifikasi **Take Home Test Senior Back-End Developer PT Skyworx Indonesia**.

---

## 1. Prerequisite & Catatan Environment Mac

| Kebutuhan         | Versi       | Keterangan                           |
| ----------------- | ----------- | ------------------------------------ |
| .NET SDK          | 8.0.x       | C# 12 / ASP.NET Core 8.0             |
| PostgreSQL        | 16.3 / 16.x | Database relasional                  |
| dotnet-ef CLI     | 8.x         | EF Core Command-Line Tools           |
| Git               | Terbaru     | Version control                      |
| Docker (Opsional) | Terbaru     | Alternatif menjalankan PostgreSQL 16 |

> **Catatan Pengguna macOS (Apple Silicon / Intel):**  
> .NET 8 SDK telah terpasang di `$HOME/.dotnet`. Jika membuka terminal baru dan perintah `dotnet` belum terbaca, muat konfigurasi shell dengan perintah:
>
> ```bash
> source ~/.zshrc
> ```
>
> Atau set environment variable:
>
> ```bash
> export DOTNET_ROOT="$HOME/.dotnet"
> export PATH="$DOTNET_ROOT:$DOTNET_ROOT/tools:$PATH"
> ```
>
> Verifikasi:
>
> ```bash
> dotnet --version      # 8.0.x
> dotnet ef --version   # 8.0.x
> ```

---

## 2. Setup Database & Migrasi

### 2.1 Menggunakan PostgreSQL Lokal

1. Buat database `kredit_db`:

   ```bash
   psql -U postgres -c "CREATE DATABASE kredit_db;"
   ```

2. Jalankan migrasi Entity Framework Core untuk membuat tabel dan index:

   ```bash
   dotnet ef database update --project PengajuanKredit.Api
   ```

3. (Opsional) Masukkan data contoh (seed):
   ```bash
   psql -U postgres -d kredit_db -f db/seed.sql
   ```

### 2.2 Alternatif Menggunakan Docker

Jika memilih menjalankan PostgreSQL melalui container Docker:

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
cd ..
```

**Kredensial Login Default:**

- **Username:** `admin`
- **Password:** `Password123!`

---

## 4. Cara Menjalankan Aplikasi & Mengakses Swagger UI

### 4.1 Menjalankan Server API

Dari root direktori project, jalankan:

```bash
dotnet run --project PengajuanKredit.Api
```

Aplikasi akan berjalan dan menampilkan log di console. Port default umumnya:

- **HTTP:** `http://localhost:5000` atau `http://localhost:5242`
- **HTTPS:** `https://localhost:5001` atau `https://localhost:7142`

Buka browser dan akses Swagger UI pada:

```text
http://localhost:5000/swagger
```

_(atau sesuaikan dengan port yang muncul di output terminal)_

### 4.2 Alur Autentikasi di Swagger UI:

1. Scroll ke endpoint **POST `/api/auth/login`**.
2. Klik **Try it out**, masukkan body JSON:
   ```json
   {
     "username": "admin",
     "password": "Password123!"
   }
   ```
3. Klik **Execute**, salin nilai `token` dari response.
4. Klik tombol hijau **Authorize** di pojok kanan atas Swagger UI.
5. Masukkan format: `Bearer <token_yang_disalin>`, lalu klik **Authorize**.
6. Sekarang semua endpoint terproteksi (`/api/pengajuan-kredit` dan `/api/angsuran/hitung`) dapat diuji langsung dari Swagger.

### 4.3 Menguji Menggunakan `curl` (Terminal)

1. **Login & Dapatkan Token:**

   ```bash
   TOKEN=$(curl -s -X POST http://localhost:5000/api/auth/login \
     -H "Content-Type: application/json" \
     -d '{"username":"admin","password":"Password123!"}' | grep -o '"token":"[^"]*' | cut -d'"' -f4)
   echo $TOKEN
   ```

2. **Hitung Angsuran:**

   ```bash
   curl -X GET "http://localhost:5000/api/angsuran/hitung?plafon=100000000&bunga=12&tenor=60" \
     -H "Authorization: Bearer $TOKEN"
   ```

3. **Buat Pengajuan Kredit Baru:**

   ```bash
   curl -X POST http://localhost:5000/api/pengajuan-kredit \
     -H "Authorization: Bearer $TOKEN" \
     -H "Content-Type: application/json" \
     -d '{"plafon": 100000000, "bunga": 12, "tenor": 60}'
   ```

4. **Lihat Daftar Pengajuan (Paginasi):**
   ```bash
   curl -X GET "http://localhost:5000/api/pengajuan-kredit?page=1&pageSize=10" \
     -H "Authorization: Bearer $TOKEN"
   ```

---

## 5. Cara Menjalankan Test (xUnit)

Jalankan seluruh 36 skenario test:

```bash
dotnet test
```

Semua pengujian mencakup:

- Formula anuitas kalkulator angsuran dan validasi rentang nilai.
- Operasi CRUD `PengajuanKreditService` dengan EF Core InMemory.
- Autentikasi & otorisasi JWT (status 401 dan 200).
- `ExceptionHandlingMiddleware` (status 400, 404, dan 500 tanpa kebocoran stack trace).
- `RequestLoggingMiddleware` (pencatatan durasi dan proteksi token/password).

---

## 6. Daftar Endpoint REST API

| Method   | Endpoint                     | Auth   | Deskripsi                                                        |
| -------- | ---------------------------- | ------ | ---------------------------------------------------------------- |
| `POST`   | `/api/auth/login`            | Publik | Mendapatkan token JWT (`{ username, password }`)                 |
| `GET`    | `/api/angsuran/hitung`       | JWT    | Hitung simulasi angsuran bulanan (`?plafon=&bunga=&tenor=`)      |
| `POST`   | `/api/pengajuan-kredit`      | JWT    | Membuat pengajuan kredit baru (`{ plafon, bunga, tenor }`)       |
| `GET`    | `/api/pengajuan-kredit`      | JWT    | Menampilkan daftar pengajuan terpaginasi (`?page=1&pageSize=10`) |
| `GET`    | `/api/pengajuan-kredit/{id}` | JWT    | Mendapatkan detail pengajuan berdasarkan GUID                    |
| `PUT`    | `/api/pengajuan-kredit/{id}` | JWT    | Memperbarui pengajuan kredit berdasarkan GUID                    |
| `DELETE` | `/api/pengajuan-kredit/{id}` | JWT    | Menghapus pengajuan kredit berdasarkan GUID                      |

---

## 7. Ekspor Database

File backup database telah diekspor ke `kredit_db.sql`:

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

## 9. Jawaban Soal 6 & 7 (Ringkasan)

> Jawaban lengkap beserta diagram Mermaid tersedia di [`docs/JAWABAN_SOAL_6_7.md`](docs/JAWABAN_SOAL_6_7.md).

### 9.1 Arsitektur Enterprise

```
Client -> Load Balancer -> API Gateway (rate limit, routing, JWT check)
   -> Auth Service (Identity/JWT) -> User DB
   -> Kredit Service -> Redis (cache) -> PostgreSQL (primary + read replica)
   -> Message Broker -> Service lain (notifikasi, audit)
Logging & monitoring: Serilog -> ELK/Seq, Prometheus + Grafana
```

Diagram visual dibuat pada saat penulisan dokumen jawaban final (draw.io atau Mermaid).

### 9.2 CI/CD dan DevOps

- **Pipeline:** build, restore, `dotnet test`, analisis kode statis, build Docker image, push ke registry, deploy ke staging, smoke test, deploy ke production.
- **Deployment aman dan scalable:** migrasi database terkontrol, secret di vault/secret manager, rolling update atau blue-green, health check dan readiness probe, auto-scaling (HPA), serta rollback otomatis bila gagal.

### 9.3 Leadership dan Code Review

- **Code review:** Pull Request kecil dengan checklist (kebenaran, keterbacaan, test, keamanan, performa), review konstruktif dan menjelaskan alasan, otomatisasi lint/test lewat CI.
- **Mentoring junior:** pair programming, tugas bertahap dengan tanggung jawab meningkat, feedback rutin 1:1, sharing session, dan dokumentasi standar tim.

### 9.4 Problem Solving

- **Lonjakan traffic:** cek monitoring dan metrik, aktifkan auto-scaling, perkuat cache dan rate limiting, identifikasi bottleneck (DB atau API), lalu lakukan postmortem.
- **Bug hanya di production:** reproduksi lewat log dan trace, bandingkan konfigurasi dan data antar environment, gunakan feature flag atau canary, perbaiki dengan hotfix dan tambahkan test regresi.
- **Perubahan di luar requirement:** catat sebagai change request, analisis dampak (waktu, biaya, risiko), diskusikan prioritas dengan stakeholder, minta persetujuan tertulis, lalu masukkan ke backlog atau sprint berikutnya.

---

## 10. Cara Pengumpulan

### 10.1 Ekspor Database

Database telah diekspor dalam tiga format:

| Format | File | Keterangan |
| ------ | ---- | ---------- |
| SQL dump | `kredit_db.sql` | Full dump `pg_dump` (schema + data) |
| SQL schema | `db/schema.sql` | DDL tabel, index, dan query Soal 1 |
| SQL seed | `db/seed.sql` | Data contoh awal |
| CSV | `db/csv/pengajuan_kredit.csv` | Ekspor data tabel ke CSV |

### 10.2 Source Code (1 File ZIP)

File ZIP pengumpulan tersedia di Desktop:

```
~/Desktop/PengajuanKredit-Submission.zip
```

Isi ZIP mencakup seluruh source code project .NET 8 (tanpa folder `bin/`, `obj/`, `.git/`, `logs/`).

**Untuk membuat ulang ZIP:**

```bash
cd /Users/user/private/credit-assigment
zip -r ~/Desktop/PengajuanKredit-Submission.zip . \
  -x ".git/*" "*/bin/*" "*/obj/*" "*/logs/*" ".vscode/*" "*.DS_Store"
```

### 10.3 Isi Pengumpulan

| No  | Item                                     | Status       |
| --- | ---------------------------------------- | ------------ |
| 1   | Project .NET 8 (1 file ZIP)              | ✅ Selesai    |
| 2   | Ekspor database (SQL + CSV)              | ✅ Selesai    |
| 3   | Index `plafon` dan `tenor`               | ✅ Selesai    |
| 4   | Query SQL (Soal 1)                       | ✅ Selesai    |
| 5   | CRUD + JWT + Validasi (Soal 2)           | ✅ Selesai    |
| 6   | API Angsuran Anuitas (Soal 3)            | ✅ Selesai    |
| 7   | Unit Test lulus (Soal 4)                 | ✅ Selesai    |
| 8   | Error Handling + Serilog (Soal 5)        | ✅ Selesai    |
| 9   | Jawaban Soal 6 & 7 (dengan diagram)     | ✅ Selesai    |
| 10  | README cara menjalankan                  | ✅ Selesai    |

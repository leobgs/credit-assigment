# PRD - Sistem Pengajuan Kredit

**Konteks:** Take Home Test Senior Back-End Developer (PT Skyworx Indonesia)  
**Stack wajib:** .NET Core 8.0, Entity Framework Core, PostgreSQL 16.3  
**Penilaian:** kode bersih, performa API, solusi scalable  

## 1. Ringkasan

Membangun REST API untuk mengelola entitas **pengajuan kredit** (CRUD), dilengkapi JWT Authentication, input validation, API perhitungan angsuran, unit test, error handling, dan logging. Dokumen ini juga memuat jawaban konseptual (Soal 6 dan 7). Seluruh pekerjaan mengikuti Soal 1 sampai 7 pada dokumen tes, tanpa menambah fitur di luar itu.

## 2. Ruang Lingkup

**Termasuk:** database dan tabel, index, query SQL, API CRUD, JWT, validasi, API angsuran, unit test (xUnit), error handling, logging (Serilog), jawaban Soal 6 dan 7.

**Tidak termasuk:** UI/frontend, manajemen user lengkap, payment, workflow approval kredit, dan fitur lain yang tidak diminta.

## 3. Aturan Pengumpulan

- Database diekspor dalam bentuk CSV/SQL/Backup.
- Source code dikumpulkan dalam 1 file project .NET (zip satu solution).
- API memakai .NET 8 dan Entity Framework.
- Memakai PostgreSQL 16.3.

## 4. Prerequisite (Instalasi Awal)

| Kebutuhan | Versi | Keterangan |
| --- | --- | --- |
| .NET SDK | 8.0.x | Bahasa C# dan runtime. Unduh dari dotnet.microsoft.com/download/dotnet/8.0 |
| PostgreSQL | 16.3 | Unduh dari postgresql.org/download (sertakan pgAdmin) |
| IDE | Visual Studio 2022 17.8+ atau VS Code + C# Dev Kit | Pilih salah satu |
| Git | terbaru | Version control |
| dotnet-ef | 8.x | EF Core CLI tool |
| Postman / Swagger | terbaru | Uji endpoint |
| Docker (opsional) | terbaru | Alternatif menjalankan PostgreSQL |

### 4.1 Langkah instalasi

1. Install **.NET SDK 8.0** lalu verifikasi:

```bash
dotnet --version      # harus 8.0.x
```

2. Install **PostgreSQL 16.3**. Catat password user `postgres` dan port (default 5432). Verifikasi:

```bash
psql --version        # psql (PostgreSQL) 16.3
```

Alternatif lewat Docker:

```bash
docker run --name pg16 -e POSTGRES_PASSWORD=postgres -p 5432:5432 -d postgres:16.3
```

3. Install EF Core CLI:

```bash
dotnet tool install --global dotnet-ef --version 8.*
```

4. Buat database:

```sql
CREATE DATABASE kredit_db;
```

## 5. Inisialisasi Project

```bash
mkdir PengajuanKredit && cd PengajuanKredit
dotnet new sln -n PengajuanKredit
dotnet new webapi -n PengajuanKredit.Api -f net8.0
dotnet new xunit -n PengajuanKredit.Tests -f net8.0
dotnet sln add PengajuanKredit.Api PengajuanKredit.Tests
dotnet add PengajuanKredit.Tests reference PengajuanKredit.Api

cd PengajuanKredit.Api
dotnet add package Microsoft.EntityFrameworkCore --version 8.*
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL --version 8.*
dotnet add package Microsoft.EntityFrameworkCore.Design --version 8.*
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer --version 8.*
dotnet add package Serilog.AspNetCore
dotnet add package FluentValidation.AspNetCore
dotnet add package Swashbuckle.AspNetCore

cd ../PengajuanKredit.Tests
dotnet add package Moq
dotnet add package FluentAssertions
dotnet add package Microsoft.EntityFrameworkCore.InMemory --version 8.*
dotnet add package Microsoft.AspNetCore.Mvc.Testing --version 8.*
```

### 5.1 Struktur folder

```
PengajuanKredit.Api/
  Controllers/      AuthController, PengajuanKreditController, AngsuranController
  Services/         IPengajuanKreditService, AngsuranCalculator
  Data/             AppDbContext, Migrations/
  Models/           PengajuanKredit (entity)
  Dtos/             Request dan response DTO
  Validators/       FluentValidation
  Middleware/       ExceptionHandlingMiddleware, RequestLoggingMiddleware
  appsettings.json  ConnectionString, Jwt, Serilog
PengajuanKredit.Tests/
  Services/ dan Controllers/
```

### 5.2 Konfigurasi awal (`appsettings.json`)

```json
{
  "ConnectionStrings": {
    "Default": "Host=localhost;Port=5432;Database=kredit_db;Username=postgres;Password=postgres"
  },
  "Jwt": { "Key": "<minimal-32-karakter>", "Issuer": "PengajuanKredit", "Audience": "PengajuanKreditUsers", "ExpireMinutes": 60 }
}
```

Catatan: secret JWT dan password jangan di-commit. Gunakan `dotnet user-secrets` atau environment variable.

## 6. Soal 1 - Database dan Query Optimization

### 6.1 Skema tabel `pengajuan_kredit`

| Kolom | Tipe | Keterangan |
| --- | --- | --- |
| id | UUID (PK) | Primary key unik |
| plafon | numeric | Jumlah pinjaman |
| bunga | numeric(5,2) | Bunga dalam persen |
| tenor | integer | Lama cicilan (bulan) |
| angsuran | numeric | Angsuran per bulan |
| created_at | timestamp | Waktu pembuatan |
| updated_at | timestamp | Waktu update terakhir |

```sql
CREATE TABLE pengajuan_kredit (
  id         UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  plafon     NUMERIC NOT NULL CHECK (plafon > 0),
  bunga      NUMERIC(5,2) NOT NULL CHECK (bunga > 0 AND bunga <= 100),
  tenor      INTEGER NOT NULL CHECK (tenor > 0),
  angsuran   NUMERIC NOT NULL,
  created_at TIMESTAMP NOT NULL DEFAULT now(),
  updated_at TIMESTAMP NOT NULL DEFAULT now()
);
```

### 6.2 Indexing

```sql
CREATE INDEX idx_pengajuan_plafon ON pengajuan_kredit (plafon);
CREATE INDEX idx_pengajuan_tenor  ON pengajuan_kredit (tenor);
-- opsional (composite untuk query tenor + plafon):
CREATE INDEX idx_pengajuan_tenor_plafon ON pengajuan_kredit (tenor DESC, plafon DESC);
```

### 6.3 Query SQL yang diminta

```sql
-- Pengajuan dengan tenor terpanjang dan plafon tertinggi
SELECT * FROM pengajuan_kredit
ORDER BY tenor DESC, plafon DESC
LIMIT 1;

-- Rata-rata bunga semua pengajuan
SELECT AVG(bunga) AS rata_rata_bunga FROM pengajuan_kredit;
```

### 6.4 Migrasi lewat EF Core

```bash
dotnet ef migrations add InitialCreate --project PengajuanKredit.Api
dotnet ef database update --project PengajuanKredit.Api
```

Ekspor database untuk pengumpulan:

```bash
pg_dump -U postgres -d kredit_db -F p -f kredit_db.sql
```

## 7. Soal 2 - API CRUD, Authentication, Validation

### 7.1 Endpoint

| Method | Endpoint | Fungsi | Auth |
| --- | --- | --- | --- |
| POST | /api/auth/login | Mendapatkan JWT (pendukung agar JWT bisa diuji) | Tidak |
| POST | /api/pengajuan-kredit | Create | JWT |
| GET | /api/pengajuan-kredit | List semua data | JWT |
| GET | /api/pengajuan-kredit/{id} | Read by ID | JWT |
| PUT | /api/pengajuan-kredit/{id} | Update | JWT |
| DELETE | /api/pengajuan-kredit/{id} | Delete | JWT |

Asumsi: user untuk login cukup hardcoded atau seed sederhana, karena manajemen user tidak masuk scope soal. Semua endpoint CRUD memakai atribut `[Authorize]`.

### 7.2 Aturan validasi

- `plafon` > 0
- `bunga` > 0 dan <= 100 (persentase valid)
- `tenor` > 0 (integer)
- Nilai negatif atau nol ditolak dengan HTTP 400.

### 7.3 Perilaku Create dan Update

`angsuran` dihitung otomatis oleh server memakai `AngsuranCalculator` (Soal 3), sehingga konsisten. `created_at` diisi saat Create, `updated_at` diperbarui setiap Update.

## 8. Soal 3 - API Perhitungan Angsuran

**Endpoint:** `GET /api/angsuran/hitung?plafon=100000000&bunga=12&tenor=60` (JWT)

**Rumus anuitas:** `A = P x r / (1 - (1 + r)^-n)`

- P = plafon, r = bunga tahunan / 12 / 100, n = tenor (bulan)

**Contoh kasus:** P = 100.000.000, bunga 12% per tahun (r = 0,01), n = 60. Hasil angsuran sekitar **Rp2.224.445 per bulan**.

Catatan implementasi: gunakan tipe `decimal` (bukan double), bulatkan dengan `Math.Round(x, 2)`, dan tangani kasus bunga 0 bila diperlukan (A = P / n). Karena validasi mewajibkan bunga > 0, kasus ini cukup sebagai pengaman.

Response contoh:

```json
{ "plafon": 100000000, "bunga": 12, "tenor": 60, "angsuranPerBulan": 2224444.47 }
```

## 9. Soal 4 - Unit Testing (xUnit)

Minimal 3 skenario untuk CRUD dan perhitungan angsuran. Rencana:

| Area | Skenario |
| --- | --- |
| Angsuran | Contoh kasus 100 juta, 12%, 60 bulan menghasilkan nilai yang benar |
| Angsuran | Input nol atau negatif menghasilkan error validasi |
| CRUD Create | Data valid tersimpan dan angsuran terisi |
| CRUD Read | ID tidak ada menghasilkan 404 |
| CRUD Update | Data valid memperbarui `updated_at` |
| CRUD Delete | Data terhapus, lalu GET menghasilkan 404 |
| Auth | Akses tanpa token menghasilkan 401 |

Menjalankan test:

```bash
dotnet test
```

## 10. Soal 5 - Error Handling dan Logging

**Error handling** lewat global exception middleware dengan format `ProblemDetails`:

| Kasus | HTTP Status |
| --- | --- |
| Data tidak ditemukan | 404 Not Found |
| Input tidak valid | 400 Bad Request (berisi daftar error validasi) |
| Tidak terautentikasi | 401 Unauthorized |
| Error tak terduga | 500 Internal Server Error (tanpa membocorkan stack trace) |

**Logging (Serilog):** mencatat request dan response (method, path, status code, durasi), ke console dan file `logs/log-.txt` (rolling harian). Jangan log data sensitif seperti password atau token.

## 11. Soal 6 - Performance dan Scalability (Poin Jawaban)

1. **Performa untuk ribuan user per hari:** index pada plafon dan tenor, pagination pada endpoint list, `AsNoTracking()` untuk query baca, async/await end-to-end, connection pooling Npgsql, response compression, rate limiting, dan horizontal scaling di belakang load balancer (API bersifat stateless karena memakai JWT).
2. **Caching (Redis):** ya. Pakai pola cache-aside untuk `GET by id` dan list. Key contoh `kredit:{id}` dan `kredit:list:{page}`, TTL 5 sampai 10 menit, invalidasi saat Create, Update, atau Delete. Implementasi lewat `IDistributedCache` dengan paket `StackExchange.Redis`.
3. **Microservices:** pisahkan Auth Service, Kredit Service, dan Kalkulasi Service. Gunakan API Gateway (misalnya YARP), database per service, komunikasi sinkron lewat REST/gRPC dan asinkron lewat message broker (RabbitMQ/Kafka), health check, service discovery, dan distributed tracing (OpenTelemetry). Deploy lewat Docker dan Kubernetes.

## 12. Soal 7 - System Design dan Leadership (Poin Jawaban)

### 12.1 Arsitektur enterprise

```
Client -> Load Balancer -> API Gateway (rate limit, routing, JWT check)
   -> Auth Service (Identity/JWT) -> User DB
   -> Kredit Service -> Redis (cache) -> PostgreSQL (primary + read replica)
   -> Message Broker -> Service lain (notifikasi, audit)
Logging & monitoring: Serilog -> ELK/Seq, Prometheus + Grafana
```

Diagram visual dibuat pada saat penulisan dokumen jawaban final (draw.io atau Mermaid).

### 12.2 CI/CD dan DevOps

- **Pipeline:** build, restore, `dotnet test`, analisis kode statis, build Docker image, push ke registry, deploy ke staging, smoke test, deploy ke production.
- **Deployment aman dan scalable:** migrasi database terkontrol, secret di vault/secret manager, rolling update atau blue-green, health check dan readiness probe, auto-scaling (HPA), serta rollback otomatis bila gagal.

### 12.3 Leadership dan code review

- **Code review:** Pull Request kecil dengan checklist (kebenaran, keterbacaan, test, keamanan, performa), review konstruktif dan menjelaskan alasan, otomatisasi lint/test lewat CI.
- **Mentoring junior:** pair programming, tugas bertahap dengan tanggung jawab meningkat, feedback rutin 1:1, sharing session, dan dokumentasi standar tim.

### 12.4 Problem solving

- **Lonjakan traffic:** cek monitoring dan metrik, aktifkan auto-scaling, perkuat cache dan rate limiting, identifikasi bottleneck (DB atau API), lalu lakukan postmortem.
- **Bug hanya di production:** reproduksi lewat log dan trace, bandingkan konfigurasi dan data antar environment, gunakan feature flag atau canary, perbaiki dengan hotfix dan tambahkan test regresi.
- **Perubahan di luar requirement:** catat sebagai change request, analisis dampak (waktu, biaya, risiko), diskusikan prioritas dengan stakeholder, minta persetujuan tertulis, lalu masukkan ke backlog atau sprint berikutnya.

## 13. Rencana Pengerjaan

| Tahap | Pekerjaan |
| --- | --- |
| 1 | Install prerequisite, inisialisasi solution dan package |
| 2 | Database, migrasi, index, query SQL (Soal 1) |
| 3 | Entity, DbContext, CRUD, validasi, JWT (Soal 2) |
| 4 | Kalkulator dan endpoint angsuran (Soal 3) |
| 5 | Middleware error handling dan Serilog (Soal 5) |
| 6 | Unit test (Soal 4) |
| 7 | Dokumen jawaban Soal 6 dan 7 |
| 8 | Ekspor database, rapikan, zip project, kumpulkan |

## 14. Checklist Pengumpulan

- [ ] Project .NET 8 dalam 1 file (zip) dan bisa dijalankan
- [ ] Ekspor database (.sql/backup/CSV)
- [ ] Index plafon dan tenor ada
- [ ] Query SQL tersedia
- [ ] CRUD, JWT, dan validasi berfungsi
- [ ] API angsuran sesuai rumus
- [ ] Unit test lulus (`dotnet test`)
- [ ] Error handling dan Serilog aktif
- [ ] Jawaban Soal 6 dan 7 (dengan diagram)
- [ ] README berisi cara menjalankan

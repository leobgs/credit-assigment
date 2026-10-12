-- =====================================================================
-- Schema Database: Sistem Pengajuan Kredit
-- Stack: PostgreSQL 16.3 / .NET 8 / EF Core 8
-- Sesuai PRD.md Bagian 6 (Soal 1 - Database dan Query Optimization)
-- =====================================================================

-- 1. Definisi Tabel pengajuan_kredit
CREATE TABLE IF NOT EXISTS pengajuan_kredit (
    id         UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    plafon     NUMERIC NOT NULL CHECK (plafon > 0),
    bunga      NUMERIC(5,2) NOT NULL CHECK (bunga > 0 AND bunga <= 100),
    tenor      INTEGER NOT NULL CHECK (tenor > 0),
    angsuran   NUMERIC NOT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT now(),
    updated_at TIMESTAMP NOT NULL DEFAULT now()
);

-- =====================================================================
-- 2. Indexing dan Penjelasan
-- =====================================================================

-- Index pada kolom plafon:
-- Alasan: Mempercepat filter query range (misal: WHERE plafon BETWEEN ...), 
-- pengurutan (ORDER BY plafon), serta agregasi berdasarkan nominal pinjaman.
CREATE INDEX IF NOT EXISTS idx_pengajuan_plafon 
    ON pengajuan_kredit (plafon);

-- Index pada kolom tenor:
-- Alasan: Mempercepat filtering dan segmentasi berdasarkan durasi pinjaman 
-- (misal: WHERE tenor = 12 atau WHERE tenor > 36) serta pengurutan.
CREATE INDEX IF NOT EXISTS idx_pengajuan_tenor 
    ON pengajuan_kredit (tenor);

-- Composite Index pada (tenor DESC, plafon DESC):
-- Alasan: Mengoptimalkan secara spesifik query pencarian pengajuan dengan tenor 
-- terpanjang dan plafon tertinggi (ORDER BY tenor DESC, plafon DESC LIMIT 1). 
-- Dengan composite index ini, PostgreSQL dapat melakukan Index Scan / Backward Scan 
-- langsung tanpa full table scan ataupun expensive in-memory sort.
CREATE INDEX IF NOT EXISTS idx_pengajuan_tenor_plafon 
    ON pengajuan_kredit (tenor DESC, plafon DESC);

-- =====================================================================
-- 3. Query SQL yang diminta (Soal 1)
-- =====================================================================

-- Query (a): Pengajuan dengan tenor terpanjang dan plafon tertinggi
-- Memanfaatkan index idx_pengajuan_tenor_plafon
SELECT * 
FROM pengajuan_kredit
ORDER BY tenor DESC, plafon DESC
LIMIT 1;

-- Query (b): Rata-rata bunga dari semua pengajuan kredit
SELECT AVG(bunga) AS rata_rata_bunga 
FROM pengajuan_kredit;

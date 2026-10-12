-- =====================================================================
-- Seed Data: Sistem Pengajuan Kredit
-- Minimal 5 baris data contoh pengajuan kredit dengan angsuran presisi
-- =====================================================================

INSERT INTO pengajuan_kredit (id, plafon, bunga, tenor, angsuran, created_at, updated_at)
VALUES 
    (
        '11111111-1111-1111-1111-111111111111',
        100000000,
        12.00,
        60,
        2224444.47,
        NOW() - INTERVAL '5 days',
        NOW() - INTERVAL '5 days'
    ),
    (
        '22222222-2222-2222-2222-222222222222',
        50000000,
        10.00,
        36,
        1613359.54,
        NOW() - INTERVAL '4 days',
        NOW() - INTERVAL '4 days'
    ),
    (
        '33333333-3333-3333-3333-333333333333',
        250000000,
        9.50,
        120,
        3235532.80,
        NOW() - INTERVAL '3 days',
        NOW() - INTERVAL '3 days'
    ),
    (
        '44444444-4444-4444-4444-444444444444',
        15000000,
        14.00,
        12,
        1346929.56,
        NOW() - INTERVAL '2 days',
        NOW() - INTERVAL '2 days'
    ),
    (
        '55555555-5555-5555-5555-555555555555',
        500000000,
        8.75,
        180,
        4999655.22,
        NOW() - INTERVAL '1 day',
        NOW() - INTERVAL '1 day'
    ),
    (
        '66666666-6666-6666-6666-666666666666',
        30000000,
        11.50,
        24,
        1405257.65,
        NOW(),
        NOW()
    )
ON CONFLICT (id) DO NOTHING;

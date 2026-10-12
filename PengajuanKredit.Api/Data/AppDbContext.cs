using Microsoft.EntityFrameworkCore;
using PengajuanKredit.Api.Models;

namespace PengajuanKredit.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Models.PengajuanKredit> PengajuanKredits => Set<Models.PengajuanKredit>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Models.PengajuanKredit>(entity =>
        {
            entity.ToTable("pengajuan_kredit");

            entity.HasKey(e => e.Id);

            entity.Property(e => e.Id)
                .HasColumnName("id")
                .HasDefaultValueSql("gen_random_uuid()");

            entity.Property(e => e.Plafon)
                .HasColumnName("plafon")
                .HasColumnType("numeric")
                .IsRequired();

            entity.Property(e => e.Bunga)
                .HasColumnName("bunga")
                .HasColumnType("numeric(5,2)")
                .IsRequired();

            entity.Property(e => e.Tenor)
                .HasColumnName("tenor")
                .IsRequired();

            entity.Property(e => e.Angsuran)
                .HasColumnName("angsuran")
                .HasColumnType("numeric")
                .IsRequired();

            entity.Property(e => e.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("timestamp without time zone")
                .HasDefaultValueSql("now()")
                .IsRequired();

            entity.Property(e => e.UpdatedAt)
                .HasColumnName("updated_at")
                .HasColumnType("timestamp without time zone")
                .HasDefaultValueSql("now()")
                .IsRequired();

            entity.HasIndex(e => e.Plafon)
                .HasDatabaseName("idx_pengajuan_plafon");

            entity.HasIndex(e => e.Tenor)
                .HasDatabaseName("idx_pengajuan_tenor");

            entity.HasIndex(e => new { e.Tenor, e.Plafon })
                .IsDescending(true, true)
                .HasDatabaseName("idx_pengajuan_tenor_plafon");
        });
    }
}

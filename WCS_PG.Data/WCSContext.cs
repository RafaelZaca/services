using Microsoft.EntityFrameworkCore;
using WCS_PG.Data.Models;

namespace WCS_PG.Data
{
    public class WCSContext: DbContext
    {
        public WCSContext(DbContextOptions options) : base(options) { }
        public DbSet<ClientCustomization> ClientCustomizations { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<GroupPermission> GroupPermissions { get; set; }
        public DbSet<ManualTriage> ManualTriages { get; set; }
        public DbSet<OperationalMetrics> OperationalMetrics { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<PickRequest> PickRequests { get; set; }
        public DbSet<PickRequestItem> PickRequestItems { get; set; }
        public DbSet<ProductionHourly> ProductionHourlies { get; set; }
        public DbSet<ProductionHourlyDetail> ProductionHourlyDetails { get; set; }
        public DbSet<Ramp> Ramps { get; set; }
        public DbSet<RampAllocation> RampAllocations { get; set; }
        public DbSet<RampConfiguration> RampConfigurations { get; set; }
        public DbSet<RampMetrics> RampMetrics { get; set; }
        public DbSet<Rejection> Rejections { get; set; }
        public DbSet<SkuBatch> SkuBatches { get; set; }
        public DbSet<SkuBatchUsage> SkuBatchUsages { get; set; }
        public DbSet<SystemAlert> SystemAlerts { get; set; }
        public DbSet<SystemParameter> SystemParameters { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Wave> Waves { get; set; }
        public DbSet<WavePickRequest> WavePickRequests { get; set; }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            // Configuração para usar apenas a tabela GroupPermissions
            modelBuilder.Entity<Group>()
                .HasMany(g => g.Permissions)
                .WithMany(p => p.Groups)
                .UsingEntity<GroupPermission>(
                    j => j.HasOne(gp => gp.Permission)
                         .WithMany()
                         .HasForeignKey(gp => gp.PermissionId),
                    j => j.HasOne(gp => gp.Group)
                         .WithMany()
                         .HasForeignKey(gp => gp.GroupId),
                    j =>
                    {
                        j.ToTable("GroupPermissions");
                        j.HasKey(t => new { t.GroupId, t.PermissionId });
                    });

            modelBuilder.Entity<GroupPermission>()
                .HasKey(gp => new { gp.GroupId, gp.PermissionId });

            modelBuilder.Entity<WavePickRequest>()
                .HasKey(wp => new { wp.WaveId, wp.PickRequestId }); // Define chave composta

            modelBuilder.Entity<WavePickRequest>()
                .HasOne(wp => wp.Wave)
                .WithMany()
                .HasForeignKey(wp => wp.WaveId);

            modelBuilder.Entity<WavePickRequest>()
                .HasOne(wp => wp.PickRequest)
                .WithMany()
                .HasForeignKey(wp => wp.PickRequestId);

            modelBuilder.Entity<WavePickRequest>()
                .HasOne(wp => wp.AddedByUser)
                .WithMany()
                .HasForeignKey(wp => wp.AddedByUserId);

            modelBuilder.Entity<RampAllocation>()
                        .HasOne(ra => ra.Ramp)
                        .WithMany(r => r.RampAllocations)
                        .HasForeignKey(ra => ra.RampId)
                        .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Rejection>()
                        .HasOne(r => r.Ramp)
                        .WithMany()
                        .HasForeignKey(r => r.RampId)
                        .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<SkuBatchUsage>()
                        .HasOne(r => r.Ramp)
                        .WithMany()
                        .HasForeignKey(r => r.RampId)
                        .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<WavePickRequest>()
                        .HasOne(r => r.Wave)
                        .WithMany(r => r.WavePickRequests)
                        .HasForeignKey(r => r.WaveId)
                        .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<ManualTriage>()
                        .HasOne(r => r.DestinationRamp)
                        .WithMany()
                        .HasForeignKey(r => r.DestinationRampId)
                        .OnDelete(DeleteBehavior.NoAction);

        }
    }
}

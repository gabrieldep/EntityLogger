using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using AppLogger.Controls;
using System.Diagnostics.CodeAnalysis;

namespace AppLogger.Model
{
    public class DbContext : Microsoft.EntityFrameworkCore.DbContext
    {
        public DbSet<LogBase> LogsBase { get; set; }
        public DbSet<EntityAttribute> EntitiesAttributes { get; set; }

        public DbContext()
        {
        }

        public DbContext([NotNull] DbContextOptions options)
            : base(options)
        {            
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EntityAttribute>(ea =>
            {
                ea.HasKey(ea => ea.Id);

                ea.HasOne(ael => ael.LogBase)
                .WithMany(ca => ca.EntitiesAttributes)
                .HasForeignKey(ael => ael.IdLogBase)
                .HasConstraintName("EntityAttributeLogBaseFKConstraint");

                ea.Property(t => t.Type)
                  .IsRequired()
                  .HasConversion(
                      convertToProviderExpression: t => t.AssemblyQualifiedName,
                      convertFromProviderExpression: t => Type.GetType(t));
            });

            modelBuilder.Entity<LogBase>(lb =>
            {
                lb.HasKey(lb => lb.Id);

                lb.Property(t => t.EntityType)
                  .IsRequired()
                  .HasConversion(
                      convertToProviderExpression: t => t.AssemblyQualifiedName,
                      convertFromProviderExpression: t => Type.GetType(t));
            });
        }

        public async Task<int> SaveChangesAsync(string user)
        {
            IList<EntityEntry> changesInfo = ChangeTracker
                .Entries()
                .Where(t =>
                    t.State == EntityState.Modified
                    || t.State == EntityState.Deleted
                    || t.State == EntityState.Added)
                .ToList();

            foreach (var item in changesInfo)
            {
                LogControl control = new LogControl(this, user);
                if (item.State == EntityState.Modified)
                    await control.AddLogAsync(Enums.LogType.Edit, item);
                else if (item.State == EntityState.Deleted)
                    await control.AddLogAsync(Enums.LogType.Delete, item);
                else if (item.State == EntityState.Added)
                    await control.AddLogAsync(Enums.LogType.Create, item);
            }
            return await base.SaveChangesAsync();
        }
    }
}

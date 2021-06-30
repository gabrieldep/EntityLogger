using Microsoft.EntityFrameworkCore;
using System;
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
                lb.Property(t => t.EntityType)
                  .IsRequired()
                  .HasConversion(
                      convertToProviderExpression: t => t.AssemblyQualifiedName,
                      convertFromProviderExpression: t => Type.GetType(t));
            });
        }

        /// <summary>
        /// Saves all changes made in this context to the database.
        /// This method will automatically call Microsoft.EntityFrameworkCore.ChangeTracking.ChangeTracker.DetectChanges
        /// to discover any changes to entity instances before saving to the underlying database and log it.
        /// </summary>
        /// <param name="user">String to identify who did the action.</param>
        /// <returns> A task that represents the asynchronous save operation. The task result contains the number of state entries written to the database.</returns>
        public async Task<int> SaveChangesAsync(string user)
        {
            await new LogControl(this, user).AddLogsAsync(ChangeTracker
                 .Entries()
                 .Where(t =>
                     t.State == EntityState.Modified ||
                     t.State == EntityState.Deleted ||
                     t.State == EntityState.Added)
                 .ToList().AsReadOnly());
            return await base.SaveChangesAsync();
        }
    }
}

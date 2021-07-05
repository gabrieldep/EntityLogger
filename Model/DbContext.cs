using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using AppLogger.Controls;
using System.Diagnostics.CodeAnalysis;
using System.Collections.Generic;

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
            IReadOnlyCollection<EntityEntry> entries = ChangeTracker
                 .Entries()
                 .Where(t =>
                     t.State == EntityState.Modified ||
                     t.State == EntityState.Deleted ||
                     t.State == EntityState.Added)
                 .ToList().AsReadOnly();
            await new LogControl(this, user).AddLogsAsync(entries);
            return await base.SaveChangesAsync();
        }

        /// <summary>
        /// Pegar chave estrangeira
        /// </summary>
        /// <param name="obj">Object to get the foreing key.</param>
        public int GetForeingKey(object obj)
        {
            return (int)obj.GetType()
                .GetProperty(GetForeingKeyName(obj))
                .GetValue(obj, null);
        }

        /// <summary>
        /// Pegar nome da chave estrangeira
        /// </summary>
        /// <param name="obj">Object to get the foreing key name.</param>
        public string GetForeingKeyName(object obj)
        {
            return Model
                .FindEntityType(obj.GetType())
                .FindPrimaryKey().Properties
                .Select(x => x.Name)
                .Single();
        }
    }
}

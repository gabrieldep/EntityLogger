﻿using Microsoft.EntityFrameworkCore;
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
            var entries = ChangeTracker
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
        /// Salva alterações de Criação de entidades e Loga, com informacoes de DataHora e quem realizou a operação.
        /// This method will automatically call Microsoft.EntityFrameworkCore.ChangeTracking.ChangeTracker.DetectChanges
        /// to discover any changes to entity instances before saving to the underlying database and log it.
        /// </summary>
        /// <param name="user">String to identify who did the action.</param>
        /// <returns> A task that represents the asynchronous save operation. The task result contains the number of state entries written to the database.</returns>
        public async Task<int> SaveChangesCreateAsync(string user)
        {
            var entries = ChangeTracker
                 .Entries()
                 .Where(t =>
                     t.State == EntityState.Modified ||
                     t.State == EntityState.Deleted ||
                     t.State == EntityState.Added)
                 .ToList().AsReadOnly();
            if (entries.Any(e => e.State != EntityState.Added))
                return 0;
            int i = await base.SaveChangesAsync();
            foreach (var item in entries)
                item.State = EntityState.Added;
            await new LogControl(this, user).AddLogsAsync(entries);
            foreach (var item in entries)
                item.State = EntityState.Unchanged;
            return await base.SaveChangesAsync();
        }

        /// <summary>
        /// Salva alterações de entidades e Loga apenas as entidades solicitadas, com informacoes de DataHora e quem realizou a operação.
        /// This method will automatically call Microsoft.EntityFrameworkCore.ChangeTracking.ChangeTracker.DetectChanges
        /// to discover any changes to entity instances before saving to the underlying database and log it.
        /// </summary>
        /// <param name="user">String to identify who did the action.</param>
        /// <param name="objetos">Objetos a serem logados.</param>
        /// <returns> A task that represents the asynchronous save operation. The task result contains the number of state entries written to the database.</returns>

        public async Task<int> SaveChangesAsync(string user, params object[] objetos)
        {
            var entries = ChangeTracker
                 .Entries()
                 .Where(t =>
                     t.State == EntityState.Modified ||
                     t.State == EntityState.Deleted ||
                     t.State == EntityState.Added)
                 .ToList().AsReadOnly();
            var entrysLog = new List<EntityEntry>();
            foreach (var item in entries)
                foreach (var obj in objetos)
                    if (EntityLoggerControl.Equals(item.CurrentValues.ToObject(), obj))
                        entrysLog.Add(item);

            await new LogControl(this, user).AddLogsAsync(entrysLog);
            return await base.SaveChangesAsync();
        }

        /// <summary>
        /// Get the foreign key from an object.
        /// </summary>
        /// <param name="obj">Object to get the foreing key.</param>
        public int GetForeingKey(object obj) => (int)obj.GetType()
                .GetProperty(GetForeingKeyName(obj.GetType()))
                .GetValue(obj, null);

        /// <summary>
        /// Get foreign key name from an object.
        /// </summary>
        /// <param name="obj">Object to get the foreing key name.</param>
        public string GetForeingKeyName(Type type) => Model
                .FindEntityType(type)
                .FindPrimaryKey().Properties
                .Select(p => p.Name)
                .SingleOrDefault();
    }
}

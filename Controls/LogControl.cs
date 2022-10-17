using AppLogger.Exceptions;
using AppLogger.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace AppLogger.Controls
{
    public class LogControl
    {
        private readonly Model.DbContext _context;
        private readonly string _user;

        public LogControl(Model.DbContext context, string user)
        {
            _context = context;
            _user = user;
        }

        /// <summary>
        /// Adds logs of editing, creating or deleting entities.
        /// </summary>
        /// <param name="entityEntries">EntityEntry list with change tracking information.</param>
        public async Task AddLogsAsync(IEnumerable<EntityEntry> entityEntries)
        {
            foreach (EntityEntry entityEntry in entityEntries)
                await AddLogAsync(entityEntry);
        }

        /// <summary>
        /// Adds log of editing, creating or deleting an entity.
        /// </summary>
        /// <param name="changeInfo">EntityEntry with change tracking information.</param>
        public async Task AddLogAsync(EntityEntry changeInfo)
        {
            var oldObj = GetOldObject(changeInfo);
            var newObj = GetNewObject(changeInfo);

            var log = new LogBase()
            {
                EntityLogState = changeInfo.State,
                DateTime = DateTime.Now,
                EntityType = changeInfo.Entity.GetType(),
                User = _user,
                EntitiesAttributes = GetListAttributes(newObj, oldObj ?? null).ToList(),
                ForeignKey = _context.GetForeingKey(newObj ?? oldObj)
            };
            await _context.LogsBase.AddAsync(log);
        }

        /// <summary>
        /// Get the properties from objects
        /// </summary>
        /// <param name="objects">Array wiht objects to get the EntityAttributes.</param>
        /// <exception cref="DifferentObjectsTypeException">If the objects array have objects from different types.</exception>
        /// <returns>Attributes list from all objects passed as parameter.</returns>
        public static IEnumerable<EntityAttribute> GetListAttributes(params object[] objects)
        {
            var type = objects?.First().GetType();
            if (objects.Any(o => o.GetType() != type))
                throw new DifferentObjectsTypeException("There are objects with different types in the array.", new("Object does not match target type."));

            var properties = GetSystemsProperties(type);
            return objects.SelectMany(obj => properties
                    .Select(p => new EntityAttribute
                    {
                        EntityType = Enums.EntityType.Old,
                        Type = p.PropertyType,
                        PropertyName = p.Name,
                        Value = p.GetValue(obj)?.ToString()
                    }));
        }

        /// <summary>
        /// Get the object before the db operation
        /// </summary>
        /// <param name="entityEntry">EntityEntry with the dbOperation.</param>
        internal static object GetOldObject(EntityEntry entityEntry) => entityEntry.State != EntityState.Added ? entityEntry.GetDatabaseValues().ToObject() : null;

        /// <summary>
        /// Get the object after the db operation
        /// </summary>
        /// <param name="entityEntry">EntityEntry with the dbOperation.</param>
        internal static object GetNewObject(EntityEntry entityEntry) => entityEntry.CurrentValues.ToObject();

        /// <summary>
        /// Get all native attributes from a type.
        /// </summary>
        /// <param name="type">Type to get the attributes.</param>
        internal static IEnumerable<PropertyInfo> GetSystemsProperties(Type type) => type.GetProperties().Where(p => p.PropertyType.Namespace == "System");
    }
}

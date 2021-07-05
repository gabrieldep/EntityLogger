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

        public LogControl(Model.DbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Adds logs of editing, creating or deleting entities.
        /// </summary>
        /// <param name="entityEntries">EntityEntry list with change tracking information.</param>
        public async Task AddLogsAsync(IEnumerable<EntityEntry> entityEntries)
        {
            foreach (EntityEntry entityEntry in entityEntries)
            {
                await AddLogAsync(entityEntry);
            }
        }

        /// <summary>
        /// Adds log of editing, creating or deleting an entity.
        /// </summary>
        /// <param name="changeInfo">EntityEntry with change tracking information.</param>
        public async Task AddLogAsync(EntityEntry changeInfo)
        {
            EntityState logType = changeInfo.State;

            object oldObj = GetOldObject(changeInfo);
            object newObj = GetNewObject(changeInfo);

            Type type = changeInfo.Entity.GetType();

            LogBase log = new()
            {
                EntityLogState = logType,
                DateTime = DateTime.Now,
                EntityType = type,
                User = _user,
                EntitiesAttributes = (oldObj == null ? 
                        GetListAttributes(type, newObj) : GetListAttributes(type, newObj, oldObj)).ToList(),
                ForeignKey = _context.GetForeingKey(newObj ?? oldObj)
            };
            await _context.LogsBase.AddAsync(log);
        }


        /// <summary>
        /// Trata os dados recebidos em forma de entidade e os devolve em formato de um lista de atributos
        /// </summary>
        /// <returns>Retorna uma lista com os atributos antigos e novos das entidades.</returns>
        /// <param name="type">Object type.</param>
        /// <param name="objects">Original entity.</param>
        public static IEnumerable<EntityAttribute> GetListAttributes(Type type, params object[] objects)
        {
            IEnumerable<PropertyInfo> properties = type
                .GetProperties()
                .Where(p => p.PropertyType.Namespace == "System");

            List<EntityAttribute> entitiesAttributes = new();

            foreach (object obj in objects)
            {
                entitiesAttributes.AddRange(properties
                   .Select(p => new EntityAttribute
                   {
                       EntityType = Enums.EntityType.Old,
                       Type = p.PropertyType,
                       PropertyName = p.Name,
                       Value = p.GetValue(obj)?.ToString()
                   }).ToList());
            }
            return entitiesAttributes;
        }

        /// <summary>
        /// Recupera o objeto antes da alteração caso ele exista
        /// </summary>
        /// <param name="entityEntry">EntityEntry com a informação.</param>
        internal static object GetOldObject(EntityEntry entityEntry)
        {
            return entityEntry.State != EntityState.Added ?
                      entityEntry.GetDatabaseValues().ToObject()
                      : null;
        }

        /// <summary>
        /// Recupera o objeto novo
        /// </summary>
        /// <param name="entityEntry">EntityEntry com a informação.</param>
        internal static object GetNewObject(EntityEntry entityEntry)
        {
            return entityEntry.CurrentValues.ToObject();
        }

    }
}

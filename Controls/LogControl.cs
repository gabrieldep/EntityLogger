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
        /// Lista de Logs baseada nos parametros
        /// </summary>
        /// <returns>Retorna um IEnumreable com os logs baseado nos parametros.</returns>
        /// <param name="start">Start date time.</param>
        /// <param name="end">End date time.</param>
        /// <param name="enumEntityState">Enum LogType.</param>
        /// <param name="type">Entity type name.</param>
        public IEnumerable<LogBase> GetLogBaseList(DateTime start, DateTime end, int enumEntityState, string type)
        {
            return _context.LogsBase
                .Include(lb => lb.EntitiesAttributes)
                .Where(lb =>
                    (start == DateTime.MinValue || lb.DateTime >= start)
                    && (end == DateTime.MinValue || lb.DateTime <= end)
                    && (enumEntityState == -1 || lb.EntityLogState == (EntityState)enumEntityState)).ToList()
                    .Where(lb => string.IsNullOrEmpty(type) || lb.EntityType == Type.GetType(type));
        }

        /// <summary>
        /// Lista de Logs de um determinado Objeto do banco de dados
        /// </summary>
        /// <param name="idEntity">Entity Id.</param>
        /// <returns>Retorna um IEnumreable com os logs baseado nos parametros.</returns>
        public IEnumerable<LogBase> GetEntityLogBaseList(int idEntity)
        {
            return _context.LogsBase
                .Include(lb => lb.EntitiesAttributes)
                .Where(lb => lb.ForeignKey == idEntity);
        }

        /// <summary>
        /// Cria o objeto do tipo T a partir do log.
        /// </summary>
        /// <param name="logBase">LogBase to reconstruct the objetct.</param>
        /// <param name="entityType">Enum EntityType.</param>
        /// <param name="objectT">Object to be reconstruct.</param>
        public static void CreateEntity<T>(LogBase logBase, Enums.EntityType entityType, ref T objectT)
        {
            IEnumerable<EntityAttribute> attributes = logBase.EntitiesAttributes
                .Where(a => a.EntityType == entityType);
            foreach (EntityAttribute attribute in attributes)
            {
                objectT
                    .GetType()
                    .GetProperty(attribute.PropertyName)
                    .SetValue(objectT, Convert.ChangeType(attribute.Value, attribute.Type));
            }
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

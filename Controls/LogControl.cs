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

        public LogControl(Model.DbContext contexto, string user)
        {
            _context = contexto;
            _user = user;
        }

        public LogControl(Model.DbContext contexto)
        {
            _context = contexto;
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
            object oldObj =
                logType != EntityState.Added ?
                    changeInfo.GetDatabaseValues().ToObject()
                    : null;
            object newObj = changeInfo.CurrentValues.ToObject();
            Type type = changeInfo.Entity.GetType();

            LogBase log = new()
            {
                EntityLogState = logType,
                DateTime = DateTime.Now,
                EntityType = type,
                User = _user,
                EntitiesAttributes = GetListAttributes(oldObj, newObj, type, logType).ToList()
            };
            await _context.LogsBase.AddAsync(log);
        }


        /// <summary>
        /// Trata os dados recebidos em forma de entidade e os devolve em formato de um lista de atributos
        /// </summary>
        /// <returns>Retorna uma lista com os atributos antigos e novos das entidades.</returns>
        /// <param name="oldObj">Original entity.</param>
        /// <param name="newObj">New entity.</param>
        /// <param name="type">Object type.</param>
        /// <param name="logType">Enum LogType.</param>
        public static IEnumerable<EntityAttribute> GetListAttributes(object oldObj, object newObj, Type type, EntityState entityState)
        {
            IEnumerable<PropertyInfo> properties = type
                .GetProperties()
                .Where(p => p.PropertyType.Namespace == "System");

            IEnumerable<EntityAttribute> EntitiesAttributes = entityState != EntityState.Added ?
                properties
                    .Select(p => new EntityAttribute
                    {
                        EntityType = Enums.EntityType.Old,
                        Type = p.PropertyType,
                        PropertyName = p.Name,
                        Value = p.GetValue(oldObj)?.ToString()
                    }).ToList() : new List<EntityAttribute>();

            return entityState == EntityState.Deleted ?
                EntitiesAttributes :
                    EntitiesAttributes.Union(properties
                        .Select(p => new EntityAttribute
                        {
                            EntityType = Enums.EntityType.New,
                            Type = p.PropertyType,
                            PropertyName = p.Name,
                            Value = p.GetValue(newObj)?.ToString()
                        })).ToList();
        }

        /// <summary>
        /// Lista de Logs baseada nos parametros
        /// </summary>
        /// <returns>Retorna um IEnumreable com os logs baseado nos parametros.</returns>
        /// <param name="start">Start date time.</param>
        /// <param name="end">End date time.</param>
        /// <param name="enumTipoLog">Enum LogType.</param>
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
        /// <param name="type">Entity type name.</param>
        /// <param name="idEntity">Entity Id.</param>
        /// <returns>Retorna um IEnumreable com os logs baseado nos parametros.</returns>
        public IEnumerable<LogBase> GetEntityLogBaseList(string type, int idEntity)
        {
            return _context.LogsBase
                .Include(lb => lb.EntitiesAttributes)
                .Where(lb => lb.EntitiesAttributes.Any(a =>
                    a.PropertyName == "Id" || a.PropertyName == "Id" + type
                    && a.Value == idEntity.ToString()));
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
    }
}

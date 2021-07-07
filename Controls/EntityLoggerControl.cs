using AppLogger.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AppLogger.Controls
{
    public class EntityLoggerControl
    {
        private readonly Model.DbContext _context;

        public EntityLoggerControl(Model.DbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Lista de Logs baseada nos parametros
        /// </summary>
        /// <returns>Retorna um IEnumreable com os logs baseado nos parametros.</returns>
        /// <param name="start">Start date time.</param>
        /// <param name="end">End date time.</param>
        /// <param name="enumEntityState">Enum LogType.</param>
        /// <param name="type">Entity type.</param>
        public IEnumerable<LogBase> GetLogBaseList(DateTime start, DateTime end, int enumEntityState, Type type)
        {
            return _context.LogsBase
                .Include(lb => lb.EntitiesAttributes)
                .Where(lb =>
                    (start == DateTime.MinValue || lb.DateTime >= start)
                    && (end == DateTime.MinValue || lb.DateTime <= end)
                    && (enumEntityState == -1 || lb.EntityLogState == (EntityState)enumEntityState)).ToList()
                    .Where(lb => type == null || lb.EntityType == type);
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
    }
}

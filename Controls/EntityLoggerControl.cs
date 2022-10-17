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
        /// Get a log list filtered by parameters
        /// </summary>
        /// <param name="start">Start date time.</param>
        /// <param name="end">End date time.</param>
        /// <param name="enumEntityState">Enum LogType.</param>
        /// <param name="type">Entity type.</param>
        /// <returns>An IEnumerable with logs.</returns>
        public IEnumerable<LogBase> GetLogBaseList(DateTime start, DateTime end, EntityState? enumEntityState, Type type)
        {
            return _context.LogsBase
                .Include(lb => lb.EntitiesAttributes)
                .Where(lb =>
                    (start == DateTime.MinValue || lb.DateTime >= start)
                    && (end == DateTime.MinValue || lb.DateTime <= end)
                    && (enumEntityState == null || lb.EntityLogState == enumEntityState)).ToList()
                    .Where(lb => type == null || lb.EntityType == type);
        }

        /// <summary>
        /// Get a log list from an object
        /// </summary>
        /// <param name="idEntity">Entity Id.</param>
        /// <returns>An IEnumerable with logs.</returns>
        public IEnumerable<LogBase> GetEntityLogBaseList(int idEntity, Type type) => _context.LogsBase
                .Include(lb => lb.EntitiesAttributes)
                .Where(lb => lb.ForeignKey == idEntity)
                .ToList().Where(lb => lb.EntityType == type);

        /// <summary>
        /// Compare two objects
        /// </summary>
        /// <returns>True if the objects are equal, otherwise, returns false.</returns>

        public static new bool Equals(object primeiro, object segundo)
        {
            if (primeiro.GetType() != segundo.GetType())
                return false;

            var propriedades = LogControl.GetSystemsProperties(primeiro.GetType());
            foreach (var property in propriedades)
            {
                var i = property.GetValue(primeiro);
                var j = property.GetValue(segundo);
                if (i.ToString() != j.ToString())
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Get last log from an entity.
        /// </summary>
        /// <param name="idEntity">Entity Id.</param>
        /// <returns>Last log from this object.</returns>
        public LogBase GetLastEntityLogBase(int idEntity, Type type) => _context.LogsBase
            .Where(lb => lb.ForeignKey == idEntity)
            .ToList()?.Last(lb => lb.EntityType == type);
    }
}

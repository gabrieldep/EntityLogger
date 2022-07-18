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
        /// Lista de Logs de um determinado Objeto do banco de dados
        /// </summary>
        /// <param name="idEntity">Entity Id.</param>
        /// <returns>Retorna um IEnumreable com os logs baseado nos parametros.</returns>
        public IEnumerable<LogBase> GetEntityLogBaseList(int idEntity, Type type) => _context.LogsBase
                .Include(lb => lb.EntitiesAttributes)
                .Where(lb => lb.ForeignKey == idEntity)
                .ToList().Where(lb => lb.EntityType == type);

        /// <summary>
        /// Compara dois objetos para verificar se os atributos são identicos;
        /// </summary>
        /// <returns>Retorna true caso os objetos sejam iguais e false caso sejam diferentes.</returns>

        public static new bool Equals(object primeiro, object segundo)
        {
            if (primeiro.GetType() != segundo.GetType())
                return false;

            var propriedades = LogControl.GetSystemsProperties(primeiro.GetType());
            foreach (var property in propriedades)
            {
                object i = property.GetValue(primeiro);
                object j = property.GetValue(segundo);
                if (i.ToString() != j.ToString())
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Lista do ultimo log de um determinado Objeto do banco de dados
        /// </summary>
        /// <param name="idEntity">Entity Id.</param>
        /// <returns>Retorna um IEnumreable com os logs baseado nos parametros.</returns>
        public LogBase GetLastEntityLogBase(int idEntity, Type type) => _context.LogsBase
            .Where(lb => lb.ForeignKey == idEntity)
            .ToList()?.Last(lb => lb.EntityType == type);
    }
}

﻿using AppLogger.Model;
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
        public IEnumerable<LogBase> GetEntityLogBaseList(int idEntity, Type type)
        {
            return _context.LogsBase
                .Include(lb => lb.EntitiesAttributes)
                .Where(lb => lb.ForeignKey == idEntity)
                .ToList().Where(lb => lb.EntityType == type);
        }

        /// <summary>
        /// Lista do ultimo log de um determinado Objeto do banco de dados
        /// </summary>
        /// <param name="idEntity">Entity Id.</param>
        /// <returns>Retorna um IEnumreable com os logs baseado nos parametros.</returns>
        public LogBase GetLastEntityLogBase(int idEntity, Type type)
        {
            return _context.LogsBase
                .Where(lb => lb.ForeignKey == idEntity)
                .ToList().Last(lb => lb.EntityType == type);
        }
    }
}

using AppLogger.Interfaces;
using AppLogger.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AppLogger.Model
{
    public class LogBase : ILogBase
    {
        public LogBase()
        {
            EntitiesAttributes = new HashSet<EntityAttribute>();
        }
        public int Id { get; set; }

        public DateTime DateTime { get; set; }
        public string User { get; set; }
        public int ForeignKey { get; set; }
        public Type EntityType { get; set; }
        public EntityState EntityLogState { get; set; }

        public ICollection<EntityAttribute> EntitiesAttributes { get; set; }

        /// <summary>
        /// Cria o objeto do tipo T a partir do log.
        /// </summary>
        /// <param name="entityType">Enum EntityType.</param>
        public T CreateEntity<T>(Enums.EntityType entityType) where T : new()
        {
            IEnumerable<EntityAttribute> attributes = EntitiesAttributes
               .Where(a => a.EntityType == entityType);
            T objectT = new();
            foreach (EntityAttribute attribute in attributes)
            {
                objectT.GetType().GetProperty(attribute.PropertyName)
                    .SetValue(objectT, Convert.ChangeType(attribute.Value, attribute.Type));
            }
            return objectT;
        }
    }
}

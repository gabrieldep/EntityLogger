using AppLogger.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;

namespace AppLogger.Model
{
    public class LogBase
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
    }
}

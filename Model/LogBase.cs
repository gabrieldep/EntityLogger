using AppLogger.Model;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace AppLogger.Model
{
    public class LogBase
    {
        public LogBase()
        {
            EntitiesAttributes = new HashSet<EntityAttribute>();
        }
        public int Id { get; set; }

        public DateTime DataHora { get; set; }
        public string IdIdentity { get; set; }
        public string EmailIdentity { get; set; }

        public ICollection<EntityAttribute> EntitiesAttributes { get; set; }

        public Type EntityType { get; set; }

        public Enums.LogType TipoLog { get; set; }
    }   
}

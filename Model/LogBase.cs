﻿using AppLogger.Model;
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

        public DateTime DataHora { get; set; }
        public string User { get; set; }

        public ICollection<EntityAttribute> EntitiesAttributes { get; set; }

        public Type EntityType { get; set; }

        public Enums.LogType TipoLog { get; set; }
    }   
}

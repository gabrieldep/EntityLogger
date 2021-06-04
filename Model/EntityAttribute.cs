using System;

namespace AppLogger.Model
{
    public class EntityAttribute
    {
        public int Id { get; set; }

        public Enums.EntityType EntityType { get; set; }

        public Type Type { get; set; }
        public string PropertyName { get; set; }
        public string Value { get; set; }

        public int IdLogBase { get; set; }
        public LogBase LogBase { get; set; }
    }
}

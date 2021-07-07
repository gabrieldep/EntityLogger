using System;
using System.Runtime.Serialization;

namespace AppLogger.Exceptions
{
    public class DifferentObjectsTypeException : Exception
    {
        public DifferentObjectsTypeException() : base() { }
        public DifferentObjectsTypeException(string message) : base(message) { }
        public DifferentObjectsTypeException(string message, Exception inner) : base(message, inner) { }

        protected DifferentObjectsTypeException(SerializationInfo info, StreamingContext context) : base(info, context) { }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

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

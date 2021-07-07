using AppLogger.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppLogger.Interfaces
{
    public interface ILogBase
    {
        T CreateEntity<T>(Enums.EntityType entityType) where T : new();
    }
}

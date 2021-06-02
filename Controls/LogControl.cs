using AppLogger.Model;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace AppLogger.Controls
{
    class LogControl
    {
        private readonly Model.DbContext _context;
        private readonly string _user;

        public LogControl(Model.DbContext contexto, string user)
        {
            _context = contexto;
            _user = user;
        }

        public LogControl(Model.DbContext contexto)
        {
            _context = contexto;
        }

        /// <summary>
        /// Adiciona Log de edição, criação ou deleção de uma entidade
        /// </summary>
        /// <param name="tipoLog">Define se o log é de edição, criação ou deleção.</param>
        /// <param name="changeInfo">Variavel que contém informações sobre a alteração.</param>
        public async Task AddLogAsync(Enums.LogType tipoLog, EntityEntry changeInfo)
        {
            object antigo =
                tipoLog != Enums.LogType.Create ?
                    changeInfo.GetDatabaseValues().ToObject()
                    : null;
            object novo = changeInfo.CurrentValues.ToObject();
            Type type = changeInfo.Entity.GetType();

            LogBase log = new()
            {
                TipoLog = tipoLog,
                DataHora = DateTime.Now,
                EntityType = type,
                EntitiesAttributes = GetListAttributes(antigo, novo, type, tipoLog).ToList()
            };
            await _context.LogsBase.AddAsync(log);
        }

        /// <summary>
        /// Trata os dados recebidos em forma de entidade e os devolve em formato de um lista de atributos
        /// </summary>
        /// <returns>Retorna uma lista com os atributos antigos e novos das entidades.</returns>
        /// <param name="antigo">Entidade original.</param>
        /// <param name="novo">Entidade nova.</param>
        public IEnumerable<EntityAttribute> GetListAttributes(object oldObj, object newObj, Type type, Enums.LogType logType)
        {
            IEnumerable<PropertyInfo> properties = type
                .GetProperties()
                .Where(p => p.PropertyType.Namespace == "System");

            IList<EntityAttribute> AtributosEntidadeLogNovo = logType != Enums.LogType.Create ?
                properties
                    .Select(p => new EntityAttribute
                    {
                        TipoEntidade = Enums.EntityType.Old,
                        Type = p.PropertyType,
                        PropertyName = p.Name,
                        Value = p.GetValue(oldObj)?.ToString()
                    }).ToList() : new List<EntityAttribute>();

            return logType == Enums.LogType.Delete ?
                AtributosEntidadeLogNovo :
                    AtributosEntidadeLogNovo.Union(properties
                        .Select(p => new EntityAttribute
                        {
                            TipoEntidade = Enums.EntityType.New,
                            Type = p.PropertyType,
                            PropertyName = p.Name,
                            Value = p.GetValue(newObj)?.ToString()
                        })).ToList();
        }

        /// <summary>
        /// Lista de Logs baseada nos parametros
        /// </summary>
        /// <returns>Retorna um IEnumreable com os logs baseado nos parametros.</returns>
        /// <param name="inicio">Data inicial para o registro do log.</param>
        /// <param name="fim">Data final para o registro do log.</param>
        /// <param name="idIdentity">IdIdentity de quem realizou a ação do Log.</param>
        /// <param name="enumTipoLog">Enum.Tipolog determinando se foi edição, criação ou deleção.</param>
        /// <param name="type">Tipo do objeto que foi alterado.</param>
        public IEnumerable<LogBase> GetLogBaseList(DateTime inicio, DateTime fim, int enumTipoLog, string type, string user)
        {
            return _context.LogsBase
                .Include(lb => lb.EntitiesAttributes)
                .Where(lb =>
                    (inicio == DateTime.MinValue || lb.DataHora >= inicio)
                    && (fim == DateTime.MinValue || lb.DataHora <= fim)
                    && (enumTipoLog == -1 || lb.TipoLog == (Enums.LogType)enumTipoLog)).ToList()
                    .Where(lb => string.IsNullOrEmpty(type) ? true : lb.EntityType == Type.GetType(type));
        }

        /// <summary>
        /// Lista de Logs de um determinado Objeto do banco de dados
        /// </summary>
        /// <returns>Retorna um IEnumreable com os logs baseado nos parametros.</returns>
        /// <param name="type">Tipo do objeto que foi alterado.</param>
        /// <param name="idClinica">Id da clinica desse objeto.</param>
        /// <param name="idEntity">Id da entidade que se quer os logs.</param>
        public IEnumerable<LogBase> GetEntityLogBaseList(string type, int idEntity)
        {
            return _context.LogsBase
                .Include(lb => lb.EntitiesAttributes)
                .Where(lb => lb.EntitiesAttributes.Any(a => 
                    a.PropertyName == "Id" || a.PropertyName == "Id" + type 
                    && a.Value == idEntity.ToString()))
                        .ToList()
                        .Where(lb =>
                            string.IsNullOrEmpty(type) || lb.EntityType == Type.GetType(type));
        }

        /// <summary>
        /// Cria o objeto do tipo T a partir do log.
        /// </summary>
        /// <param name="logBase">Logbase no qual deve se pegar os atributos para construir o objeto.</param>
        /// <param name="tipoEntidade">Enum para saber se objeto solicitado é o novo ou o antigo.</param>
        /// <param name="objeto">Objeto a ser construido passado por referencia.</param>
        public static T CreateEntity<T>(LogBase logBase, Enums.EntityType tipoEntidade, T objeto)
        {
            List<EntityAttribute> atributos = logBase.EntitiesAttributes
                .Where(a => a.TipoEntidade == tipoEntidade)
                .ToList();
            foreach (var atributo in atributos)
            {
                objeto
                    .GetType()
                    .GetProperty(atributo.PropertyName)
                    .SetValue(objeto, Convert.ChangeType(atributo.Value, atributo.Type));
            }

            return objeto;
        }
    }
}

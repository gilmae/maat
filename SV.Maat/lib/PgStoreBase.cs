using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using Dapper;
using Events;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace SV.Maat.lib
{
    public class PgStoreBase : IEventStore
    {
        string _connectionString;
        List<Type> _validTypes;


        public PgStoreBase(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("maat");
            _validTypes = AppDomain.CurrentDomain.GetAssemblies()
                                   .SelectMany(x => x.GetTypes())
                                   .Where(x => x.IsSubclassOf(typeof(Event))).ToList();
        }

        public int? GetCurrentVersion(Guid id)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                return conn.Query<int?>("select max(Version) from Events where aggregate_id = @id", new { id }).FirstOrDefault();
            }
        }

        private Event Deserialise(dynamic data)
        {
            Type event_type = _validTypes.FirstOrDefault(t => t.Name == data.event_type);
            if (event_type != null)
            {
                var e = System.Text.Json.JsonSerializer.Deserialize(data.body, event_type) as Event;
                e.Id = data.id;
                e.Version = data.version;
                return e;
            }
            return null;
        }

        public long StoreEvent(Event e)
        {
            Type aggregateType = e.AggregateType();
            string _type = aggregateType.Name;
            string sql = $"insert into Events (aggregate_id, type, event_type, body, version) values(@aggregate_id, @type, @event_type, @body, @version)";
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                    string body = System.Text.Json.JsonSerializer.Serialize(e as object);
                    string eventType = e.GetType().Name;
                    conn.Execute(sql, new { aggregate_id = e.AggregateId, type = _type, event_type = eventType, body, e.Version });
            }

            return e.Version;
        }


        public IList<Event> Retrieve()
        {
            string sql = $"select id, aggregate_id, type, body, event_type, version from Events";
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();

                IEnumerable<dynamic> bodies = conn.Query<dynamic>(sql);

                return bodies.Select(Deserialise).OrderBy(e => e.AggregateId).OrderBy(e => e.Version).ToList();
            }
        }

        public IList<Event> Retrieve(EventScope scope)
        {
            StringBuilder sql = new StringBuilder($"select id, aggregate_id, type, body, event_type, version from Events");
            IList<string> filters = new List<string>();
            dynamic sqlparams = new ExpandoObject();

            if (scope.After.HasValue)
            {
                filters.Add("id > @id");
                sqlparams.id = scope.After.Value;
            }

            if (scope.AggregateId.HasValue)
            {
                filters.Add("aggregate_id = @aggregate_id");
                sqlparams.aggregate_id = scope.AggregateId.Value;
            }

            if (scope.AggregateType != null)
            {
                filters.Add("type = @type");
                sqlparams.type = scope.AggregateType.Name;
            }

            if (filters.Count > 0)
            {
                sql.Append(" where ").Append(string.Join(" and ", filters));
            }


            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();

                IEnumerable<dynamic> bodies = conn.Query<dynamic>(sql.ToString(), (object) sqlparams);

                return bodies.Select(Deserialise).OrderBy(e => e.AggregateId).OrderBy(e => e.Version).ToList();
            }
        }


        //public long StoreEvent(IList<Event<T>> events)
        //{

        //    string sql = $"insert into Events (aggregate_id, type, event_type, body, version) values(@aggregate_id, @type, @event_type, @body, @version)";
        //    using (var conn = new NpgsqlConnection(_connectionString))
        //    {
        //        conn.Open();
        //        foreach (Event<T> e in events)
        //        {
        //            string body = System.Text.Json.JsonSerializer.Serialize(e as object);
        //            string eventType = e.GetType().Name;
        //            conn.Execute(sql, new { aggregate_id = e.AggregateId, type = _type, event_type = eventType, body, e.Version });
        //        }
        //    }

        //    return events.Last().Version;
        //}
    }
}

﻿using System;
using System.Collections.Generic;
using Events;
using Npgsql;
using Dapper;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace SV.Maat
{
    public class PgStore<T> : IEventStore<T> where T : Aggregate
    {
        string _connectionString;
        readonly string _type = typeof(T).Name;
        List<Type> _validTypes;


        public PgStore(IConfiguration configuration)
        {
            _connectionString = Environment.GetEnvironmentVariable("MAATCONNSTR");
            _validTypes = AppDomain.CurrentDomain.GetAssemblies()
                                   .SelectMany(x => x.GetTypes())
                                   .Where(x => x.IsSubclassOf(typeof(Event<T>))).ToList();
        }

        public int? GetCurrentVersion(Guid id)
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                return conn.Query<int?>("select max(Version) from Events where aggregate_id = @id and type=@type", new { id, type = _type }).FirstOrDefault();
            }
        }

        public IList<Event<T>> Retrieve(Guid id)
        {
            string sql = $"select body, event_type from Events where aggregate_id = @id and type=@type";
            using (var conn = new NpgsqlConnection(_connectionString)) {
                conn.Open();

                IEnumerable<dynamic> bodies = conn.Query<dynamic>(sql, new { id, type = _type });

                return bodies.Select(Deserialise).OrderBy(e=>e.Version).ToList();
            }
        }

        public IList<Event<T>> Retrieve()
        {
            string sql = $"select body, event_type from Events where type=@type";
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();

                IEnumerable<dynamic> bodies = conn.Query<dynamic>(sql, new { type = _type });

                return bodies.Select(Deserialise).OrderBy(e => e.AggregateId).OrderBy(e => e.Version).ToList();
            }
        }

        private Event<T> Deserialise(dynamic data)
        {
            Type event_type = _validTypes.FirstOrDefault(t => t.Name == data.event_type);
            if (event_type != null)
            {
                return System.Text.Json.JsonSerializer.Deserialize(data.body, event_type);
            }
            return null;
        }
    

        public long StoreEvent(Event<T> e)
        {
            return StoreEvent(new[] { e });
        }

        public long StoreEvent(IList<Event<T>> events)
        {
            
            string sql = $"insert into Events (aggregate_id, type, event_type, body, version) values(@aggregate_id, @type, @event_type, @body, @version)";
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                foreach (Event<T> e in events)
                {
                    string body = System.Text.Json.JsonSerializer.Serialize(e as object);
                    string eventType = e.GetType().Name;
                    conn.Execute(sql, new { aggregate_id = e.AggregateId, type = _type, event_type = eventType, body, e.Version });
                }
            }

            return events.Last().Version;
        }
    }
}
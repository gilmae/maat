using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

using mf;
using Newtonsoft.Json.Linq;

namespace StrangeVanilla.Blogging.Events
{
    public class MicroformatPropertiesSerialiser : JsonConverter<Dictionary<string, object[]>>
    {
        public override Dictionary<string, object[]> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException($"JsonTokenType was of type {reader.TokenType}, only objects are supported");
            }

            var dictionary = new Dictionary<string, object[]>();
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    return dictionary;
                }

                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    throw new JsonException("JsonTokenType was not PropertyName");
                }

                var propertyName = reader.GetString();

                if (string.IsNullOrWhiteSpace(propertyName))
                {
                    throw new JsonException("Failed to get property name");
                }

                reader.Read();

                var val = ExtractValue(ref reader, options);
                if (val is List<object>)
                {
                    
                    dictionary.Add(propertyName, (val as List<object>).ToArray());
                } else
                {
                    dictionary.Add(propertyName, new[] { val });
                }
                
            }

            return dictionary;
        }

        private object ExtractValue(ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.String:
                    if (reader.TryGetDateTime(out var date))
                    {
                        return date;
                    }
                    return reader.GetString();
                case JsonTokenType.False:
                    return false;
                case JsonTokenType.True:
                    return true;
                case JsonTokenType.Null:
                    return null;
                case JsonTokenType.Number:
                    if (reader.TryGetInt64(out var result))
                    {
                        return result;
                    }
                    return reader.GetDecimal();
                case JsonTokenType.StartObject:
                    return ReadObject(ref reader, options);
                case JsonTokenType.StartArray:
                    var list = new List<object>();
                    while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
                    {
                        list.Add(ExtractValue(ref reader, options));
                    }
                    return list;
                default:
                    throw new JsonException($"'{reader.TokenType}' is not supported");
            }
        }

        public Dictionary<string,object> ReadObject(ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException($"JsonTokenType was of type {reader.TokenType}, only objects are supported");
            }

            var dictionary = new Dictionary<string, object>();
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    return dictionary;
                }

                if (reader.TokenType != JsonTokenType.PropertyName)
                {
                    throw new JsonException("JsonTokenType was not PropertyName");
                }

                var propertyName = reader.GetString();

                if (string.IsNullOrWhiteSpace(propertyName))
                {
                    throw new JsonException("Failed to get property name");
                }

                reader.Read();

                dictionary.Add(propertyName, ExtractValue(ref reader, options));
            }

            return dictionary;
        }
    
        public override void Write(Utf8JsonWriter writer, Dictionary<string, object[]> dictionary, JsonSerializerOptions options)
        {
            JsonSerializer.Serialize(writer, dictionary, options);
            //writer.WriteStartObject();

            //foreach ((string key, object[] value) in dictionary)
            //{
            //    var propertyName = key.ToString();
            //    writer.WritePropertyName
            //        (options.PropertyNamingPolicy?.ConvertName(propertyName) ?? propertyName);

            //    writer.WriteStartArray();
            //    foreach (var v in value)
            //    {
            //        if (v is string)
            //        {
            //            writer.WriteStringValue(v as string);
            //        }
            //        else if (v is JObject)
            //        {
            //            // Because Newtonsoft interprets objects (i.e. {k:v,k1,v1}mixed with strings as [[k,v][k1,v1]] because why not
            //            writer.WriteStartObject();
            //            JObject dict = v as JObject;

            //            foreach (var k in dict)
            //            {
            //                writer.WritePropertyName(k.Key);
            //                var thefuckingvalue = (k.Value as JValue).Value as string;
            //                writer.WriteStringValue(thefuckingvalue);
            //            }

            //            writer.WriteEndObject();
            //        }
            //        else
            //        {
            //            JsonSerializer.Serialize(writer, v, options);
            //        }
            //    }
            //    writer.WriteEndArray();
            //}

            //writer.WriteEndObject();
        }

    }
}

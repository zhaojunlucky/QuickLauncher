using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace QuickLauncher.Misc
{
    public class JsonListConverterFactoryForListOfT : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert)
        {
            return typeToConvert.IsGenericType &&
               typeToConvert.GetGenericTypeDefinition() == typeof(List<>);
        }

        public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
        {
            Debug.Assert(typeToConvert.IsGenericType &&
               typeToConvert.GetGenericTypeDefinition() == typeof(List<>));

            Type elementType = typeToConvert.GetGenericArguments()[0];

            JsonConverter converter = (JsonConverter)Activator.CreateInstance(
                typeof(JsonListConverter<>)
                    .MakeGenericType(new Type[] { elementType }),
                BindingFlags.Instance | BindingFlags.Public,
                binder: null,
                args: null,
                culture: null)!;

            return converter;
        }
    }

    public class JsonListConverter<T> : JsonConverter<List<T>>
    {
        public override List<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartArray || !reader.Read())
            {
                throw new JsonException();
            }
            var value = new List<T>();
            while (reader.TokenType != JsonTokenType.EndArray)
            {
                if (reader.TokenType == JsonTokenType.EndArray)
                {
                    return value;
                }

                value.Add(JsonSerializer.Deserialize<T>(ref reader, options));

                if (!reader.Read())
                {
                    throw new JsonException();
                }

            }
            return value;
        }

        public override void Write(Utf8JsonWriter writer, List<T> value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }
    }
}

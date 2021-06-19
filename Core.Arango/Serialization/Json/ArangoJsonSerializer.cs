using System;
using System.Text.Json;

namespace Core.Arango.Serialization.Json
{
    /// <summary>
    ///     Arango Json Serializer with System.Json.Text
    /// </summary>
    public class ArangoJsonSerializer : IArangoSerializer
    {
        private readonly JsonSerializerOptions _options;

        /// <summary>
        /// </summary>
        /// <param name="policy">PascalCase or camelCase policy</param>
        public ArangoJsonSerializer(JsonNamingPolicy policy)
        {
            _options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
            {
                PropertyNamingPolicy = policy,
                //DictionaryKeyPolicy = policy,
                IgnoreNullValues = false
            };
        }

        /// <inheritdoc />
        public byte[] Serialize(object value)
        {
            return JsonSerializer.SerializeToUtf8Bytes(value, _options);
        }

        /// <inheritdoc />
        public T Deserialize<T>(byte[] value)
        {
            return JsonSerializer.Deserialize<T>(value, _options);
        }

        /// <inheritdoc />
        public object Deserialize(byte[] v, Type t)
        {
            return JsonSerializer.Deserialize(v, t, _options);
        }

        public string ContentType => "application/json";
    }
}
using System;
using ArangoDB.VelocyPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Core.Arango.Serialization.VelocyPack
{
    /// <summary>
    ///     Arango VelocyPack with Newtonsoft
    /// </summary>
    public class ArangoVelocyPackSerializer : IArangoSerializer
    {
        private readonly JsonSerializerSettings _settings;

        /// <summary>
        ///     Arango VelocyPack with Newtonsoft
        /// </summary>
        /// <param name="resolver">PascalCase or camelCaseResolver</param>
        public ArangoVelocyPackSerializer(IContractResolver resolver)
        {
            /*_settings = new JsonSerializerSettings
            {
                ContractResolver = resolver,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Formatting = Formatting.None,
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                DateTimeZoneHandling = DateTimeZoneHandling.Utc
            };*/
        }

        /// <inheritdoc />
        public byte[] Serialize(object value)
        {
            return VPack.Serialize(value);
        }

        /// <inheritdoc />
        public T Deserialize<T>(byte[] value)
        {
            return VPack.Deserialize<T>(value);
        }

        /// <inheritdoc />
        public object Deserialize(byte[] v, Type t)
        {
            return VPack.Deserialize(v, t);
        }

        /// <inheritdoc />
        public string ContentType => "application/x-velocypack";
    }
}
using System;

namespace Core.Arango.Serialization
{
    /// <summary>
    ///     Arango Serializer Interface
    /// </summary>
    public interface IArangoSerializer
    {
        /// <summary>
        ///     Convert object to string
        /// </summary>
        public byte[] Serialize(object value);

        /// <summary>
        ///     Convert string to object
        /// </summary>
        public T Deserialize<T>(byte[] value);

        /// <summary>
        ///     Convert string to object
        /// </summary>
        public object Deserialize(byte[] value, Type type);

        /// <summary>
        ///  json or vpack
        /// </summary>
        public string ContentType { get; }
    }
}
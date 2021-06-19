using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArangoDB.VelocyPack
{
    public class SliceWriter : JsonWriter
    {
        VPackBuilder builder;
        string attributeName;

        public byte[] GetBuffer()
        {
            return builder.Buffer;
        }

        public SliceWriter()
        {
            builder = new VPackBuilder();
        }

        /// <summary>
        /// Closes this stream and the underlying stream.
        /// </summary>
        public override void Close()
        {
            base.Close();
        }

        /// <summary>
        /// Writes the beginning of a JSON object.
        /// </summary>
        public override void WriteStartObject()
        {
            base.WriteStartObject();
            if (attributeName == null)
            {
                builder.Add(SliceType.Object);
            }
            else
            {
                builder.Add(attributeName, SliceType.Object);
                attributeName = null;
            }
        }

        /// <summary>
        /// Writes the end of a JSON object.
        /// </summary>
        public override void WriteEndObject()
        {
            base.WriteEndObject();
            builder.Close();
        }

        /// <summary>
        /// Writes the beginning of a JSON array.
        /// </summary>
        public override void WriteStartArray()
        {
            base.WriteStartArray();
            if (attributeName == null)
            {
                builder.Add(SliceType.Array);
            }
            else
            {
                builder.Add(attributeName, SliceType.Array);
                attributeName = null;
            }
        }

        /// <summary>
        /// Writes the end of an array.
        /// </summary>
        public override void WriteEndArray()
        {
            base.WriteEndArray();
            builder.Close();
        }

        /// <summary>
        /// Writes the property name of a name/value pair on a JSON object.
        /// </summary>
        /// <param name="name">The name of the property.</param>
        public override void WritePropertyName(string name)
        {
            base.WritePropertyName(name);

            attributeName = name;
        }

        /// <summary>
        /// Writes the property name of a name/value pair on a JSON object.
        /// </summary>
        /// <param name="name">The name of the property.</param>
        /// <param name="escape">A flag to indicate whether the text should be escaped when it is written as a JSON property name.</param>
        public override void WritePropertyName(string name, bool escape)
        {
            WritePropertyName(name);
        }

        /// <summary>
        /// Writes the end of the current JSON object or array.
        /// </summary>
        public override void WriteEnd()
        {
            base.WriteEnd();
        }

        /// <summary>
        /// Writes a null value.
        /// </summary>
        public override void WriteNull()
        {
            base.WriteNull();
            if (attributeName == null)
            {
                builder.Add(SliceType.Null);
            }
            else
            {
                builder.Add(attributeName, SliceType.Null);
                attributeName = null;
            }
        }

        /// <summary>
        /// Writes an undefined value.
        /// </summary>
        public override void WriteUndefined()
        {
            base.WriteUndefined();
            if (attributeName == null)
            {
                builder.Add(SliceType.None);
            }
            else
            {
                builder.Add(attributeName, SliceType.None);
                attributeName = null;
            }
        }

        /// <summary>
        /// Writes raw JSON without changing the writer's state.
        /// </summary>
        /// <param name="json">The raw JSON to write.</param>
        public override void WriteRaw(string json)
        {
            base.WriteRaw(json);
        }

        /// <summary>
        /// Writes raw JSON where a value is expected and updates the writer's state.
        /// </summary>
        /// <param name="json">The raw JSON to write.</param>
        public override void WriteRawValue(string json)
        {
            base.WriteRawValue(json);
        }

        /// <summary>
        /// Writes a <see cref="String"/> value.
        /// </summary>
        /// <param name="value">The <see cref="String"/> value to write.</param>
        public override void WriteValue(string value)
        {
            base.WriteValue(value);
            if (attributeName == null)
            {
                builder.Add(value);
            }
            else
            {
                builder.Add(attributeName, value);
                attributeName = null;
            }
        }

        /// <summary>
        /// Writes a <see cref="Int32"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Int32"/> value to write.</param>
        public override void WriteValue(int value)
        {
            base.WriteValue(value);
            if (attributeName == null)
            {
                builder.Add(value);
            }
            else
            {
                builder.Add(attributeName, value);
                attributeName = null;
            }
        }

        /// <summary>
        /// Writes a <see cref="UInt32"/> value.
        /// </summary>
        /// <param name="value">The <see cref="UInt32"/> value to write.</param>
        [CLSCompliant(false)]
        public override void WriteValue(uint value)
        {
            base.WriteValue(value);
            if (attributeName == null)
            {
                builder.Add(value);
            }
            else
            {
                builder.Add(attributeName, value);
                attributeName = null;
            }
        }

        /// <summary>
        /// Writes a <see cref="Int64"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Int64"/> value to write.</param>
        public override void WriteValue(long value)
        {
            base.WriteValue(value);
            if (attributeName == null)
            {
                builder.Add(value);
            }
            else
            {
                builder.Add(attributeName, value);
                attributeName = null;
            }
        }

        /// <summary>
        /// Writes a <see cref="UInt64"/> value.
        /// </summary>
        /// <param name="value">The <see cref="UInt64"/> value to write.</param>
        [CLSCompliant(false)]
        public override void WriteValue(ulong value)
        {
            base.WriteValue(value);
            if (attributeName == null)
            {
                builder.Add(value);
            }
            else
            {
                builder.Add(attributeName, value);
                attributeName = null;
            }
        }

        /// <summary>
        /// Writes a <see cref="Single"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Single"/> value to write.</param>
        public override void WriteValue(float value)
        {
            base.WriteValue(value);
            if (attributeName == null)
            {
                builder.Add(value);
            }
            else
            {
                builder.Add(attributeName, value);
                attributeName = null;
            }
        }

        /// <summary>
        /// Writes a <see cref="Double"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Double"/> value to write.</param>
        public override void WriteValue(double value)
        {
            base.WriteValue(value);
            if (attributeName == null)
            {
                builder.Add(value);
            }
            else
            {
                builder.Add(attributeName, value);
                attributeName = null;
            }
        }

        /// <summary>
        /// Writes a <see cref="Boolean"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Boolean"/> value to write.</param>
        public override void WriteValue(bool value)
        {
            base.WriteValue(value);
            if (attributeName == null)
            {
                builder.Add(value);
            }
            else
            {
                builder.Add(attributeName, value);
                attributeName = null;
            }
        }

        /// <summary>
        /// Writes a <see cref="Int16"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Int16"/> value to write.</param>
        public override void WriteValue(short value)
        {
            base.WriteValue(value);
            if (attributeName == null)
            {
                builder.Add(value);
            }
            else
            {
                builder.Add(attributeName, value);
                attributeName = null;
            }
        }

        /// <summary>
        /// Writes a <see cref="UInt16"/> value.
        /// </summary>
        /// <param name="value">The <see cref="UInt16"/> value to write.</param>
        [CLSCompliant(false)]
        public override void WriteValue(ushort value)
        {
            base.WriteValue(value);
            if (attributeName == null)
            {
                builder.Add(value);
            }
            else
            {
                builder.Add(attributeName, value);
                attributeName = null;
            }
        }

        /// <summary>
        /// Writes a <see cref="Char"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Char"/> value to write.</param>
        public override void WriteValue(char value)
        {
            base.WriteValue(value);
            if (attributeName == null)
            {
                builder.Add(value);
            }
            else
            {
                builder.Add(attributeName, value);
                attributeName = null;
            }
        }

        /// <summary>
        /// Writes a <see cref="Byte"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Byte"/> value to write.</param>
        public override void WriteValue(byte value)
        {
            base.WriteValue(value);
            if (attributeName == null)
            {
                builder.Add(value);
            }
            else
            {
                builder.Add(attributeName, value);
                attributeName = null;
            }
        }

        /// <summary>
        /// Writes a <see cref="SByte"/> value.
        /// </summary>
        /// <param name="value">The <see cref="SByte"/> value to write.</param>
        [CLSCompliant(false)]
        public override void WriteValue(sbyte value)
        {
            base.WriteValue(value);
            if (attributeName == null)
            {
                builder.Add(value);
            }
            else
            {
                builder.Add(attributeName, value);
                attributeName = null;
            }
        }

        /// <summary>
        /// Writes a <see cref="Decimal"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Decimal"/> value to write.</param>
        public override void WriteValue(decimal value)
        {
            base.WriteValue(value);
            throw new NotImplementedException(nameof(Decimal));
        }

        /// <summary>
        /// Writes a <see cref="DateTime"/> value.
        /// </summary>
        /// <param name="value">The <see cref="DateTime"/> value to write.</param>
        public override void WriteValue(DateTime value)
        {
            base.WriteValue(value);
            if (attributeName == null)
            {
                builder.Add(value);
            }
            else
            {
                builder.Add(attributeName, value);
                attributeName = null;
            }
        }

        /// <summary>
        /// Writes a <see cref="DateTimeOffset"/> value.
        /// </summary>
        /// <param name="value">The <see cref="DateTimeOffset"/> value to write.</param>
        public override void WriteValue(DateTimeOffset value)
        {
            base.WriteValue(value);
            if (attributeName == null)
            {
                builder.Add(value);
            }
            else
            {
                builder.Add(attributeName, value);
                attributeName = null;
            }
        }

        /// <summary>
        /// Writes a <see cref="Guid"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Guid"/> value to write.</param>
        public override void WriteValue(Guid value)
        {
            base.WriteValue(value);
            if (attributeName == null)
            {
                builder.Add(value.ToByteArray());
            }
            else
            {
                builder.Add(attributeName, value.ToByteArray());
                attributeName = null;
            }
        }

        /// <summary>
        /// Writes a <see cref="TimeSpan"/> value.
        /// </summary>
        /// <param name="value">The <see cref="TimeSpan"/> value to write.</param>
        public override void WriteValue(TimeSpan value)
        {
            base.WriteValue(value);
            if (attributeName == null)
            {
                builder.Add(value);
            }
            else
            {
                builder.Add(attributeName, value);
                attributeName = null;
            }
        }

        /// <summary>
        /// Writes a <see cref="Nullable{Int32}"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Nullable{Int32}"/> value to write.</param>
        public override void WriteValue(int? value)
        {
            base.WriteValue(value);
            if (attributeName == null)
            {
                builder.Add(value);
            }
            else
            {
                builder.Add(attributeName, value);
                attributeName = null;
            }
        }

        /// <summary>
        /// Writes a <see cref="Nullable{UInt32}"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Nullable{UInt32}"/> value to write.</param>
        [CLSCompliant(false)]
        public override void WriteValue(uint? value)
        {
            base.WriteValue(value);
            if (attributeName == null)
            {
                builder.Add(value);
            }
            else
            {
                builder.Add(attributeName, value);
                attributeName = null;
            }
        }

        /// <summary>
        /// Writes a <see cref="Nullable{Int64}"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Nullable{Int64}"/> value to write.</param>
        public override void WriteValue(long? value)
        {
            base.WriteValue(value);
            if (attributeName == null)
            {
                builder.Add(value);
            }
            else
            {
                builder.Add(attributeName, value);
                attributeName = null;
            }
        }

        /// <summary>
        /// Writes a <see cref="Nullable{UInt64}"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Nullable{UInt64}"/> value to write.</param>
        [CLSCompliant(false)]
        public override void WriteValue(ulong? value)
        {
            base.WriteValue(value);
            if (attributeName == null)
            {
                builder.Add(value);
            }
            else
            {
                builder.Add(attributeName, value);
                attributeName = null;
            }
        }

        /// <summary>
        /// Writes a <see cref="Nullable{Single}"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Nullable{Single}"/> value to write.</param>
        public override void WriteValue(float? value)
        {
            base.WriteValue(value);
            if (attributeName == null)
            {
                builder.Add(value);
            }
            else
            {
                builder.Add(attributeName, value);
                attributeName = null;
            }
        }

        /// <summary>
        /// Writes a <see cref="Nullable{Double}"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Nullable{Double}"/> value to write.</param>
        public override void WriteValue(double? value)
        {
            base.WriteValue(value);
            if (attributeName == null)
            {
                builder.Add(value);
            }
            else
            {
                builder.Add(attributeName, value);
                attributeName = null;
            }
        }

        /// <summary>
        /// Writes a <see cref="Nullable{Boolean}"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Nullable{Boolean}"/> value to write.</param>
        public override void WriteValue(bool? value)
        {
            base.WriteValue(value);
            if (attributeName == null)
            {
                builder.Add(value);
            }
            else
            {
                builder.Add(attributeName, value);
                attributeName = null;
            }
        }

        /// <summary>
        /// Writes a <see cref="Nullable{Int16}"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Nullable{Int16}"/> value to write.</param>
        public override void WriteValue(short? value)
        {
            base.WriteValue(value);
            if (attributeName == null)
            {
                builder.Add(value);
            }
            else
            {
                builder.Add(attributeName, value);
                attributeName = null;
            }
        }

        /// <summary>
        /// Writes a <see cref="Nullable{UInt16}"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Nullable{UInt16}"/> value to write.</param>
        [CLSCompliant(false)]
        public override void WriteValue(ushort? value)
        {
            base.WriteValue(value);
            if (attributeName == null)
            {
                builder.Add(value);
            }
            else
            {
                builder.Add(attributeName, value);
                attributeName = null;
            }
        }

        /// <summary>
        /// Writes a <see cref="Nullable{Char}"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Nullable{Char}"/> value to write.</param>
        public override void WriteValue(char? value)
        {
            base.WriteValue(value);
            if (attributeName == null)
            {
                builder.Add(value);
            }
            else
            {
                builder.Add(attributeName, value);
                attributeName = null;
            }
        }

        /// <summary>
        /// Writes a <see cref="Nullable{Byte}"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Nullable{Byte}"/> value to write.</param>
        public override void WriteValue(byte? value)
        {
            base.WriteValue(value);
            if (attributeName == null)
            {
                builder.Add(value);
            }
            else
            {
                builder.Add(attributeName, value);
                attributeName = null;
            }
        }

        /// <summary>
        /// Writes a <see cref="Nullable{SByte}"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Nullable{SByte}"/> value to write.</param>
        [CLSCompliant(false)]
        public override void WriteValue(sbyte? value)
        {
            base.WriteValue(value);
            if (attributeName == null)
            {
                builder.Add(value);
            }
            else
            {
                builder.Add(attributeName, value);
                attributeName = null;
            }
        }

        /// <summary>
        /// Writes a <see cref="Nullable{Decimal}"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Nullable{Decimal}"/> value to write.</param>
        public override void WriteValue(decimal? value)
        {
            base.WriteValue(value);
            throw new NotImplementedException(nameof(Decimal));
        }

        /// <summary>
        /// Writes a <see cref="Nullable{DateTime}"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Nullable{DateTime}"/> value to write.</param>
        public override void WriteValue(DateTime? value)
        {
            base.WriteValue(value);
            if (attributeName == null)
            {
                builder.Add(value);
            }
            else
            {
                builder.Add(attributeName, value);
                attributeName = null;
            }
        }

        /// <summary>
        /// Writes a <see cref="Nullable{DateTimeOffset}"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Nullable{DateTimeOffset}"/> value to write.</param>
        public override void WriteValue(DateTimeOffset? value)
        {
            base.WriteValue(value);
            if (attributeName == null)
            {
                builder.Add(value);
            }
            else
            {
                builder.Add(attributeName, value);
                attributeName = null;
            }
        }

        /// <summary>
        /// Writes a <see cref="Nullable{Guid}"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Nullable{Guid}"/> value to write.</param>
        public override void WriteValue(Guid? value)
        {
            base.WriteValue(value);
            if (attributeName == null)
            {
                builder.Add(value?.ToByteArray());
            }
            else
            {
                builder.Add(attributeName, value?.ToByteArray());
                attributeName = null;
            }
        }

        /// <summary>
        /// Writes a <see cref="Nullable{TimeSpan}"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Nullable{TimeSpan}"/> value to write.</param>
        public override void WriteValue(TimeSpan? value)
        {
            base.WriteValue(value);
            if (attributeName == null)
            {
                builder.Add(value);
            }
            else
            {
                builder.Add(attributeName, value);
                attributeName = null;
            }
        }

        /// <summary>
        /// Writes a <see cref="Byte"/>[] value.
        /// </summary>
        /// <param name="value">The <see cref="Byte"/>[] value to write.</param>
        public override void WriteValue(byte[] value)
        {
            base.WriteValue(value);
            if (attributeName == null)
            {
                builder.Add(value);
            }
            else
            {
                builder.Add(attributeName, value);
                attributeName = null;
            }
        }

        /// <summary>
        /// Writes a <see cref="Uri"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Uri"/> value to write.</param>
        public override void WriteValue(Uri value)
        {
            base.WriteValue(value);
            if (attributeName == null)
            {
                builder.Add(value?.ToString());
            }
            else
            {
                builder.Add(attributeName, value?.ToString());
                attributeName = null;
            }
        }

        public override void Flush()
        {
        }
    }
}

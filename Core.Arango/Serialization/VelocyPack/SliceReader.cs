using ArangoDB.VelocyPack.Enumerators;
using ArangoDB.VelocyPack.Exceptions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArangoDB.VelocyPack
{
    public class SliceReader : JsonReader
    {
        byte[] buffer;
        List<object> containerSlices;
        object currentContainerSlice => containerSlices.Last();
        VPackSlice currentSlice;

        class ObjectSliceState
        {
            public ObjectSliceState(VPackSlice slice)
            {
                if (slice.IsType(SliceType.Object) == false)
                    throw new VPackValueTypeException(SliceType.Object);

                Enumerator = slice.ObjectEnumerable().GetEnumerator();
            }

            IEnumerator<KeyValuePair<string, VPackSlice>> Enumerator { get; set; }

            public VPackSlice Slice => Enumerator.Current.Value;

            public string PropertyName => Enumerator.Current.Key;

            public bool Initialized { get; set; }

            public bool MoveNext()
            {
                return Enumerator.MoveNext();
            }
        }

        class ArraySliceState
        {
            public ArraySliceState(VPackSlice slice)
            {
                if (slice.IsType(SliceType.Array) == false)
                    throw new VPackValueTypeException(SliceType.Array);

                Enumerator = slice.ArrayEnumerable().GetEnumerator();
            }

            IEnumerator<VPackSlice> Enumerator { get; set; }

            public VPackSlice Slice => Enumerator.Current;

            public bool Initialized { get; set; }

            public bool MoveNext()
            {
                return Enumerator.MoveNext();
            }
        }

        public SliceReader(byte[] buffer)
        {
            this.buffer = buffer;
            VPackSlice s = new VPackSlice(buffer);

            containerSlices = new List<object>();

            currentSlice = s;
        }

        public override bool Read()
        {
            if (currentSlice == null)
            {
                // do nothing 
            }
            else if (currentSlice.IsType(SliceType.Object))
            {
                containerSlices.Add(new ObjectSliceState(currentSlice));
                currentSlice = null;
            }
            else if (currentSlice.IsType(SliceType.Array))
            {
                containerSlices.Add(new ArraySliceState(currentSlice));
                currentSlice = null;
            }
            else
            {
                ParseValue();
                currentSlice = null;
                return true;
            }

            var objectSlice = currentContainerSlice as ObjectSliceState;
            if (objectSlice != null)
            {
                if (objectSlice.Initialized == false)
                {
                    SetToken(JsonToken.StartObject);
                    objectSlice.Initialized = true;
                    return true;
                }

                bool hasAnotherSlice = objectSlice.MoveNext();
                if (hasAnotherSlice == false)
                {
                    containerSlices.Remove(objectSlice);
                    SetToken(JsonToken.EndObject);
                }
                else
                {
                    SetToken(JsonToken.PropertyName, objectSlice.PropertyName);
                    currentSlice = objectSlice.Slice;
                }
                return true;
            }

            var arraySlice = currentContainerSlice as ArraySliceState;
            if (arraySlice != null)
            {
                if (arraySlice.Initialized == false)
                {
                    SetToken(JsonToken.StartArray);
                    arraySlice.Initialized = true;
                    return true;
                }

                bool hasAnotherSlice = arraySlice.MoveNext();
                if (hasAnotherSlice == false)
                {
                    containerSlices.Remove(arraySlice);
                    SetToken(JsonToken.EndArray);
                }
                else
                {
                    currentSlice = arraySlice.Slice;
                    Read();
                }
                return true;
            }

            throw new InvalidOperationException($"Error at reading vpack slice");
        }

        public void SetCurrentFromArray()
        {
            var arraySlice = currentContainerSlice as ArraySliceState;
            if (arraySlice != null)
            {
                if (arraySlice.MoveNext())
                    currentSlice = arraySlice.Slice;
                else
                {
                    containerSlices.Remove(arraySlice);
                    SetToken(JsonToken.EndArray);
                }

            }
            else
                throw new InvalidOperationException($"Expected ArraySliceState");
        }

        public void ParseValue()
        {
            JsonToken? token = null;

            switch (currentSlice.Type)
            {
                case SliceType.Binary:
                    token = JsonToken.Bytes;
                    break;
                case SliceType.Boolean:
                    token = JsonToken.Boolean;
                    break;
                case SliceType.Double:
                    token = JsonToken.Float;
                    break;
                case SliceType.Int:
                case SliceType.UInt:
                case SliceType.SmallInt:
                    token = JsonToken.Integer;
                    break;
                case SliceType.UtcDate:
                    token = JsonToken.Date;
                    break;
                case SliceType.String:
                    token = JsonToken.String;
                    break;
                case SliceType.Null:
                    token = JsonToken.Null;
                    break;
            }

            if (token.HasValue == false)
                throw new InvalidOperationException($"Error at reading vpack slice parsing value");

            SetToken(token.Value, currentSlice.Value());
        }

        public override bool? ReadAsBoolean()
        {
            if (currentSlice == null)
                SetCurrentFromArray();

            if (currentSlice == null)
                return null;

            var value = currentSlice.ToBoolean();
            SetToken(JsonToken.Boolean, value);
            currentSlice = null;
            return value;
        }

        public override byte[] ReadAsBytes()
        {
            if (currentSlice == null)
                SetCurrentFromArray();

            if (currentSlice == null)
                return null;

            var value = currentSlice.ToBinary();
            SetToken(JsonToken.Bytes, value);
            currentSlice = null;
            return value;
        }

        public override DateTime? ReadAsDateTime()
        {
            if (currentSlice == null)
                SetCurrentFromArray();

            if (currentSlice == null)
                return null;

            var value = currentSlice.ToDateTime();
            SetToken(JsonToken.Date, value);
            currentSlice = null;
            return value;
        }

        public override DateTimeOffset? ReadAsDateTimeOffset()
        {
            if (currentSlice == null)
                SetCurrentFromArray();

            if (currentSlice == null)
                return null;

            var value = currentSlice.ToDateTimeOffset();
            SetToken(JsonToken.Date, value);
            currentSlice = null;
            return value;
        }

        public override decimal? ReadAsDecimal()
        {
            throw new NotImplementedException(nameof(Decimal));
        }

        public override double? ReadAsDouble()
        {
            if (currentSlice == null)
                SetCurrentFromArray();

            if (currentSlice == null)
                return null;

            var value = currentSlice.ToDouble();
            SetToken(JsonToken.Float, value);
            currentSlice = null;
            return value;
        }

        public override int? ReadAsInt32()
        {
            if (currentSlice == null)
                SetCurrentFromArray();

            if (currentSlice == null)
                return null;

            var value = currentSlice.ToInt32();
            SetToken(JsonToken.Integer, value);
            currentSlice = null;
            return value;
        }

        public override string ReadAsString()
        {
            if (currentSlice == null)
                SetCurrentFromArray();

            if (currentSlice == null)
                return null;

            var value = currentSlice.ToStringValue();
            SetToken(JsonToken.String, value);
            currentSlice = null;
            return value;
        }
    }
}

using ArangoDB.VelocyPack.Enumerators;
using ArangoDB.VelocyPack.Exceptions;
using ArangoDB.VelocyPack.Utils;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArangoDB.VelocyPack
{
    public class VPackSlice
    {
        public static IVPackAttributeTranslator attributeTranslator = new VPackAttributeTranslator();

        private byte[] vpack;
        private int start;

        protected VPackSlice()
            : this(new byte[] { 0x00 }, 0)
        {
        }

        public VPackSlice(byte[] vpack)
            : this(vpack, 0)
        {
        }

        public VPackSlice(byte[] vpack, int start)
        {
            this.vpack = vpack;
            this.start = start;
        }

        public byte TypeCode => vpack[start];

        public byte[] Buffer => vpack;

        public int Start => start;

        private SliceType? _type;
        public SliceType Type
        {
            get
            {
                if (_type.HasValue == false)
                    _type = SliceTypeUtil.Get(TypeCode);

                return _type.Value;
            }
        }

        private int? _byteLength;
        internal int ByteLength
        {
            get
            {
                if (_byteLength.HasValue == false)
                    _byteLength = ValueLengthUtil.Get(TypeCode) - 1;

                return _byteLength.Value;
            }
        }


        public bool IsType(SliceType type)
        {
            return Type == type;
        }

        public bool IsInteger()
        {
            return IsType(SliceType.Int) || IsType(SliceType.UInt) || IsType(SliceType.SmallInt);
        }

        public bool IsNumeric()
        {
            return IsInteger() || IsType(SliceType.Double);
        }

        #region convert unchecked

        DateTime ToDateTimeUnchecked(DateTimeKind? kind = DateTimeKind.Local)
        {
            var totalMilliseconds = BitConverter.ToInt64(vpack, start + 1);
            DateTime date = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(totalMilliseconds);
            return kind == DateTimeKind.Utc ? date : date.ToLocalTime();
        }

        DateTimeOffset ToDateTimeOffsetUnChecked(TimeSpan? offset = null)
        {
            if (offset.HasValue == false)
                offset = TimeSpan.Zero;

            var totalMilliseconds = BitConverter.ToInt64(vpack, start + 1);
            DateTimeOffset date = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero).AddMilliseconds(totalMilliseconds);
            return date.ToOffset(offset.Value);
        }
        
        double ToDoubleUnchecked()
        {
            return BitConverter.Int64BitsToDouble((long)BitConverter.ToUInt64(vpack, start + 1));
        }

        long ToInt64Unchecked()
        {
            return BitConverter.ToInt64(vpack, start + 1);
        }

        int ToInt32Unchecked()
        {
            return BitConverter.ToInt32(vpack, start + 1);
        }

        short ToInt16Unchecked()
        {
            return BitConverter.ToInt16(vpack, start + 1);
        }

        sbyte ToSByteUnchecked()
        {
            return (sbyte)vpack[start + 1];
        }

        ulong ToUInt64Unchecked()
        {
            return BitConverter.ToUInt64(vpack, start + 1);
        }

        uint ToUInt32Unchecked()
        {
            return BitConverter.ToUInt32(vpack, start + 1);
        }

        ushort ToUInt16Unchecked()
        {
            return BitConverter.ToUInt16(vpack, start + 1);
        }

        byte ToByteUnchecked()
        {
            return vpack[start + 1];
        }

        bool ToBooleanUnchecked()
        {
            return TypeCode == 0x1a;
        }

        int ToSmallIntUnchecked()
        {
            byte head = TypeCode;
            int smallInt;
            if (head >= 0x30 && head <= 0x39)
            {
                smallInt = head - 0x30;
            }
            else /* if (head >= 0x3a && head <= 0x3f) */
            {
                smallInt = head - 0x3a - 6;
            }
            return smallInt;
        }

        internal string ToStringUnchecked()
        {
            return IsLongString() ? ToLongString() : ToShortString();
        }

        byte[] ToBinaryUnchecked()
        {
            byte[] binary = BinaryUtil.ToBinary(vpack, start + 1 + TypeCode - 0xbf, BinaryLength());
            return binary;
        }

        #endregion

        #region convert

        public object Value()
        {
            if (IsType(SliceType.Boolean))
                return ToBooleanUnchecked();
            if (IsType(SliceType.Double))
                return ToDoubleUnchecked();
            if (TypeCode == 0x20)
                return ToSByteUnchecked();
            if (TypeCode == 0x21)
                return ToInt16Unchecked();
            if (TypeCode == 0x23)
                return ToInt32Unchecked();
            if (TypeCode == 0x27)
                return ToInt64Unchecked();
            if (TypeCode == 0x28)
                return ToByteUnchecked();
            if (TypeCode == 0x29)
                return ToUInt16Unchecked();
            if (TypeCode == 0x2b)
                return ToUInt32Unchecked();
            if (TypeCode == 0x2f)
                return ToUInt64Unchecked();
            if (IsType(SliceType.SmallInt))
                return ToSmallIntUnchecked();
            if (IsType(SliceType.String))
                return ToStringUnchecked();
            if (IsType(SliceType.Binary))
                return ToBinaryUnchecked();
            if (IsType(SliceType.UtcDate))
                return ToDateTimeOffsetUnChecked();
            if (IsType(SliceType.Null))
                return null;

            throw new InvalidOperationException($"Cannot get the value of type {Type}");
        }

        public DateTime? ToDateTime(DateTimeKind? kind = null, DateTimeStyles? style = null)
        {
            if(IsType(SliceType.Null))
            {
                return null;
            }
            if(IsType(SliceType.UtcDate))
            {
                return ToDateTimeUnchecked(kind);
            }
            else if (IsType(SliceType.String))
            {
                string time = ToStringUnchecked();

                if (style.HasValue == false)
                    style = DateTimeStyles.RoundtripKind;
                
                return DateTime.Parse(time, CultureInfo.InvariantCulture, style.Value);
            }
            else
                throw new VPackValueTypeException(SliceType.UtcDate, SliceType.String);
        }

        public DateTimeOffset? ToDateTimeOffset(TimeSpan? offset = null, DateTimeStyles? style = null)
        {
            if (IsType(SliceType.Null))
            {
                return null;
            }
            if (IsType(SliceType.UtcDate))
            {
                return ToDateTimeOffsetUnChecked(offset);
            }
            else if (IsType(SliceType.String))
            {
                string time = ToStringUnchecked();

                if (style.HasValue == false)
                    style = DateTimeStyles.RoundtripKind;

                return DateTimeOffset.Parse(time, CultureInfo.InvariantCulture, style.Value);
            }
            else
                throw new VPackValueTypeException(SliceType.UtcDate, SliceType.String);
        }

        public TimeSpan? ToTimeSpan()
        {
            if (IsType(SliceType.Null))
                return null;

            return TimeSpan.FromTicks(ToInt64().Value);
        }

        public bool? ToBoolean()
        {
            if (IsType(SliceType.Null))
                return null;
            
            return Convert.ToBoolean(Value(), CultureInfo.InvariantCulture);
        }

        public double? ToDouble()
        {
            if (IsType(SliceType.Null))
                return null;

            return Convert.ToDouble(Value(), CultureInfo.InvariantCulture);
        }

        public float? ToSingle()
        {
            if (IsType(SliceType.Null))
                return null;

            return Convert.ToSingle(Value(), CultureInfo.InvariantCulture);
        }

        [CLSCompliant(false)]
        public sbyte? ToSByte()
        {
            if (IsType(SliceType.Null))
                return null;

            return Convert.ToSByte(Value(), CultureInfo.InvariantCulture);
        }

        public short? ToInt16()
        {
            if (IsType(SliceType.Null))
                return null;

            return Convert.ToInt16(Value(), CultureInfo.InvariantCulture);
        }

        public int? ToInt32()
        {
            if (IsType(SliceType.Null))
                return null;

            return Convert.ToInt32(Value(), CultureInfo.InvariantCulture);
        }

        public long? ToInt64()
        {
            if (IsType(SliceType.Null))
                return null;

            return Convert.ToInt64(Value(), CultureInfo.InvariantCulture);
        }

        public byte? ToByte()
        {
            if (IsType(SliceType.Null))
                return null;

            return Convert.ToByte(Value(), CultureInfo.InvariantCulture);
        }

        [CLSCompliant(false)]
        public ushort? ToUInt16()
        {
            if (IsType(SliceType.Null))
                return null;

            return Convert.ToUInt16(Value(), CultureInfo.InvariantCulture);
        }

        [CLSCompliant(false)]
        public uint? ToUInt32()
        {
            if (IsType(SliceType.Null))
                return null;

            return Convert.ToUInt32(Value(), CultureInfo.InvariantCulture);
        }

        [CLSCompliant(false)]
        public ulong? ToUInt64()
        {
            if (IsType(SliceType.Null))
                return null;

            return Convert.ToUInt64(Value(), CultureInfo.InvariantCulture);
        }

        public string ToStringValue()
        {
            if (IsType(SliceType.UtcDate))
            {
                return ToDateTimeOffset().Value.ToString("o");
            }

            return Value()?.ToString();
        }

        public char? ToChar()
        {
            if (IsType(SliceType.Null))
                return null;

            return Convert.ToChar(Value(), CultureInfo.InvariantCulture);
        }

        public byte[] ToBinary()
        {
            if (IsType(SliceType.Null))
            {
                return null;
            }
            if (IsType(SliceType.Binary))
            {
                return ToBinaryUnchecked();
            }

            throw new VPackValueTypeException(SliceType.Binary);
        }

        #endregion
        
        #region string helpers

        string ToShortString()
        {
            return StringUtil.ToString(vpack, start + 1, ByteLength);
        }

        string ToLongString()
        {
            return StringUtil.ToString(vpack, start + 9, GetLongStringLength());
        }

        bool IsLongString()
        {
            return TypeCode == 0xbf;
        }
        
        int GetLongStringLength()
        {
            return (int)NumberUtil.ToLong(vpack, start + 1, 8);
        }

        int GetStringLength()
        {
            return IsLongString() ? GetLongStringLength() : TypeCode - 0x40;
        }

        #endregion

        #region binary helpers
        public int BinaryLength()
        {
            if (!IsType(SliceType.Binary))
            {
                throw new VPackValueTypeException(SliceType.Binary);
            }
            return GetBinaryLengthUnchecked();
        }

        int GetBinaryLengthUnchecked()
        {
            return (int)NumberUtil.ToLong(vpack, start + 1, TypeCode - 0xbf);
        }
        #endregion

        private int? _length;

        /// <summary>
        /// Return the number of members for an Array, Object or String
        /// </summary>
        public int Length
        {
            get
            {
                if (_length.HasValue)
                    return _length.Value;

                long length;
                if (IsType(SliceType.String))
                {
                    length = GetStringLength();
                }
                else if (!IsType(SliceType.Array) && !IsType(SliceType.Object))
                {
                    throw new VPackValueTypeException(SliceType.Array, SliceType.Object, SliceType.String);
                }
                else
                {
                    byte head = TypeCode;
                    if (head == 0x01 || head == 0x0a)
                    {
                        // empty
                        length = 0;
                    }
                    else if (head == 0x13 || head == 0x14)
                    {
                        // compact array or object
                        long end = NumberUtil.ReadVariableValueLength(vpack, start + 1, false);
                        length = NumberUtil.ReadVariableValueLength(vpack, (int)(start + end - 1), true);
                    }
                    else
                    {
                        int offsetsize = ObjectArrayUtil.GetOffsetSize(head);
                        long end = NumberUtil.ToLong(vpack, start + 1, offsetsize);
                        if (head <= 0x05)
                        {
                            // array with no offset table or length
                            int dataOffset = FindDataOffset();
                            VPackSlice first = new VPackSlice(vpack, start + dataOffset);
                            length = (end - dataOffset) / first.GetByteSize();
                        }
                        else if (offsetsize < 8)
                        {
                            length = NumberUtil.ToLong(vpack, start + 1 + offsetsize, offsetsize);
                        }
                        else
                        {
                            length = NumberUtil.ToLong(vpack, (int)(start + end - offsetsize), offsetsize);
                        }
                    }
                }

                _length = (int)length;

                return (int)length;
            }
        }


        /// <summary>
        /// Must be called for a nonempty array or object at start():
        /// </summary>
        /// <returns></returns>
        internal int FindDataOffset()
        {
            int fsm = ObjectArrayUtil.GetFirstSubMap(TypeCode);
            int offset;
            if (fsm <= 2 && vpack[start + 2] != 0)
            {
                offset = 2;
            }
            else if (fsm <= 3 && vpack[start + 3] != 0)
            {
                offset = 3;
            }
            else if (fsm <= 5 && vpack[start + 6] != 0)
            {
                offset = 5;
            }
            else
            {
                offset = 9;
            }
            return offset;
        }

        internal int GetByteSize()
        {
            long size;
            byte head = TypeCode;
            int valueLength = ValueLengthUtil.Get(head);
            if (valueLength != 0)
            {
                size = valueLength;
            }
            else
            {
                switch (Type)
                {
                    case SliceType.Array:
                    case SliceType.Object:
                        if (head == 0x13 || head == 0x14)
                        {
                            // compact Array or Object
                            size = NumberUtil.ReadVariableValueLength(vpack, start + 1, false);
                        }
                        else /* if (head <= 0x14) */
                        {
                            size = NumberUtil.ToLong(vpack, start + 1, ObjectArrayUtil.GetOffsetSize(head));
                        }
                        break;
                    case SliceType.String:
                        // long UTF-8 String
                        size = GetLongStringLength() + 1 + 8;
                        break;
                    case SliceType.Binary:
                        size = 1 + head - ((byte)0xbf) + GetBinaryLengthUnchecked();
                        break;
                    case SliceType.Bcd:
                        if (head <= 0xcf)
                        {
                            size = 1 + head + ((byte)0xc7) + NumberUtil.ToLong(vpack, start + 1, head - ((byte)0xc7));
                        }
                        else
                        {
                            size = 1 + head - ((byte)0xcf) + NumberUtil.ToLong(vpack, start + 1, head - ((byte)0xcf));
                        }
                        break;
                    case SliceType.Custom:
                        if (head == 0xf4 || head == 0xf5 || head == 0xf6)
                        {
                            size = 2 + NumberUtil.ToLong(vpack, start + 1, 1);
                        }
                        else if (head == 0xf7 || head == 0xf8 || head == 0xf9)
                        {
                            size = 3 + NumberUtil.ToLong(vpack, start + 1, 2);
                        }
                        else if (head == 0xfa || head == 0xfb || head == 0xfc)
                        {
                            size = 5 + NumberUtil.ToLong(vpack, start + 1, 4);
                        }
                        else /* if (head == 0xfd || head == 0xfe || head == 0xff) */
                        {
                            size = 9 + NumberUtil.ToLong(vpack, start + 1, 8);
                        }
                        break;
                    default:
                        // TODO
                        throw new Exception("Internal error");
                }
            }
            return (int)size;
        }

        #region indexers

        /// <summary>
        /// Return array value at the specified index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public VPackSlice this[int index]
        {
            get
            {
                if (!IsType(SliceType.Array))
                {
                    throw new VPackValueTypeException(SliceType.Array);
                }
                return GetNth(index);
            }
        }

        /// <summary>
        /// Return object by attribute name
        /// </summary>
        /// <param name="attribute"></param>
        /// <returns></returns>
        public VPackSlice this[string attribute]
        {
            get
            {
                if (!IsType(SliceType.Object))
                {
                    throw new VPackValueTypeException(SliceType.Object);
                }
                byte head = TypeCode;
                VPackSlice result = new VPackSlice();
                if (head == 0x0a)
                {
                    // special case, empty object
                    result = new VPackSlice();
                }
                else if (head == 0x14)
                {
                    // compact Object
                    result = GetFromCompactObject(attribute);
                }
                else
                {
                    int offsetsize = ObjectArrayUtil.GetOffsetSize(head);
                    long end = NumberUtil.ToLong(vpack, start + 1, offsetsize);
                    long n;
                    if (offsetsize < 8)
                    {
                        n = NumberUtil.ToLong(vpack, start + 1 + offsetsize, offsetsize);
                    }
                    else
                    {
                        n = NumberUtil.ToLong(vpack, (int)(start + end - offsetsize), offsetsize);
                    }
                    if (n == 1)
                    {
                        // Just one attribute, there is no index table!
                        VPackSlice key = new VPackSlice(vpack, start + FindDataOffset());

                        if (key.IsType(SliceType.String))
                        {
                            if (key.IsEqualString(attribute))
                            {
                                result = new VPackSlice(vpack, key.start + key.GetByteSize());
                            }
                            else
                            {
                                // no match
                                result = new VPackSlice();
                            }
                        }
                        else if (key.IsInteger())
                        {
                            // translate key
                            if (attributeTranslator == null)
                            {
                                throw new VPackNeedAttributeTranslatorException();
                            }
                            if (key.TranslateUnchecked().IsEqualString(attribute))
                            {
                                result = new VPackSlice(vpack, key.start + key.GetByteSize());
                            }
                            else
                            {
                                // no match
                                result = new VPackSlice();
                            }
                        }
                        else
                        {
                            // no match
                            result = new VPackSlice();
                        }
                    }
                    else
                    {
                        long ieBase = end - n * offsetsize - (offsetsize == 8 ? 8 : 0);

                        // only use binary search for attributes if we have at least
                        // this many entries
                        // otherwise we'll always use the linear search
                        long sortedSearchEntriesThreshold = 4;

                        bool sorted = head >= 0x0b && head <= 0x0e;
                        if (sorted && n >= sortedSearchEntriesThreshold)
                        {
                            // This means, we have to handle the special case n == 1
                            // only in the linear search!
                            result = SearchObjectKeyBinary(attribute, ieBase, offsetsize, n);
                        }
                        else
                        {
                            result = SearchObjectKeyLinear(attribute, ieBase, offsetsize, n);
                        }
                    }
                }
                return result;
            }
        }

        #endregion


        #region object helpers

        //translates an integer key into a string, without checks
        VPackSlice TranslateUnchecked()
        {
            VPackSlice result = attributeTranslator.Translate(ToInt32().Value);
            return result != null ? result : new VPackSlice();
        }

        internal VPackSlice MakeKey()
        {
            if (IsType(SliceType.String))
            {
                return this;

            }
            if (IsInteger())
            {
                if (attributeTranslator == null)
                {
                    throw new VPackNeedAttributeTranslatorException();
                }
                return TranslateUnchecked();
            }
            throw new VPackKeyTypeException("Cannot translate key of this type");
        }

        VPackSlice GetFromCompactObject(string attribute)
        {
            foreach (var next in ObjectEnumerable())
            {
                if (next.Key.Equals(attribute))
                {
                    return next.Value;
                }
            }
            // not found
            return new VPackSlice();
        }

        VPackSlice SearchObjectKeyBinary(
                     string attribute,
                     long ieBase,
                     int offsetsize,
                     long n)
        {

            bool useTranslator = attributeTranslator != null;
            VPackSlice result;
            long l = 0;
            long r = n - 1;

            for (;;)
            {
                // midpoint
                long index = l + ((r - l) / 2);
                long offset = ieBase + index * offsetsize;
                long keyIndex = NumberUtil.ToLong(vpack, (int)(start + offset), offsetsize);
                VPackSlice key = new VPackSlice(vpack, (int)(start + keyIndex));
                int res = 0;
                if (key.IsType(SliceType.String))
                {
                    res = key.CompareString(attribute);
                }
                else if (key.IsInteger())
                {
                    // translate key
                    if (!useTranslator)
                    {
                        // no attribute translator
                        throw new VPackNeedAttributeTranslatorException();
                    }
                    res = key.TranslateUnchecked().CompareString(attribute);
                }
                else
                {
                    // invalid key
                    result = new VPackSlice();
                    break;
                }
                if (res == 0)
                {
                    // found
                    result = new VPackSlice(vpack, key.start + key.GetByteSize());
                    break;
                }
                if (res > 0)
                {
                    if (index == 0)
                    {
                        result = new VPackSlice();
                        break;
                    }
                    r = index - 1;
                }
                else
                {
                    l = index + 1;
                }
                if (r < l)
                {
                    result = new VPackSlice();
                    break;
                }
            }
            return result;
        }

        private VPackSlice SearchObjectKeyLinear(
             string attribute,
             long ieBase,
             int offsetsize,
             long n)
        {
            bool useTranslator = attributeTranslator != null;
            VPackSlice result = new VPackSlice();
            for (long index = 0; index < n; index++)
            {
                long offset = ieBase + index * offsetsize;
                long keyIndex = NumberUtil.ToLong(vpack, (int)(start + offset), offsetsize);
                VPackSlice key = new VPackSlice(vpack, (int)(start + keyIndex));
                if (key.IsType(SliceType.String))
                {
                    if (!key.IsEqualString(attribute))
                    {
                        continue;
                    }
                }
                else if (key.IsInteger())
                {
                    // translate key
                    if (!useTranslator)
                    {
                        // no attribute translator
                        throw new VPackNeedAttributeTranslatorException();
                    }
                    if (!key.TranslateUnchecked().IsEqualString(attribute))
                    {
                        continue;
                    }
                }
                else
                {
                    // invalid key type
                    result = new VPackSlice();
                    break;
                }
                // key is identical. now return value
                result = new VPackSlice(vpack, key.start + key.GetByteSize());
                break;
            }
            return result;

        }

        public VPackSlice KeyAt(int index)
        {
            if (!IsType(SliceType.Object))
            {
                throw new VPackValueTypeException(SliceType.Object);
            }
            return GetNthKey(index);
        }

        public VPackSlice ValueAt(int index)
        {
            if (!IsType(SliceType.Object))
            {
                throw new VPackValueTypeException(SliceType.Object);
            }
            VPackSlice key = GetNthKey(index);
            return new VPackSlice(vpack, key.start + key.GetByteSize());
        }

        VPackSlice GetNthKey(int index)
        {
            return new VPackSlice(vpack, start + GetNthOffset(index));
        }

        VPackSlice GetNth(int index)
        {
            return new VPackSlice(vpack, start + GetNthOffset(index));
        }

        //Return the offset for the nth member from an Array or Object type
        int GetNthOffset(int index)
        {
            int offset;
            byte head = TypeCode;
            if (head == 0x13 || head == 0x14)
            {
                // compact Array or Object
                offset = GetNthOffsetFromCompact(index);
            }
            else if (head == 0x01 || head == 0x0a)
            {
                // special case: empty Array or empty Object
                throw new IndexOutOfRangeException();
            }
            else
            {
                long n;
                int offsetsize = ObjectArrayUtil.GetOffsetSize(head);
                long end = NumberUtil.ToLong(vpack, start + 1, offsetsize);
                int dataOffset = FindDataOffset();
                if (head <= 0x05)
                {
                    // array with no offset table or length
                    VPackSlice first = new VPackSlice(vpack, start + dataOffset);
                    n = (end - dataOffset) / first.GetByteSize();
                }
                else if (offsetsize < 8)
                {
                    n = NumberUtil.ToLong(vpack, start + 1 + offsetsize, offsetsize);
                }
                else
                {
                    n = NumberUtil.ToLong(vpack, (int)(start + end - offsetsize), offsetsize);
                }
                if (index >= n)
                {
                    throw new IndexOutOfRangeException();
                }
                if (head <= 0x05 || n == 1)
                {
                    // no index table, but all array items have the same length
                    // or only one item is in the array
                    // now fetch first item and determine its length
                    if (dataOffset == 0)
                    {
                        dataOffset = FindDataOffset();
                    }
                    offset = dataOffset + index * new VPackSlice(vpack, start + dataOffset).GetByteSize();
                }
                else
                {
                    long ieBase = end - n * offsetsize + index * offsetsize - (offsetsize == 8 ? 8 : 0);
                    offset = (int)NumberUtil.ToLong(vpack, (int)(start + ieBase), offsetsize);
                }
            }
            return offset;
        }

        //Return the offset for the nth member from a compact Array or Object type
        int GetNthOffsetFromCompact(int index)
        {
            long end = NumberUtil.ReadVariableValueLength(vpack, start + 1, false);
            long n = NumberUtil.ReadVariableValueLength(vpack, (int)(start + end - 1), true);
            if (index >= n)
            {
                throw new IndexOutOfRangeException();
            }
            byte head = TypeCode;
            long offset = 1 + NumberUtil.GetVariableValueLength(end);
            long current = 0;
            while (current != index)
            {
                long byteSize = new VPackSlice(vpack, (int)(start + offset)).GetByteSize();
                offset += byteSize;
                if (head == 0x14)
                {
                    offset += byteSize;
                }
                ++current;
            }
            return (int)offset;
        }

        #endregion

        bool IsEqualString(string s)
        {
            string _string = ToStringUnchecked();
            return _string.Equals(s);
        }

        int CompareString(string s)
        {
            string _string = ToStringUnchecked();
            return _string.CompareTo(s);
        }

        public IEnumerable<VPackSlice> ArrayEnumerable()
        {
            if (IsType(SliceType.Array))
            {
                return new ArrayEnumerable(this);
            }
            else
            {
                throw new VPackValueTypeException(SliceType.Array);
            }
        }

        public IEnumerable<KeyValuePair<string, VPackSlice>> ObjectEnumerable()
        {
            if (IsType(SliceType.Object))
            {
                return new ObjectEnumerable(this);
            }
            else
            {
                throw new VPackValueTypeException(SliceType.Object);
            }
        }

        T[] CopyOfRange<T>(T[] src, int start, int end)
        {
            int len = end - start;
            T[] dest = new T[len];
            Array.Copy(src, start, dest, 0, len);
            return dest;
        }

        public byte[] GetRawVPack()
        {
            return CopyOfRange(vpack, start, start + GetByteSize());
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int prime = 31;
                int result = 1;
                result = prime * result + start;

                var bytes = GetRawVPack();
                int bytesHash = 17;
                foreach (var b in bytes)
                {
                    bytesHash = bytesHash * 31 + EqualityComparer<byte>.Default.GetHashCode(b);
                }


                result = prime * result + bytesHash;
                return result;
            }
        }

        public override bool Equals(object obj)
        {
            if (this == obj)
            {
                return true;
            }
            if (obj == null)
            {
                return false;
            }
            if (GetType() != obj.GetType())
            {
                return false;
            }
            VPackSlice other = (VPackSlice)obj;
            if (start != other.start)
            {
                return false;
            }
            if (!GetRawVPack().SequenceEqual(other.GetRawVPack()))
            {
                return false;
            }
            return true;
        }

    }
}

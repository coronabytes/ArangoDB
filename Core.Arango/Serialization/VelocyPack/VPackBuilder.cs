using ArangoDB.VelocyPack.Exceptions;
using ArangoDB.VelocyPack.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ArangoDB.VelocyPack
{
    public class VPackBuilder
    {
        byte[] buffer;

        List<int> stack;
        Dictionary<int, List<int>> index;
        int size;
        bool keyWritten;
        IBuilderOptions options;

        public VPackBuilder()
            : this(new DefaultVPackBuilderOptions())
        {
        }

        public VPackBuilder(IBuilderOptions options)
        {
            this.options = options;
            size = 0;
            buffer = new byte[10];
            stack = new List<int>();
            index = new Dictionary<int, List<int>>();
        }

        public IBuilderOptions GetOptions()
        {
            return options;
        }

        void EnsureCapacity(int minCapacity)
        {
            int oldCapacity = buffer.Length;
            if (minCapacity > oldCapacity)
            {
                byte[] oldData = buffer;
                int newCapacity = (oldCapacity * 3) / 2 + 1;
                if (newCapacity < minCapacity)
                {
                    newCapacity = minCapacity;
                }
                buffer = new byte[newCapacity];
                System.Buffer.BlockCopy(oldData, 0, buffer, 0, oldData.Length);
            }
        }

        void Write(byte b)
        {
            Write(b, size + 1);
        }

        void Write(byte b, int minCapacity)
        {
            EnsureCapacity(minCapacity);
            buffer[size++] = b;
        }

        void Write(byte[] bytes)
        {
            Write(bytes, size + bytes.Length);
        }

        void Write(byte[] bytes, int minCapacity)
        {
            EnsureCapacity(minCapacity);
            for (int i = size; i < size + bytes.Length; i++)
                buffer[i] = bytes[i - size];

            size += bytes.Length;
        }

        void WriteUnchecked(byte b)
        {
            buffer[size++] = b;
        }

        #region add

        public VPackBuilder Add(SliceType value, bool unindexed = false)
        {
            return WrapAdd(value, () => AppendValueType(value, unindexed));
        }

        public VPackBuilder Add(bool? value)
        {
            return WrapAdd(value, () => AppendBool(value.Value));
        }

        public VPackBuilder Add(double? value)
        {
            return WrapAdd(value, () => AppendDouble(value.Value));
        }

        public VPackBuilder Add(float? value)
        {
            return WrapAdd(value, () => AppendDouble(value.Value));
        }

        public VPackBuilder AddCompactInt(long? value)
        {
            return WrapAdd(value, () =>
            {
                if (value <= 9 && value >= -6)
                {
                    AppendSmallInt(value.Value);
                }
                else if (value <= sbyte.MaxValue && value >= sbyte.MinValue)
                {
                    AppendSByte((sbyte)value.Value);
                }
                else if (value <= short.MaxValue && value >= short.MinValue)
                {
                    AppendShort((short)value.Value);
                }
                else if (value <= int.MaxValue && value >= int.MinValue)
                {
                    AppendInt((int)value.Value);
                }
                else
                {
                    AppendLong(value.Value);
                }
            });
        }

        [CLSCompliant(false)]
        public VPackBuilder AddCompactUInt(ulong? value)
        {
            return WrapAdd(value, () =>
            {
                if (value <= 9)
                {
                    AppendSmallInt(value.Value);
                }
                else if (value <= byte.MaxValue)
                {
                    AppendByte((byte)value.Value);
                }
                else if (value <= ushort.MaxValue)
                {
                    AppendUShort((ushort)value.Value);
                }
                else if (value <= uint.MaxValue)
                {
                    AppendUInt((uint)value.Value);
                }
                else
                {
                    AppendULong(value.Value);
                }
            });
        }

        [CLSCompliant(false)]
        public VPackBuilder Add(sbyte? value)
        {
            return options.IsBuildCompactIntegers()
                ? AddCompactInt(value)
                : WrapAdd(value, () => AppendSByte(value.Value));
        }

        public VPackBuilder Add(byte? value)
        {
            return options.IsBuildCompactIntegers()
                ? AddCompactUInt(value)
                : WrapAdd(value, () => AppendByte(value.Value));
        }

        public VPackBuilder Add(short? value)
        {
            return options.IsBuildCompactIntegers()
                ? AddCompactInt(value)
                : WrapAdd(value, () => AppendShort(value.Value));
        }

        [CLSCompliant(false)]
        public VPackBuilder Add(ushort? value)
        {
            return options.IsBuildCompactIntegers()
                ? AddCompactUInt(value)
                : WrapAdd(value, () => AppendUShort(value.Value));
        }

        public VPackBuilder Add(int? value)
        {
            return options.IsBuildCompactIntegers()
                ? AddCompactInt(value)
                : WrapAdd(value, () => AppendInt(value.Value));
        }

        [CLSCompliant(false)]
        public VPackBuilder Add(uint? value)
        {
            return options.IsBuildCompactIntegers()
                ? AddCompactUInt(value)
                : WrapAdd(value, () => AppendUInt(value.Value));
        }

        public VPackBuilder Add(long? value)
        {
            return options.IsBuildCompactIntegers()
                ? AddCompactInt(value)
                : WrapAdd(value, () => AppendLong(value.Value));
        }

        [CLSCompliant(false)]
        public VPackBuilder Add(ulong? value)
        {
            return options.IsBuildCompactIntegers()
                ? AddCompactUInt(value)
                : WrapAdd(value, () => AppendULong(value.Value));
        }

        public VPackBuilder Add(string value)
        {
            return WrapAdd(value, () => AppendString(value));
        }

        public VPackBuilder Add(char? value)
        {
            return WrapAdd(value, () => AppendString(value.Value.ToString()));
        }

        public VPackBuilder Add(byte[] value)
        {
            return WrapAdd(value, () => AppendBinary(value));
        }

        public VPackBuilder Add(DateTime? value)
        {
            return WrapAdd(value, () => AppendDateTime(value.Value));
        }

        public VPackBuilder Add(DateTimeOffset? value)
        {
            return WrapAdd(value, () => AppendDateTimeOffset(value.Value));
        }

        public VPackBuilder Add(TimeSpan? value)
        {
            return WrapAdd(value, () => AppendTimeSpan(value.Value));
        }

        public VPackBuilder Add(VPackSlice value)
        {
            return WrapAdd(value, () => AppendVPack(value));
        }

        public VPackBuilder Add(string attribute, SliceType value, bool unindexed = false)
        {
            return WrapAdd(attribute, value, () => AppendValueType(value, unindexed));
        }

        public VPackBuilder Add(string attribute, bool? value)
        {
            return WrapAdd(attribute, value, () => AppendBool(value.Value));
        }

        public VPackBuilder Add(string attribute, double? value)
        {
            return WrapAdd(attribute, value, () => AppendDouble(value.Value));
        }

        public VPackBuilder Add(string attribute, float? value)
        {
            return WrapAdd(attribute, value, () => AppendDouble(value.Value));
        }

        public VPackBuilder AddCompactInt(string attribute, long? value)
        {
            return WrapAdd(attribute, value, () =>
            {
                if (value <= 9 && value >= -6)
                {
                    AppendSmallInt(value.Value);
                }
                else if (value <= sbyte.MaxValue && value >= sbyte.MinValue)
                {
                    AppendSByte((sbyte)value.Value);
                }
                else if (value <= short.MaxValue && value >= short.MinValue)
                {
                    AppendShort((short)value.Value);
                }
                else if (value <= int.MaxValue && value >= int.MinValue)
                {
                    AppendInt((int)value.Value);
                }
                else
                {
                    AppendLong(value.Value);
                }
            });
        }

        [CLSCompliant(false)]
        public VPackBuilder AddCompactUInt(string attribute, ulong? value)
        {
            return WrapAdd(attribute, value, () =>
            {
                if (value <= 9)
                {
                    AppendSmallInt(value.Value);
                }
                else if (value <= byte.MaxValue)
                {
                    AppendByte((byte)value.Value);
                }
                else if (value <= ushort.MaxValue)
                {
                    AppendUShort((ushort)value.Value);
                }
                else if (value <= uint.MaxValue)
                {
                    AppendUInt((uint)value.Value);
                }
                else
                {
                    AppendULong(value.Value);
                }
            });
        }

        [CLSCompliant(false)]
        public VPackBuilder Add(string attribute, sbyte? value)
        {
            return options.IsBuildCompactIntegers()
                ? AddCompactInt(attribute, value)
                : WrapAdd(attribute, value, () => AppendSByte(value.Value));
        }

        public VPackBuilder Add(string attribute, byte? value)
        {
            return options.IsBuildCompactIntegers()
                ? AddCompactUInt(attribute, value)
                : WrapAdd(attribute, value, () => AppendByte(value.Value));
        }

        public VPackBuilder Add(string attribute, short? value)
        {
            return options.IsBuildCompactIntegers()
                ? AddCompactInt(attribute, value)
                : WrapAdd(attribute, value, () => AppendShort(value.Value));
        }

        [CLSCompliant(false)]
        public VPackBuilder Add(string attribute, ushort? value)
        {
            return options.IsBuildCompactIntegers()
                ? AddCompactUInt(attribute, value)
                : WrapAdd(attribute, value, () => AppendUShort(value.Value));
        }

        public VPackBuilder Add(string attribute, int? value)
        {
            return options.IsBuildCompactIntegers()
                ? AddCompactInt(attribute, value)
                : WrapAdd(attribute, value, () => AppendInt(value.Value));
        }

        [CLSCompliant(false)]
        public VPackBuilder Add(string attribute, uint? value)
        {
            return options.IsBuildCompactIntegers()
                ? AddCompactUInt(attribute, value)
                : WrapAdd(attribute, value, () => AppendUInt(value.Value));
        }

        public VPackBuilder Add(string attribute, long? value)
        {
            return options.IsBuildCompactIntegers()
                ? AddCompactInt(attribute, value)
                : WrapAdd(attribute, value, () => AppendLong(value.Value));
        }

        [CLSCompliant(false)]
        public VPackBuilder Add(string attribute, ulong? value)
        {
            return options.IsBuildCompactIntegers()
                ? AddCompactUInt(attribute, value)
                : WrapAdd(attribute, value, () => AppendULong(value.Value));
        }

        public VPackBuilder Add(string attribute, string value)
        {
            return WrapAdd(attribute, value, () => AppendString(value));
        }

        public VPackBuilder Add(string attribute, char? value)
        {
            return WrapAdd(attribute, value, () => AppendString(value.Value.ToString()));
        }

        public VPackBuilder Add(string attribute, byte[] value)
        {
            return WrapAdd(attribute, value, () => AppendBinary(value));
        }

        public VPackBuilder Add(string attribute, DateTime? value)
        {
            return WrapAdd(attribute, value, () => AppendDateTime(value.Value));
        }

        public VPackBuilder Add(string attribute, DateTimeOffset? value)
        {
            return WrapAdd(attribute, value, () => AppendDateTimeOffset(value.Value));
        }

        public VPackBuilder Add(string attribute, TimeSpan? value)
        {
            return WrapAdd(attribute, value, () => AppendTimeSpan(value.Value));
        }

        public VPackBuilder Add(string attribute, VPackSlice value)
        {
            return WrapAdd(attribute, value, () => AppendVPack(value));
        }

        #endregion

        #region append

        void AppendSByte(sbyte value)
        {
            Write(0x20);
            Write((byte)value);
        }

        void AppendShort(short value)
        {
            Write(0x21);
            Write(BitConverter.GetBytes(value), size + sizeof(short));
        }

        void AppendInt(int value)
        {
            Write(0x23);
            Write(BitConverter.GetBytes(value), size + sizeof(int));
        }

        void AppendLong(long value)
        {
            Write(0x27);
            Write(BitConverter.GetBytes(value), size + sizeof(long));
        }

        void AppendByte(byte value)
        {
            Write(0x28);
            Write(value, size + sizeof(byte));
        }

        void AppendUShort(ushort value)
        {
            Write(0x29);
            Write(BitConverter.GetBytes(value), size + sizeof(ushort));
        }

        void AppendUInt(uint value)
        {
            Write(0x2b);
            Write(BitConverter.GetBytes(value), size + sizeof(uint));
        }

        void AppendULong(ulong value)
        {
            Write(0x2f);
            Write(BitConverter.GetBytes(value), size + sizeof(ulong));
        }

        //appendSmallInt(long value)
        void AppendSmallInt(long value)
        {
            if (value >= 0)
            {
                Write((byte)(value + 0x30));
            }
            else
            {
                Write((byte)(value + 0x40));
            }
        }

        void AppendSmallInt(ulong value)
        {
            Write((byte)(value + 0x30));
        }

        void AppendDouble(double value)
        {
            Write(0x1b);
            Write(BitConverter.GetBytes((ulong)BitConverter.DoubleToInt64Bits(value)), size + sizeof(ulong));
        }

        void AppendDateTime(DateTime value)
        {
            Write(0x1c);
            var st = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan t = value.ToUniversalTime() - st;
            Write(BitConverter.GetBytes((long)t.TotalMilliseconds), size + sizeof(long));
        }

        void AppendDateTimeOffset(DateTimeOffset value)
        {
            Write(0x1c);
            var st = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);
            TimeSpan t = value.ToUniversalTime() - st;
            Write(BitConverter.GetBytes((long)t.TotalMilliseconds), size + sizeof(long));
        }

        void AppendTimeSpan(TimeSpan t)
        {
            AppendLong(t.Ticks);
        }

        //appendString(String value)
        void AppendString(string value)
        {
            int length = Encoding.UTF8.GetBytes(value).Length;
            if (length <= 126)
            {
                // short string
                Write((byte)(0x40 + length));
            }
            else
            {
                // long string
                Write(0xbf);
                AppendLength(length);
            }
            try
            {
                byte[] bytes = Encoding.UTF8.GetBytes(value);
                EnsureCapacity(size + bytes.Length);
                System.Buffer.BlockCopy(bytes, 0, buffer, size, bytes.Length);
                size += bytes.Length;
            }
            catch (Exception e)
            {
                throw new VPackBuilderException(e.Message, e);
            }
        }

        //appendBinary(byte[] value)
        void AppendBinary(byte[] value)
        {
            Write(0xc3);
            Write(BitConverter.GetBytes(value.Length), size + sizeof(int));
            EnsureCapacity(size + value.Length);
            System.Buffer.BlockCopy(value, 0, buffer, size, value.Length);
            size += value.Length;
        }

        //appendLength(long length)
        void AppendLength(long length)
        {
            Write(BitConverter.GetBytes(length), size + sizeof(long));
        }

        //appendNull()
        void AppendNull()
        {
            Write(0x18);
        }

        //appendBoolean(boolean value)
        void AppendBool(bool value)
        {
            if (value)
            {
                Write(0x1a);
            }
            else
            {
                Write(0x19);
            }
        }

        //appendVPack(VPackSlice value)
        void AppendVPack(VPackSlice value)
        {
            byte[] vpack = value.GetRawVPack();
            EnsureCapacity(size + vpack.Length);
            System.Buffer.BlockCopy(vpack, 0, buffer, size, vpack.Length);
            size += vpack.Length;
        }

        void AppendValueType(SliceType value, bool? unindexed = null)
        {
            switch (value)
            {
                case SliceType.Null:
                    AppendNull();
                    break;
                case SliceType.Array:
                    AddArray(unindexed.Value);
                    break;
                case SliceType.Object:
                    AddObject(unindexed.Value);
                    break;
                default:
                    throw new VPackValueTypeException(SliceType.Array, SliceType.Object, SliceType.Null);
            }
        }

        #endregion

        void ReportAdd()
        {
            List<int> depth = index[stack.Count - 1];
            depth.Add(size - stack[stack.Count - 1]);
        }

        void CleanupAdd()
        {
            List<int> depth = index[stack.Count - 1];
            depth.Remove(depth.Count - 1);
        }

        byte Head()
        {
            int _in = stack[stack.Count - 1];
            return buffer[_in];
        }

        bool IsClosed()
        {
            return stack.Count == 0;
        }

        VPackBuilder WrapAdd(object value, Action append)
        {
            bool haveReported = false;
            if (stack.Count != 0 && !keyWritten)
            {
                ReportAdd();
                haveReported = true;
            }
            try
            {
                if (value == null)
                {
                    AppendNull();
                }
                else
                {
                    append();
                }
            }
            catch (VPackBuilderException e)
            {
                // clean up in case of an exception
                if (haveReported)
                {
                    CleanupAdd();
                }
                throw e;
            }
            return this;
        }

        VPackBuilder WrapAdd(string attribute, object value, Action append)
        {
            if (attribute != null)
            {
                bool haveReported = false;
                if (stack.Count != 0)
                {
                    byte head = Head();
                    if (head != 0x0b && head != 0x14)
                    {
                        throw new VPackBuilderNeedOpenObjectException();
                    }
                    if (keyWritten)
                    {
                        throw new VPackBuilderKeyAlreadyWrittenException();
                    }
                    ReportAdd();
                    haveReported = true;
                }
                try
                {
                    if (VelocyPack.VPackSlice.attributeTranslator != null)
                    {
                        VPackSlice translate = VelocyPack.VPackSlice.attributeTranslator.Translate(attribute);
                        if (translate != null)
                        {
                            byte[] trValue = translate.GetRawVPack();
                            EnsureCapacity(size + trValue.Length);
                            for (int i = 0; i < trValue.Length; i++)
                            {
                                WriteUnchecked(trValue[i]);
                            }
                            keyWritten = true;
                            if (value == null)
                            {
                                AppendNull();
                            }
                            else
                            {
                                append();
                            }
                            return this;
                        }
                        // otherwise fall through to regular behavior
                    }
                    AppendString(attribute);
                    keyWritten = true;
                    if (value == null)
                    {
                        AppendNull();
                    }
                    else
                    {
                        append();
                    }
                }
                catch (VPackBuilderException e)
                {
                    // clean up in case of an exception
                    if (haveReported)
                    {
                        CleanupAdd();
                    }
                    throw e;
                }
                finally
                {
                    keyWritten = false;
                }
            }
            else
            {
                WrapAdd(value, append);
            }
            return this;
        }

        void AddArray(bool unindexed)
        {
            AddCompoundValue((byte)(unindexed ? 0x13 : 0x06));
        }

        void AddObject(bool unindexed)
        {
            AddCompoundValue((byte)(unindexed ? 0x14 : 0x0b));
        }

        void AddCompoundValue(byte head)
        {
            // an Array or Object is started:
            stack.Add(size);
            index[stack.Count - 1] = new List<int>();

            Write((byte)head);
            // Will be filled later with bytelength and nr subs
            size += 8;
            EnsureCapacity(size);
        }

        public VPackSlice Slice()
        {
            return new VPackSlice(buffer);
        }

        public int Size => size;

        public byte[] Buffer => buffer;

        void Remove(int index)
        {
            int numMoved = size - index - 1;
            if (numMoved > 0)
            {
                System.Buffer.BlockCopy(buffer, index + 1, buffer, index, numMoved);
            }
            buffer[--size] = 0;
        }

        void StoreVariableValueLength(int offset, long value, bool reverse)
        {
            int i = offset;
            long val = value;
            if (reverse)
            {
                while (val >= 0x80)
                {
                    buffer[--i] = (byte)((byte)(val & 0x7f) | (byte)0x80);
                    val >>= 7;
                }
                buffer[--i] = (byte)(val & 0x7f);
            }
            else
            {
                while (val >= 0x80)
                {
                    buffer[++i] = (byte)((byte)(val & 0x7f) | (byte)0x80);
                    val >>= 7;
                }
                buffer[++i] = (byte)(val & 0x7f);
            }
        }

        #region SortEntry

        class SortEntry
        {
            public VPackSlice Slice { get; set; }
            public int Offset { get; set; }

            public SortEntry(VPackSlice slice, int offset)
            {
                this.Slice = slice;
                Offset = offset;
            }
        }

        class SortEntryComparer : IComparer<SortEntry>
        {
            public int Compare(SortEntry o1, SortEntry o2)
            {
                return o1.Slice.ToStringUnchecked().CompareTo(o2.Slice.ToStringUnchecked());
            }
        }

        void SortObjectIndex(int start, List<int> offsets)
        {

            List<SortEntry> attributes = new List<SortEntry>();
            foreach (var offset in offsets)
            {
                attributes.Add(new SortEntry(new VPackSlice(buffer, start + offset).MakeKey(), offset));
            }

            attributes.Sort(new SortEntryComparer());

            offsets.Clear();
            foreach (var sortEntry in attributes)
            {
                offsets.Add(sortEntry.Offset);
            }
        }

        #endregion

        #region close

        public VPackBuilder Close()
        {
            try
            {
                return Close(true);
            }
            catch (VPackKeyTypeException e)
            {
                throw new VPackBuilderException(e.Message, e);
            }
            catch (VPackNeedAttributeTranslatorException e)
            {
                throw new VPackBuilderException(e.Message, e);
            }
        }

        VPackBuilder CloseArray(int tos, List<int> _in)
        {
            // fix head byte in case a compact Array was originally
            // requested
            buffer[tos] = 0x06;

            bool needIndexTable = true;
            bool needNrSubs = true;
            int n = _in.Count;
            if (n == 1)
            {
                needIndexTable = false;
                needNrSubs = false;
            }
            else if ((size - tos) - _in[0] == n * (_in[1] - _in[0]))
            {
                // In this case it could be that all entries have the same length
                // and we do not need an offset table at all:
                bool noTable = true;
                int subLen = _in[1] - _in[0];
                if ((size - tos) - _in[n - 1] != subLen)
                {
                    noTable = false;
                }
                else
                {
                    for (int i = 1; i < n - 1; i++)
                    {
                        if (_in[i + 1] - _in[i] != subLen)
                        {
                            noTable = false;
                            break;
                        }
                    }
                }
                if (noTable)
                {
                    needIndexTable = false;
                    needNrSubs = false;
                }
            }

            // First determine byte length and its format:
            int offsetSize;
            // can be 1, 2, 4 or 8 for the byte width of the offsets,
            // the byte length and the number of subvalues:
            if ((size - 1 - tos) + (needIndexTable ? n : 0) - (needNrSubs ? 6 : 7) <= 0xff)
            {
                // We have so far used _pos - tos bytes, including the reserved 8
                // bytes for byte length and number of subvalues. In the 1-byte
                // number
                // case we would win back 6 bytes but would need one byte per
                // subvalue
                // for the index table
                offsetSize = 1;
            }
            else if ((size - 1 - tos) + (needIndexTable ? 2 * n : 0) <= 0xffff)
            {
                offsetSize = 2;
            }
            else if (((size - 1 - tos) / 2) + ((needIndexTable ? 4 * n : 0) / 2) <= int.MaxValue/* 0xffffffffu */)
            {
                offsetSize = 4;
            }
            else
            {
                offsetSize = 8;
            }
            // Maybe we need to move down data
            if (offsetSize == 1)
            {
                int targetPos = 3;
                if (!needIndexTable)
                {
                    targetPos = 2;
                }
                if ((size - 1) > (tos + 9))
                {
                    for (int i = tos + targetPos; i < tos + 9; i++)
                    {

                        Remove(tos + targetPos);
                    }
                }
                int diff = 9 - targetPos;
                if (needIndexTable)
                {
                    for (int i = 0; i < n; i++)
                    {
                        _in[i] = _in[i] - diff;
                    }
                } // Note: if !needIndexTable the index is now wrong!
            }
            // One could move down things in the offsetSize == 2 case as well,
            // since we only need 4 bytes in the beginning. However, saving these
            // 4 bytes has been sacrificed on the Altar of Performance.

            // Now build the table:
            if (needIndexTable)
            {
                // int tableBase = size;
                for (int i = 0; i < n; i++)
                {
                    long xx = _in[i];

                    EnsureCapacity(size + offsetSize);
                    for (int j = 0; j < offsetSize; j++)
                    {

                        WriteUnchecked(/* tableBase + offsetSize * i + j, */ (byte)(xx & 0xff));
                        xx >>= 8;
                    }
                }
            }
            else
            { // no index table
                buffer[tos] = (byte)0x02;
            }
            // Finally fix the byte width in the type byte:
            if (offsetSize > 1)
            {
                if (offsetSize == 2)
                {
                    buffer[tos] = (byte)(buffer[tos] + 1);
                }
                else if (offsetSize == 4)
                {
                    buffer[tos] = (byte)(buffer[tos] + 2);
                }
                else
                { // offsetSize == 8
                    buffer[tos] = (byte)(buffer[tos] + 3);
                    if (needNrSubs)
                    {

                        AppendLength(n);
                    }
                }
            }
            // Fix the byte length in the beginning
            long x = size - tos;
            for (int i = 1; i <= offsetSize; i++)
            {
                buffer[tos + i] = (byte)(x & 0xff);
                x >>= 8;
            }
            // set the number of items in the beginning
            if (offsetSize < 8 && needNrSubs)
            {
                x = n;
                for (int i = offsetSize + 1; i <= 2 * offsetSize; i++)
                {
                    buffer[tos + i] = (byte)(x & 0xff);
                    x >>= 8;
                }
            }
            stack.RemoveAt(stack.Count - 1);
            return this;
        }

        bool CloseCompactArrayOrObject(int tos, bool isArray, List<int> _in)
        {
            // use the compact Array / Object format
            long nLen = NumberUtil.GetVariableValueLength(_in.Count);
            long byteSize = size - (tos + 8) + nLen;
            long bLen = NumberUtil.GetVariableValueLength(byteSize);
            byteSize += bLen;
            if (NumberUtil.GetVariableValueLength(byteSize) != bLen)
            {
                byteSize += 1;
                bLen += 1;
            }
            if (bLen < 9)
            {
                // can only use compact notation if total byte length is at most
                // 8 bytes long
                buffer[tos] = (byte)(isArray ? 0x13 : 0x14);
                int targetPos = (int)(1 + bLen);
                if (size - 1 > (tos + 9))
                {
                    for (int i = tos + targetPos; i < tos + 9; i++)
                    {
                        Remove(tos + targetPos);
                    }
                }
                // store byte length
                StoreVariableValueLength(tos, byteSize, false);
                // need additional memory for storing the number of values
                if (nLen > 8 - bLen)
                {
                    EnsureCapacity((int)(size + nLen));
                }
                // store number of values
                StoreVariableValueLength((int)(tos + byteSize), _in.Count, true);
                size += (int)nLen;
                stack.RemoveAt(stack.Count - 1);
                return true;
            }
            return false;
        }


        VPackBuilder CloseEmptyArrayOrObject(int tos, bool isArray)
        {
            // empty Array or Object
            buffer[tos] = (byte)(isArray ? 0x01 : 0x0a);
            // no bytelength and number subvalues needed
            for (int i = 1; i <= 8; i++)
            {
                Remove(tos + 1);
            }
            stack.RemoveAt(stack.Count - 1);
            return this;
        }

        VPackBuilder Close(bool sort)
        {
            if (IsClosed())
            {
                throw new VPackBuilderNeedOpenCompoundException();
            }
            byte head = Head();
            bool isArray = head == 0x06 || head == 0x13;
            List<int> _in = index[stack.Count - 1];
            int tos = stack[stack.Count - 1];
            if (_in.Count == 0)
            {
                return CloseEmptyArrayOrObject(tos, isArray);
            }
            if (head == 0x13 || head == 0x14 || (head == 0x06 && options.IsBuildUnindexedArrays())
                    || head == 0x0b && (options.IsBuildUnindexedObjects() || _in.Count == 1))
            {
                if (CloseCompactArrayOrObject(tos, isArray, _in))
                {
                    return this;

                }
                // This might fall through, if closeCompactArrayOrObject gave up!
            }
            if (isArray)
            {
                return CloseArray(tos, _in);
            }
            // fix head byte in case a compact Array / Object was originally
            // requested
            buffer[tos] = (byte)0x0b;

            // First determine byte length and its format:
            int offsetSize;
            // can be 1, 2, 4 or 8 for the byte width of the offsets,
            // the byte length and the number of subvalues:
            if (size - tos + _in.Count - 6 <= 0xff)
            {
                // We have so far used _pos - tos bytes, including the reserved 8
                // bytes for byte length and number of subvalues. In the 1-byte
                // number
                // case we would win back 6 bytes but would need one byte per
                // subvalue
                // for the index table
                offsetSize = 1;
            }
            else if ((size - 1 - tos) + 2 * _in.Count <= 0xffff)
            {
                offsetSize = 2;
            }
            else if (((size - 1 - tos) / 2) + 4 * _in.Count / 2 <= int.MaxValue/* 0xffffffffu */)
            {
                offsetSize = 4;
            }
            else
            {
                offsetSize = 8;
            }
            // Maybe we need to move down data
            if (offsetSize == 1)
            {
                int targetPos = 3;
                if ((size - 1) > (tos + 9))
                {
                    for (int i = tos + targetPos; i < tos + 9; i++)
                    {
                        Remove(tos + targetPos);
                    }
                }
                int diff = 9 - targetPos;
                int n = _in.Count;
                for (int i = 0; i < n; i++)
                {
                    _in[i] = _in[i] - diff;
                }
            }

            // One could move down things in the offsetSize == 2 case as well,
            // since we only need 4 bytes in the beginning. However, saving these
            // 4 bytes has been sacrificed on the Altar of Performance.

            // Now build the table:
            if (sort && _in.Count >= 2)
            {
                // Object
                SortObjectIndex(tos, _in);
            }
            // int tableBase = size;
            for (int i = 0; i < _in.Count; i++)
            {
                long xx = _in[i];

                EnsureCapacity(size + offsetSize);
                for (int j = 0; j < offsetSize; j++)
                {

                    WriteUnchecked(/* tableBase + offsetSize * i + j, */ (byte)(xx & 0xff));
                    xx >>= 8;
                }
            }
            // Finally fix the byte width in the type byte:
            if (offsetSize > 1)
            {
                if (offsetSize == 2)
                {
                    buffer[tos] = (byte)(buffer[tos] + 1);
                }
                else if (offsetSize == 4)
                {
                    buffer[tos] = (byte)(buffer[tos] + 2);
                }
                else
                { // offsetSize == 8
                    buffer[tos] = (byte)(buffer[tos] + 3);

                    AppendLength(_in.Count);
                }
            }
            // Fix the byte length in the beginning
            long x = size - tos;
            for (int i = 1; i <= offsetSize; i++)
            {
                buffer[tos + i] = (byte)(x & 0xff);
                x >>= 8;
            }
            // set the number of items in the beginning
            if (offsetSize < 8)
            {
                x = _in.Count;
                for (int i = offsetSize + 1; i <= 2 * offsetSize; i++)
                {
                    buffer[tos + i] = (byte)(x & 0xff);
                    x >>= 8;
                }
            }
            stack.RemoveAt(stack.Count - 1);
            return this;
        }

        #endregion
    }
}

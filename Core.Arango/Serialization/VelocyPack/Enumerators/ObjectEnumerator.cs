using ArangoDB.VelocyPack.Exceptions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArangoDB.VelocyPack.Enumerators
{
    public class ObjectEnumerator : IEnumerator<KeyValuePair<string, VPackSlice>>
    {
        VPackSlice slice;
        KeyValuePair<string, VPackSlice> _current;
        long size;
        long position;
        long current_position;

        public ObjectEnumerator(VPackSlice slice)
        {
            this.slice = slice;
            size = slice.Length;
            position = 0;

            if (!slice.IsType(SliceType.Object))
            {
                throw new VPackValueTypeException(SliceType.Object);
            }
            if (size > 0)
            {
                byte head = slice.TypeCode;
                if (head == 0x14)
                {
                    current_position = slice.KeyAt(0).Start;
                }
                else
                {
                    current_position = slice.Start + slice.FindDataOffset();
                }
            }
        }

        public KeyValuePair<string, VPackSlice> Current
        {
            get
            {
                return _current;
            }
        }

        object IEnumerator.Current
        {
            get
            {
                return _current;
            }
        }

        VPackSlice GetCurrent()
        {
            return new VPackSlice(slice.Buffer, (int)current_position);
        }

        public bool MoveNext()
        {
            if (position++ > 0)
            {
                if (position <= size && current_position != 0)
                {
                    // skip over key
                    current_position += GetCurrent().GetByteSize();
                    // skip over value
                    current_position += GetCurrent().GetByteSize();
                }
                else
                {
                    return false;
                }
            }

            VPackSlice currentField = GetCurrent();

            string key;
            try
            {
                key = currentField.MakeKey().ToStringUnchecked();
            }
            catch (VPackKeyTypeException) {
                return false;
            } catch (VPackNeedAttributeTranslatorException) {
                return false;
            }

            VPackSlice value = new VPackSlice(currentField.Buffer, currentField.Start + currentField.GetByteSize());

            _current = new KeyValuePair<string, VPackSlice>(key, value);

            return true;
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
        }
    }
}

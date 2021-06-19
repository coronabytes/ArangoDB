using ArangoDB.VelocyPack.Exceptions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArangoDB.VelocyPack.Enumerators
{
    public class ArrayEnumerator : IEnumerator<VPackSlice>
    {
        VPackSlice slice;
        VPackSlice _current;
        long position;
        long size;

        public ArrayEnumerator(VPackSlice slice)
        {
            if (!slice.IsType(SliceType.Array))
            {
                throw new VPackValueTypeException(SliceType.Array);
            }
            this.slice = slice;
            size = slice.Length;
            position = 0;
        }

        public VPackSlice Current
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
        
        public bool MoveNext()
        {
            if (position < size)
            {
                _current = slice[(int)position++];
                return true;
            }
            else
            {
                return false;
            }
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

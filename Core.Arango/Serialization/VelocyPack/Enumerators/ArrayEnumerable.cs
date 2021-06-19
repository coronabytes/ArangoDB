using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArangoDB.VelocyPack.Enumerators
{
    public class ArrayEnumerable : IEnumerable<VPackSlice>
    {
        VPackSlice slice;

        public ArrayEnumerable(VPackSlice slice)
        {
            this.slice = slice;
        }

        public IEnumerator<VPackSlice> GetEnumerator()
        {
            return new ArrayEnumerator(slice);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new ArrayEnumerator(slice);
        }
    }
}

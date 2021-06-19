using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArangoDB.VelocyPack.Enumerators
{
    public class ObjectEnumerable : IEnumerable<KeyValuePair<string, VPackSlice>>
    {
        VPackSlice slice;

        public ObjectEnumerable(VPackSlice slice)
        {
            this.slice = slice;
        }

        public IEnumerator<KeyValuePair<string, VPackSlice>> GetEnumerator()
        {
            return new ObjectEnumerator(slice);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new ObjectEnumerator(slice);
        }
    }
}

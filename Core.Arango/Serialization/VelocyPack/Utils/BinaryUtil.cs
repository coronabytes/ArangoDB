using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArangoDB.VelocyPack.Utils
{
    public class BinaryUtil
    {
        private BinaryUtil()
        {
        }

        public static byte[] ToBinary(byte[] array, int offset, int length)
        {
            byte[] result = new byte[length];
            for (int i = offset, j = 0; j < length; i++, j++)
            {
                result[j] = array[i];
            }
            return result;
        }
    }
}

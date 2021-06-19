using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArangoDB.VelocyPack.Utils
{
    public class NumberUtil
    {
        private NumberUtil()
        {
        }

        public static long ToLong(byte[] array, int offset, int length)
        {
            long result = 0;
            for (int i = (offset + length - 1); i >= offset; i--)
            {
                result <<= 8;
                result |= (array[i] & 0xFF);
            }
            return result;
        }

        /**
         * read a variable length integer in unsigned LEB128 format
         */
        public static long ReadVariableValueLength(byte[] array, int offset, bool reverse)
        {
            long len = 0;
            byte v;
            // long to int
            int p = 0;
            int i = offset;
            do
            {
                v = array[i];
                len += ((long)(v & 0x7f)) << p;
                p += 7;
                if (reverse)
                {
                    --i;
                }
                else
                {
                    ++i;
                }
            } while ((v & 0x80) != 0);
            return len;
        }

        /**
         * calculate the length of a variable length integer in unsigned LEB128 format
         */
        public static long GetVariableValueLength(long value)
        {
            long len = 1;
            long val = value;
            while (val >= 0x80)
            {
                val >>= 7;
                ++len;
            }
            return len;
        }

    }
}

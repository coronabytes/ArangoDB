using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArangoDB.VelocyPack.Exceptions
{
    public class VPackValueTypeException : VPackBuilderException
    {
        static string CreateMessage(params SliceType[] types)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Expecting type ");
            for (int i = 0; i < types.Length; i++)
            {
                if (i > 0)
                {
                    sb.Append(" or ");
                }
                sb.Append(types[i].ToString());
            }
            return sb.ToString();
        }

        public VPackValueTypeException(params SliceType[] types)
            : base(CreateMessage(types))
        {
        }

        public VPackValueTypeException()
            : base()
        {
        }

        public VPackValueTypeException(string message)
            : base(message)
        {
        }

        public VPackValueTypeException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}

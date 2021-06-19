using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArangoDB.VelocyPack.Exceptions
{
    public class VPackBuilderUnexpectedValueException : VPackBuilderException
    {
        static String CreateMessage(SliceType type, string specify, params Type[] declaringTypes)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Must give ");
            if (specify != null)
            {
                sb.Append(specify);
                sb.Append(" ");
            }
            for (int i = 0; i < declaringTypes.Length; i++)
            {
                if (i > 0)
                {
                    sb.Append(" or ");
                }
                sb.Append(declaringTypes[i].Name);
            }
            sb.Append(" for ");
            sb.Append(type.ToString());
            return sb.ToString();
        }

        public VPackBuilderUnexpectedValueException(SliceType type, params Type[] declaringTypes)
            : base(CreateMessage(type, null, declaringTypes))
        {
        }

        public VPackBuilderUnexpectedValueException(SliceType type, string specify, params Type[] declaringTypes)
            : base(CreateMessage(type, specify, declaringTypes))
        {
        }
    }
}

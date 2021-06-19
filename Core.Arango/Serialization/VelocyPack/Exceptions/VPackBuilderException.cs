using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArangoDB.VelocyPack.Exceptions
{
    public class VPackBuilderException : VPackException
    {
        public VPackBuilderException()
            : base()
        {
        }

        public VPackBuilderException(string message)
            : base(message)
        {
        }

        public VPackBuilderException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}

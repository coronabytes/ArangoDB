using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArangoDB.VelocyPack.Exceptions
{
    public class VPackException : Exception
    {
        public VPackException()
            : base()
        {
        }

        public VPackException(string message)
            : base(message)
        {
        }

        public VPackException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}

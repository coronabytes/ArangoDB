using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArangoDB.VelocyPack.Exceptions
{
    public class VPackBuilderKeyAlreadyWrittenException : VPackBuilderException
    {
        public VPackBuilderKeyAlreadyWrittenException()
            : base()
        {
        }
    }
}

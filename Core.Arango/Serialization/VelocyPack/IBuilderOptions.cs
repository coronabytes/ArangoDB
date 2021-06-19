using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArangoDB.VelocyPack
{
    public interface IBuilderOptions
    {
        bool IsBuildUnindexedArrays();

        void SetBuildUnindexedArrays(bool buildUnindexedArrays);
        
        bool IsBuildUnindexedObjects();

        void SetBuildUnindexedObjects(bool buildUnindexedObjects);

        bool IsBuildCompactIntegers();

        void SetBuildCompactIntegers(bool buildCompactIntegers);
    }
}

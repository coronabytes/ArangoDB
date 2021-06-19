using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArangoDB.VelocyPack
{
    public class DefaultVPackBuilderOptions : IBuilderOptions
    {
        private bool _buildUnindexedArrays;
        private bool _buildUnindexedObjects;
        private bool _buildCompactIntegers;

        public DefaultVPackBuilderOptions()
        {
            _buildUnindexedArrays = false;
            _buildUnindexedObjects = false;
            _buildCompactIntegers = true;
        }
        
        public bool IsBuildUnindexedArrays()
        {
            return _buildUnindexedArrays;
        }
        
        public void SetBuildUnindexedArrays(bool buildUnindexedArrays)
        {
            _buildUnindexedArrays = buildUnindexedArrays;
        }
        
        public bool IsBuildUnindexedObjects()
        {
            return _buildUnindexedObjects;
        }
        
        public void SetBuildUnindexedObjects(bool buildUnindexedObjects)
        {
            _buildUnindexedObjects = buildUnindexedObjects;
        }

        public bool IsBuildCompactIntegers()
        {
            return _buildCompactIntegers;
        }

        public void SetBuildCompactIntegers(bool buildCompactIntegers)
        {
            _buildCompactIntegers = buildCompactIntegers;
        }
    }
}

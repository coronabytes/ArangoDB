using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArangoDB.VelocyPack
{
    public enum SliceType
    {
        None,
        Illegal,
        Null,
        Boolean,
        Array,
        Object,
        Double,
        UtcDate,
        External,
        MinKey,
        MaxKey,
        Int,
        UInt,
        SmallInt,
        String,
        Binary,
        Bcd,
        Custom,
        VPack
    }
}

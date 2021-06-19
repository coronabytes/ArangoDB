using ArangoDB.VelocyPack.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArangoDB.VelocyPack
{
    public class VPackAttributeTranslator : IVPackAttributeTranslator
    {
        private static string KEY = "_key";
        private static string REV = "_rev";
        private static string ID = "_id";
        private static string FROM = "_from";
        private static string TO = "_to";

        private static byte KEY_ATTRIBUTE = 0x31;
        private static byte REV_ATTRIBUTE = 0x32;
        private static byte ID_ATTRIBUTE = 0x33;
        private static byte FROM_ATTRIBUTE = 0x34;
        private static byte TO_ATTRIBUTE = 0x35;
        private static byte ATTRIBUTE_BASE = 0x30;

        private VPackBuilder builder;
        private Dictionary<string, VPackSlice> attributeToKey;
        private Dictionary<int, VPackSlice> keyToAttribute;

        public VPackAttributeTranslator()
        {
            builder = null;
            attributeToKey = new Dictionary<string, VPackSlice>();
            keyToAttribute = new Dictionary<int, VPackSlice>();
            try
            {
                Add(KEY, KEY_ATTRIBUTE - ATTRIBUTE_BASE);
                Add(REV, REV_ATTRIBUTE - ATTRIBUTE_BASE);
                Add(ID, ID_ATTRIBUTE - ATTRIBUTE_BASE);
                Add(FROM, FROM_ATTRIBUTE - ATTRIBUTE_BASE);
                Add(TO, TO_ATTRIBUTE - ATTRIBUTE_BASE);
                Seal();
            }
            catch (VPackException e)
            {
                throw new Exception(e.Message, e);
            }
        }

        public void Add(string attribute, int key)
        {
            if (builder == null)
            {
                builder = new VPackBuilder();
                builder.Add(SliceType.Object);
            }
            builder.AddCompactInt(attribute, key);
        }

        public void Seal()
        {
            if (builder == null)
            {
                return;
            }
            builder.Close();
            VPackSlice slice = builder.Slice();
            for (int i = 0; i < slice.Length; i++)
            {
                VPackSlice key = slice.KeyAt(i);
                VPackSlice value = slice.ValueAt(i);
                attributeToKey[key.ToStringUnchecked()] = value;
                keyToAttribute[value.ToInt32().Value] = key;
            }
        }

        public VPackSlice Translate(string attribute)
        {
            VPackSlice slice = null;
            attributeToKey.TryGetValue(attribute, out slice);
            return slice;
        }

        public VPackSlice Translate(int key)
        {
            VPackSlice slice = null;
            keyToAttribute.TryGetValue(key, out slice);
            return slice;
        }

    }
}

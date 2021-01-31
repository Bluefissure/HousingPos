using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HousingPos.Objects
{
    public class HousingItem
    {
        public static HousingItem Empty => new HousingItem(0, 0, 0, 0, 0, 0, "null");

        public ushort ModelKey;
        public uint ItemKey;
        public float X;
        public float Y;
        public float Z;
        public float Rotate;
        public string Name;
        public bool HiddenOnScreen = false;

        public HousingItem(ushort modelKey, uint itemKey, float x, float y, float z, float rotate, string name)
        {
            ModelKey = modelKey;
            ItemKey = itemKey;
            X = x;
            Y = y;
            Z = z;
            Rotate = rotate;
            Name = name;
        }
    }
}

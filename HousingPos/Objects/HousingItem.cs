using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace HousingPos.Objects
{
    public class HousingItem
    {
        public static HousingItem Empty => new HousingItem(0, 0, 0, 0, 0, 0, 0, "null");

        public ushort ModelKey;
        public uint ItemKey;
        public byte Stain;
        public float X;
        public float Y;
        public float Z;
        public float Rotate;
        public string Name;
        public List<HousingItem> children = new List<HousingItem>();
        // Relative distance to base furniture in spherical coordinate
        // only useful when it's a child of a base item
        // (radial distance, azimuthal angle, polar angle)
        public Vector4 relative = Vector4.Zero;

        public HousingItem(ushort modelKey, uint itemKey, byte stain, float x, float y, float z, float rotate, string name)
        {
            ModelKey = modelKey;
            ItemKey = itemKey;
            Stain = stain;
            X = x;
            Y = y;
            Z = z;
            Rotate = rotate;
            Name = name;
        }

        public Vector4 CalcRelativeTo(HousingItem baseItem)
        {
            // be careful that x, y, z in game are different from traditional orthogonal
            // traditional: X Y Z
            // in-game:     Z X Y
            // all variables here refer to in-game ones
            double dx = this.X - baseItem.X;
            double dy = this.Y - baseItem.Y;
            double dz = this.Z - baseItem.Z;
            float r = (float)Math.Sqrt(dx * dx + dy * dy + dz * dz);
            float theta = (float)Math.Acos(dy / r);
            float phi = (float)(Math.Atan2(dx, dz) - baseItem.Rotate);
            relative = new Vector4(r, theta, phi, (float)(this.Rotate - baseItem.Rotate));
            return relative;
        }

        public void ReCalcChildrenPos()
        {
            for(int i = 0; i < children.Count; i++)
            {
                var relative = children[i].relative;
                float r = relative.X;
                float theta = relative.Y;
                float phi = Rotate + relative.Z;
                while (phi > Math.PI) phi -= (float)(Math.PI * 2);
                while (phi < -Math.PI) phi += (float)(Math.PI * 2);
                children[i].Rotate = Rotate + relative.W ;
                children[i].Y = (float)(r * Math.Cos(theta)) + Y;
                children[i].X = (float)(r * Math.Sin(theta) * Math.Sin(phi)) + X;
                children[i].Z = (float)(r * Math.Sin(theta) * Math.Cos(phi)) + Z;
            }

        }

    }
}

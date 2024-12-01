using UnityEngine;

namespace MENet.FrameSync
{
    public static class FixedMath
    {
        public const int DECIMAL_PLACES = 2;
        public const long PRECISION = 100;

        public static FixedPoint Sqrt(FixedPoint value)
        {
            if (value == FixedPoint.Zero) return FixedPoint.Zero;
            
            float floatValue = value.ToFloat();
            return FixedPoint.FromRaw((long)(Mathf.Sqrt(floatValue) * PRECISION));
        }

        public static FixedPoint Sin(FixedPoint value)
        {
            float floatValue = value.ToFloat();
            return FixedPoint.FromRaw((long)(Mathf.Sin(floatValue) * PRECISION));
        }

        public static FixedPoint Cos(FixedPoint value)
        {
            float floatValue = value.ToFloat();
            return FixedPoint.FromRaw((long)(Mathf.Cos(floatValue) * PRECISION));
        }

        public static FixedPoint Acos(FixedPoint value)
        {
            float floatValue = value.ToFloat();
            return FixedPoint.FromRaw((long)(Mathf.Acos(floatValue) * PRECISION));
        }

        public static FixedPoint Dot(FixedVector3 a, FixedVector3 b)
        {
            return a.x * b.x + a.y * b.y + a.z * b.z;
        }

        public static FixedPoint Magnitude(FixedVector3 vector)
        {
            return Sqrt(vector.x * vector.x + vector.y * vector.y + vector.z * vector.z);
        }

        public static FixedVector3 Normalize(FixedVector3 vector)
        {
            FixedPoint mag = Magnitude(vector);
            if (mag > FixedPoint.Zero)
            {
                return vector * (FixedPoint.One / mag);
            }
            return FixedVector3.zero;
        }
    }
} 

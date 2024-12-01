using UnityEngine;

namespace MENet.FrameSync
{
    public struct FixedVector3
    {
        public FixedPoint x, y, z;

        public static readonly FixedVector3 zero = new FixedVector3(FixedPoint.Zero, FixedPoint.Zero, FixedPoint.Zero);
        public static readonly FixedVector3 one = new FixedVector3(FixedPoint.One, FixedPoint.One, FixedPoint.One);
        public static readonly FixedVector3 forward = new FixedVector3(FixedPoint.Zero, FixedPoint.Zero, FixedPoint.One);
        public static readonly FixedVector3 back = new FixedVector3(FixedPoint.Zero, FixedPoint.Zero, -FixedPoint.One);
        public static readonly FixedVector3 up = new FixedVector3(FixedPoint.Zero, FixedPoint.One, FixedPoint.Zero);
        public static readonly FixedVector3 down = new FixedVector3(FixedPoint.Zero, -FixedPoint.One, FixedPoint.Zero);
        public static readonly FixedVector3 right = new FixedVector3(FixedPoint.One, FixedPoint.Zero, FixedPoint.Zero);
        public static readonly FixedVector3 left = new FixedVector3(-FixedPoint.One, FixedPoint.Zero, FixedPoint.Zero);

        public FixedVector3(FixedPoint x, FixedPoint y, FixedPoint z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public static FixedVector3 FromRaw(long x, long y, long z)
        {
            return new FixedVector3(
                FixedPoint.FromRaw(x),
                FixedPoint.FromRaw(y),
                FixedPoint.FromRaw(z)
            );
        }

        public static FixedVector3 FromInt(int x, int y, int z)
        {
            return new FixedVector3(
                FixedPoint.FromInt(x),
                FixedPoint.FromInt(y),
                FixedPoint.FromInt(z)
            );
        }

        // 基本運算
        public static FixedVector3 operator +(FixedVector3 a, FixedVector3 b)
        {
            return new FixedVector3(a.x + b.x, a.y + b.y, a.z + b.z);
        }

        public static FixedVector3 operator -(FixedVector3 a, FixedVector3 b)
        {
            return new FixedVector3(a.x - b.x, a.y - b.y, a.z - b.z);
        }

        public static FixedVector3 operator *(FixedVector3 a, FixedPoint b)
        {
            return new FixedVector3(a.x * b, a.y * b, a.z * b);
        }

        public static FixedVector3 operator /(FixedVector3 a, FixedPoint b)
        {
            return new FixedVector3(a.x / b, a.y / b, a.z / b);
        }

        public static FixedVector3 operator -(FixedVector3 a)
        {
            return new FixedVector3(-a.x, -a.y, -a.z);
        }

        // 向量運算
        public static FixedPoint Dot(FixedVector3 a, FixedVector3 b)
        {
            return a.x * b.x + a.y * b.y + a.z * b.z;
        }

        public static FixedVector3 Cross(FixedVector3 a, FixedVector3 b)
        {
            return new FixedVector3(
                a.y * b.z - a.z * b.y,
                a.z * b.x - a.x * b.z,
                a.x * b.y - a.y * b.x
            );
        }

        public FixedPoint SqrMagnitude()
        {
            return x * x + y * y + z * z;
        }

        public FixedPoint Magnitude
        {
            get { return FixedMath.Magnitude(this); }
        }

        public FixedVector3 Normalized
        {
            get { return FixedMath.Normalize(this); }
        }

        // 或者作為方法
        public FixedPoint GetMagnitude()
        {
            return FixedMath.Magnitude(this);
        }

        public FixedVector3 GetNormalized()
        {
            return FixedMath.Normalize(this);
        }

        // Unity 互操作
        public UnityEngine.Vector3 ToVector3()
        {
            return new UnityEngine.Vector3(x.ToFloat(), y.ToFloat(), z.ToFloat());
        }

        public static FixedVector3 FromVector3(UnityEngine.Vector3 v)
        {
            return FromRaw(
                (long)(v.x * FixedPoint.PRECISION),
                (long)(v.y * FixedPoint.PRECISION),
                (long)(v.z * FixedPoint.PRECISION)
            );
        }

        public override string ToString()
        {
            return string.Format("({0}, {1}, {2})", x, y, z);
        }

        public override bool Equals(object obj)
        {
            if (obj is FixedVector3)
            {
                FixedVector3 other = (FixedVector3)obj;
                return x == other.x && y == other.y && z == other.z;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return x.GetHashCode() ^ (y.GetHashCode() << 2) ^ (z.GetHashCode() >> 2);
        }
    }
} 
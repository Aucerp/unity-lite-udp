using UnityEngine;

namespace MENet.FrameSync
{
    public struct FixedQuaternion
    {
        public FixedPoint x, y, z, w;

        public static readonly FixedQuaternion identity = new FixedQuaternion(
            FixedPoint.FromInt(0),
            FixedPoint.FromInt(0),
            FixedPoint.FromInt(0),
            FixedPoint.FromInt(1)
        );

        public FixedQuaternion(FixedPoint x, FixedPoint y, FixedPoint z, FixedPoint w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public static FixedQuaternion FromUnityQuaternion(Quaternion q)
        {
            return new FixedQuaternion(
                FixedPoint.FromRaw((long)(q.x * FixedPoint.PRECISION)),
                FixedPoint.FromRaw((long)(q.y * FixedPoint.PRECISION)),
                FixedPoint.FromRaw((long)(q.z * FixedPoint.PRECISION)),
                FixedPoint.FromRaw((long)(q.w * FixedPoint.PRECISION))
            );
        }

        public Quaternion ToUnityQuaternion()
        {
            return new Quaternion(x.ToFloat(), y.ToFloat(), z.ToFloat(), w.ToFloat());
        }

        public static FixedVector3 operator *(FixedQuaternion rotation, FixedVector3 point)
        {
            FixedPoint two = FixedPoint.FromInt(2);
            FixedPoint one = FixedPoint.FromInt(1);

            FixedPoint num1 = rotation.x * two;
            FixedPoint num2 = rotation.y * two;
            FixedPoint num3 = rotation.z * two;
            FixedPoint num4 = rotation.x * num1;
            FixedPoint num5 = rotation.y * num2;
            FixedPoint num6 = rotation.z * num3;
            FixedPoint num7 = rotation.x * num2;
            FixedPoint num8 = rotation.x * num3;
            FixedPoint num9 = rotation.y * num3;
            FixedPoint num10 = rotation.w * num1;
            FixedPoint num11 = rotation.w * num2;
            FixedPoint num12 = rotation.w * num3;

            FixedVector3 result;
            result.x = (one - (num5 + num6)) * point.x + (num7 - num12) * point.y + (num8 + num11) * point.z;
            result.y = (num7 + num12) * point.x + (one - (num4 + num6)) * point.y + (num9 - num10) * point.z;
            result.z = (num8 - num11) * point.x + (num9 + num10) * point.y + (one - (num4 + num5)) * point.z;
            return result;
        }

        public static FixedQuaternion Euler(FixedVector3 euler)
        {
            FixedPoint half = FixedPoint.FromRaw((long)(0.5f * FixedPoint.PRECISION));
            FixedPoint halfX = euler.x * half;
            FixedPoint halfY = euler.y * half;
            FixedPoint halfZ = euler.z * half;

            FixedPoint sinX = FixedPoint.FromRaw((long)(Mathf.Sin(halfX.ToFloat()) * FixedPoint.PRECISION));
            FixedPoint cosX = FixedPoint.FromRaw((long)(Mathf.Cos(halfX.ToFloat()) * FixedPoint.PRECISION));
            FixedPoint sinY = FixedPoint.FromRaw((long)(Mathf.Sin(halfY.ToFloat()) * FixedPoint.PRECISION));
            FixedPoint cosY = FixedPoint.FromRaw((long)(Mathf.Cos(halfY.ToFloat()) * FixedPoint.PRECISION));
            FixedPoint sinZ = FixedPoint.FromRaw((long)(Mathf.Sin(halfZ.ToFloat()) * FixedPoint.PRECISION));
            FixedPoint cosZ = FixedPoint.FromRaw((long)(Mathf.Cos(halfZ.ToFloat()) * FixedPoint.PRECISION));

            FixedQuaternion result = new FixedQuaternion();
            result.w = cosX * cosY * cosZ + sinX * sinY * sinZ;
            result.x = sinX * cosY * cosZ - cosX * sinY * sinZ;
            result.y = cosX * sinY * cosZ + sinX * cosY * sinZ;
            result.z = cosX * cosY * sinZ - sinX * sinY * cosZ;
            return result;
        }

        public static FixedVector3 forward
        {
            get { return FixedVector3.FromInt(0, 0, 1); }
        }
    }
} 
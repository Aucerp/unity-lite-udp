using UnityEngine;

namespace MENet.FrameSync
{
    public struct RayHitResult
    {
        public bool hit;
        public FixedVector3 point;
        public FixedVector3 normal;
        public FixedPoint distance;
    }

    public static class FixedPhysics
    {
        public static RayHitResult Raycast(FixedVector3 origin, FixedVector3 direction, FixedPoint maxDistance)
        {
            // 使用 Unity 的物理系統進行射線檢測，但結果轉換為定點數
            RaycastHit hit;
            bool hasHit = Physics.Raycast(
                origin.ToVector3(),
                direction.ToVector3(),
                out hit,
                maxDistance.ToFloat()
            );

            RayHitResult result = new RayHitResult();
            result.hit = hasHit;

            if (hasHit)
            {
                result.point = FixedVector3.FromVector3(hit.point);
                result.normal = FixedVector3.FromVector3(hit.normal);
                result.distance = FixedPoint.FromRaw((long)(hit.distance * FixedPoint.PRECISION));
            }

            return result;
        }

        // 可以添加其他物理計算方法，如：
        public static bool BoxCast(FixedVector3 center, FixedVector3 size, FixedQuaternion rotation)
        {
            // 實現盒型碰撞檢測
            return false;
        }

        public static bool SphereCast(FixedVector3 center, FixedPoint radius)
        {
            // 實現球型碰撞檢測
            return false;
        }
    }
} 
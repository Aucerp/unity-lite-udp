using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace MENet.FrameSync
{
    public class FPSFrameSyncManager : FrameSyncManager
    {
        private const int TICK_RATE = 64;  // FPS 遊戲通常使用更高的 tick rate
        private const float TICK_TIME = 1.0f / TICK_RATE;
        
        private List<ShootResult> pendingShootResults;
        private Dictionary<int, List<HitConfirmation>> hitConfirmations;

        public FPSFrameSyncManager()
        {
            pendingShootResults = new List<ShootResult>();
            hitConfirmations = new Dictionary<int, List<HitConfirmation>>();
        }

        protected override void ProcessFrame()
        {
            base.ProcessFrame();
            ProcessPendingShots();
        }

        private void ProcessPendingShots()
        {
            List<ShootResult> shots = new List<ShootResult>(pendingShootResults);
            foreach (ShootResult shot in shots)
            {
                if (ValidateShot(shot))
                {
                    BroadcastHitConfirmation(shot);
                }
                pendingShootResults.Remove(shot);
            }
        }

        private bool ValidateShot(ShootResult shot)
        {
            PlayerState shooter = GetPlayerState(shot.ShooterID);
            PlayerState target = GetPlayerState(shot.TargetID);
            
            if (shooter == null || target == null) return false;

            FixedPoint distance = (target.Position - shooter.Position).Magnitude;
            if (distance > FixedPoint.FromInt(100)) // 最大射程
                return false;

            FixedVector3 toTarget = (target.Position - shooter.Position).Normalized;
            FixedVector3 shooterForward = shooter.Rotation * FixedVector3.forward;
            FixedPoint angle = FixedMath.Acos(FixedMath.Dot(toTarget, shooterForward));
            
            Debug.Log("[FPSFrameSync] 射擊驗證 - 距離:" + distance.ToFloat() + " 角度:" + angle.ToFloat());
            
            return angle < FixedPoint.FromRaw((long)(0.1f * FixedPoint.PRECISION)); // 約5度
        }

        public void AddShootResult(ShootResult result)
        {
            pendingShootResults.Add(result);
        }

        private void BroadcastHitConfirmation(ShootResult shot)
        {
            HitConfirmation confirmation = new HitConfirmation();
            confirmation.ShooterID = shot.ShooterID;
            confirmation.TargetID = shot.TargetID;
            confirmation.Damage = shot.Damage;
            confirmation.Frame = currentFrame;

            // 通過網絡廣播確認
            // ...
        }
    }
} 
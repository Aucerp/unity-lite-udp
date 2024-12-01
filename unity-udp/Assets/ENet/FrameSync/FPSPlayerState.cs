using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace MENet.FrameSync
{
    public class FPSPlayerState : PlayerState
    {
        public bool IsShooting { get; private set; }
        public int Health { get; private set; }

        public FPSPlayerState(int playerId) : base(playerId)
        {
            Health = 100;
        }

        public override void ApplyCommand(InputCommand cmd)
        {
            base.ApplyCommand(cmd);

            if (cmd.Type == CommandType.Shoot)
            {
                ProcessShoot(cmd);
            }
        }

        private void ProcessShoot(InputCommand cmd)
        {
            IsShooting = true;
            
            // 射線檢測使用定點數計算
            var rayStart = Position;
            var rayDirection = Rotation * FixedVector3.forward;
            var rayHit = FixedPhysics.Raycast(rayStart, rayDirection, FixedPoint.FromInt(100));
            
            if (rayHit.hit)
            {
                Debug.Log("[FPSPlayerState] 射擊命中 - 玩家:" + PlayerID + " 位置:" + rayHit.point.ToVector3());
            }
        }

        public override PlayerState Clone()
        {
            FPSPlayerState clone = new FPSPlayerState(PlayerID);
            clone.Position = Position;
            clone.Velocity = Velocity;
            clone.Rotation = Rotation;
            clone.Health = Health;
            clone.IsShooting = IsShooting;
            return clone;
        }
    }
} 
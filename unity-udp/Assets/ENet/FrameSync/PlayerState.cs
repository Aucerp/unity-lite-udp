using UnityEngine;

namespace MENet.FrameSync
{
    public class PlayerState
    {
        public int PlayerID { get; private set; }
        public FixedVector3 Position { get; protected set; }
        public FixedVector3 Velocity { get; protected set; }
        public FixedQuaternion Rotation { get; protected set; }
        public int Frame { get; protected set; }

        public PlayerState(int playerId)
        {
            PlayerID = playerId;
            Position = FixedVector3.FromInt(0, 0, 0);
            Velocity = FixedVector3.FromInt(0, 0, 0);
            Rotation = FixedQuaternion.identity;
            Frame = 0;
        }

        public virtual void SetPosition(FixedVector3 newPosition)
        {
            Position = newPosition;
            Debug.Log("[PlayerState] 設置位置 - 玩家:" + PlayerID + 
                " 新位置:" + Position.ToVector3());
        }

        public virtual void ApplyCommand(InputCommand cmd)
        {
            Frame = cmd.Frame;
            if (cmd.Type == CommandType.Move)
            {
                if (ENetManager.Instance != null && ENetManager.Instance.IsPlayingRecord())
                {
                    Position = cmd.Position;
                    Velocity = cmd.Direction * FixedPoint.FromInt((int)ENetManager.Instance.GetPlayerMoveSpeed());
                    Debug.Log("[PlayerState] 回放位置 - 玩家:" + PlayerID + 
                        " 位置:" + Position.ToVector3() + 
                        " 幀:" + Frame);
                }
                else
                {
                    Position = cmd.Position;
                    Velocity = cmd.Direction * FixedPoint.FromInt((int)ENetManager.Instance.GetPlayerMoveSpeed());
                    Debug.Log("[PlayerState] 更新位置 - 玩家:" + PlayerID + 
                        " 新位置:" + Position.ToVector3() + 
                        " 速度:" + Velocity.ToVector3() +
                        " 幀:" + Frame);
                }
            }
            else if (cmd.Type == CommandType.Look)
            {
                UpdateRotation(cmd.Rotation);
            }
        }

        protected virtual void UpdateMovement(FixedVector3 direction)
        {
            float speedValue = ENetManager.Instance != null ? 
                ENetManager.Instance.GetPlayerMoveSpeed() : 10f;
            
            FixedPoint moveSpeed = FixedPoint.FromInt((int)speedValue);
            
            FixedPoint sqrMagnitude = direction.SqrMagnitude();
            if (sqrMagnitude > FixedPoint.One)
            {
                FixedPoint magnitude = FixedMath.Sqrt(sqrMagnitude);
                direction = direction * (FixedPoint.One / magnitude);
            }
            
            Velocity = direction * moveSpeed;
            Position = Position + (Velocity * FrameSyncManager.FIXED_FRAME_TIME);
            
            Debug.Log("[PlayerState] 更新位置 - 玩家:" + PlayerID + 
                " 方向:" + direction.ToVector3() + 
                " 速度:" + Velocity.ToVector3() + 
                " 新位置:" + Position.ToVector3() +
                " 幀:" + Frame);
        }

        protected virtual void UpdateRotation(FixedQuaternion newRotation)
        {
            Rotation = newRotation;
        }

        public virtual void Update(float deltaTime)
        {
            // 基礎更新邏輯
        }

        public virtual PlayerState Clone()
        {
            PlayerState clone = new PlayerState(PlayerID);
            clone.Position = Position;
            clone.Velocity = Velocity;
            clone.Rotation = Rotation;
            clone.Frame = Frame;
            return clone;
        }
    }
} 
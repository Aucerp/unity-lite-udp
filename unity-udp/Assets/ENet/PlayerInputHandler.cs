using UnityEngine;
using MENet.FrameSync;

namespace MENet
{
    public class PlayerInputHandler : MonoBehaviour
    {
        private NetworkPlayer player;
        private Transform cameraTransform;
        private ENetManager netManager;

        public void Initialize(NetworkPlayer networkPlayer)
        {
            player = networkPlayer;
            cameraTransform = Camera.main.transform;
            netManager = ENetManager.Instance;
        }

        private void Update()
        {
            if (netManager != null && netManager.IsPlayingRecord())
            {
                return;
            }

            ProcessMovementInput();
            ProcessRotationInput();
        }

        private void ProcessMovementInput()
        {
            if (netManager != null && netManager.IsPlayingRecord())
            {
                return;
            }

            Vector3 moveDir = Vector3.zero;

            if (Input.GetKey(KeyCode.W)) moveDir += Vector3.forward;
            if (Input.GetKey(KeyCode.S)) moveDir += Vector3.back;
            if (Input.GetKey(KeyCode.A)) moveDir += Vector3.left;
            if (Input.GetKey(KeyCode.D)) moveDir += Vector3.right;

            if (moveDir != Vector3.zero)
            {
                moveDir = cameraTransform.TransformDirection(moveDir);
                moveDir.y = 0;
                moveDir.Normalize();

                float speedValue = netManager.GetPlayerMoveSpeed();
                FixedVector3 fixedDir = FixedVector3.FromVector3(moveDir);
                FixedPoint moveSpeed = FixedPoint.FromInt((int)speedValue);
                FixedVector3 velocity = fixedDir * moveSpeed;
                FixedVector3 newPosition = player.GetCurrentPosition() + (velocity * FrameSyncManager.FIXED_FRAME_TIME);

                InputCommand cmd = new InputCommand
                {
                    Type = CommandType.Move,
                    PlayerID = 0,
                    Frame = netManager.currentFrame,
                    Direction = fixedDir,
                    Position = newPosition
                };

                player.SendCommand(cmd);
                Debug.Log("[PlayerInputHandler] 發送移動命令 - 當前位置:" + player.GetCurrentPosition().ToVector3() + 
                    " 計算位置:" + newPosition.ToVector3() +
                    " 方向:" + moveDir);
            }
        }

        private void ProcessRotationInput()
        {
            if (netManager != null && netManager.IsPlayingRecord())
            {
                return;
            }

            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            if (Mathf.Abs(mouseX) > 0.01f || Mathf.Abs(mouseY) > 0.01f)
            {
                // 處理旋轉...
            }
        }
    }
} 
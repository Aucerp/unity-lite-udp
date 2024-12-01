using UnityEngine;
using MENet.FrameSync;

namespace MENet
{
    public class NetworkPlayer : MonoBehaviour
    {
        private static readonly bool ENABLE_DEBUG_LOG = false;

        public int PlayerID { get; private set; }
        private FPSPlayerState state;
        private ENetClient client;
        private bool isLocalPlayer;

        public void Initialize(int playerID, bool isLocal = false)
        {
            PlayerID = playerID;
            state = new FPSPlayerState(playerID);
            isLocalPlayer = isLocal;
            
            FixedVector3 initialPosition = FixedVector3.FromVector3(ENetManager.Instance.spawnPoint);
            state.SetPosition(initialPosition);
            transform.position = initialPosition.ToVector3();
            
            if (isLocalPlayer && ENetManager.Instance != null)
            {
                client = ENetManager.Instance.GetClient(playerID);
            }

            if (ENetManager.Instance != null)
            {
                ENetManager.Instance.AddPlayer(playerID, this);
            }

            if (ENABLE_DEBUG_LOG)
            {
                Debug.Log(string.Format("[NetworkPlayer] 初始化 - 玩家:{0} 邏輯位置:{1} Transform位置:{2}",
                    playerID, 
                    state.Position.ToVector3(), 
                    transform.position));
            }
        }

        public void UpdateState(PlayerState newState)
        {
            if (newState is FPSPlayerState)
            {
                var oldPos = state != null ? state.Position.ToVector3() : Vector3.zero;
                state = (FPSPlayerState)newState;
                UpdateVisuals();
                
                if (ENABLE_DEBUG_LOG)
                {
                    Debug.Log(string.Format("[NetworkPlayer] 狀態更新 - 玩家:{0} 舊位置:{1} 新位置:{2} Transform位置:{3}",
                        PlayerID,
                        oldPos,
                        state.Position.ToVector3(),
                        transform.position));
                }
            }
            else
            {
                Debug.LogWarning("[NetworkPlayer] 狀態類型錯誤 - 玩家:" + PlayerID + 
                    " 類型:" + newState.GetType());
            }
        }

        private void UpdateVisuals()
        {
            Vector3 oldPos = transform.position;
            Vector3 newPos = state.Position.ToVector3();
            transform.position = newPos;
            transform.rotation = state.Rotation.ToUnityQuaternion();

            if (ENABLE_DEBUG_LOG && oldPos != newPos)
            {
                Debug.Log(string.Format("[NetworkPlayer] 視覺更新 - 玩家:{0} 從:{1} 到:{2} 速度:{3} 幀:{4}",
                    PlayerID,
                    oldPos,
                    newPos,
                    state.Velocity.ToVector3(),
                    state.Frame));
            }
        }

        public void SendCommand(InputCommand cmd)
        {
            if (client != null)
            {
                try
                {
                    string message = FormatCommand(cmd);
                    if (!string.IsNullOrEmpty(message))
                    {
                        client.SendMessage(message);
                        if (ENABLE_DEBUG_LOG)
                        {
                            Debug.Log(string.Format("[NetworkPlayer] 發送命令 - 玩家:{0} 幀:{1} 方向:{2} 位置:{3}\n命令內容: {4}",
                                PlayerID,
                                cmd.Frame,
                                cmd.Direction.ToVector3(),
                                cmd.Position.ToVector3(),
                                message));
                        }
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError("[NetworkPlayer] 發送命令失敗: " + e.Message + "\n" + e.StackTrace);
                }
            }
        }

        private string FormatCommand(InputCommand cmd)
        {
            try
            {
                if (cmd.Type == CommandType.Move)
                {
                    Vector3 direction = cmd.Direction.ToVector3();
                    Vector3 position = cmd.Position.ToVector3();
                    
                    string message = string.Format("FRAME_INPUT:{0}:{1}:{2:F3}:{3:F3}:{4:F3}:{5:F3}:{6:F3}:{7:F3}",
                        cmd.PlayerID,
                        cmd.Frame,
                        direction.x,
                        direction.y,
                        direction.z,
                        position.x,
                        position.y,
                        position.z
                    );

                    return message;
                }
                return string.Empty;
            }
            catch (System.Exception e)
            {
                Debug.LogError("[NetworkPlayer] 格式化命令失敗: " + e.Message + "\n" + e.StackTrace);
                return string.Empty;
            }
        }

        private void OnDestroy()
        {
            if (ENetManager.Instance != null)
            {
                ENetManager.Instance.RemovePlayer(PlayerID);
            }
        }

        public FixedVector3 GetCurrentPosition()
        {
            return state != null ? state.Position : FixedVector3.FromVector3(transform.position);
        }
    }
} 
using ENet;
using UnityEngine;

namespace MENet.FrameSync.Handlers
{
    public class FrameInputHandler : IMsgHandler
    {
        private GameServer server;

        public FrameInputHandler(GameServer server)
        {
            this.server = server;
        }

        public void Handle(string content, Peer sender)
        {
            try
            {
                // 輸入格式: playerID:frame:dirX:dirY:dirZ:posX:posY:posZ
                string[] parts = content.Split(':');
                if (parts.Length != 8)
                {
                    Debug.LogError("[FrameInputHandler] 無效的輸入格式: " + content);
                    return;
                }

                // 使用 float.Parse 而不是 int.Parse 來處理浮點數
                int playerID = int.Parse(parts[0]);
                int frame = int.Parse(parts[1]);
                
                // 解析方向向量
                Vector3 direction = new Vector3(
                    float.Parse(parts[2]),
                    float.Parse(parts[3]),
                    float.Parse(parts[4])
                );

                // 解析位置向量
                Vector3 position = new Vector3(
                    float.Parse(parts[5]),
                    float.Parse(parts[6]),
                    float.Parse(parts[7])
                );

                // 創建輸入命令
                InputCommand cmd = new InputCommand
                {
                    PlayerID = playerID,
                    Frame = frame,
                    Type = CommandType.Move,
                    Direction = FixedVector3.FromVector3(direction),
                    Position = FixedVector3.FromVector3(position)
                };

                // 添加到服務器
                server.AddInputCommand(cmd);

                string logMessage = string.Format("[FrameInputHandler] 處理輸入命令 - 玩家:{0} 幀:{1} 方向:({2:F3}, {3:F3}, {4:F3}) 位置:({5:F3}, {6:F3}, {7:F3})",
                    playerID,
                    frame,
                    direction.x,
                    direction.y,
                    direction.z,
                    position.x,
                    position.y,
                    position.z);
                Debug.Log(logMessage);
            }
            catch (System.Exception e)
            {
                string errorMessage = "[FrameInputHandler] 處理命令失敗: " + e.Message + 
                    "\n堆疊: " + e.StackTrace + 
                    "\n內容: " + content;
                Debug.LogError(errorMessage);
            }
        }
    }
} 
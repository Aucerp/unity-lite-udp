using UnityEngine;
using System.Text;

namespace MENet
{
    public class DebugGUI : MonoBehaviour
    {
        private ENetManager netManager;
        private float updateInterval = 0.5f;
        private float accum = 0;
        private int frames = 0;
        private float timeleft;
        private float fps;
        private StringBuilder stringBuilder = new StringBuilder();

        private void Start()
        {
            netManager = ENetManager.Instance;
            timeleft = updateInterval;
        }

        private void Update()
        {
            // FPS 計算
            timeleft -= Time.deltaTime;
            accum += Time.timeScale / Time.deltaTime;
            frames++;

            if (timeleft <= 0.0)
            {
                fps = accum / frames;
                timeleft = updateInterval;
                accum = 0;
                frames = 0;
            }
        }

        private void OnGUI()
        {
            if (netManager == null) return;

            stringBuilder.Length = 0;
            stringBuilder.AppendLine("遊戲狀態信息:");
            stringBuilder.AppendFormat("FPS: {0:F2}\n", fps);
            stringBuilder.AppendFormat("當前幀: {0}\n", netManager.currentFrame);
            stringBuilder.AppendFormat("幀時間: {0:F2}ms\n", (1000.0f / fps));
            stringBuilder.AppendLine("\n已連接玩家:");

            var players = netManager.GetPlayerObjects();
            foreach (var kvp in players)
            {
                NetworkPlayer player = kvp.Value;
                if (player != null)
                {
                    Vector3 position = player.transform.position;
                    stringBuilder.AppendFormat("玩家 {0}: 位置 ({1:F2}, {2:F2}, {3:F2})\n",
                        kvp.Key, position.x, position.y, position.z);
                }
            }

            GUI.Label(new Rect(10, 10, 300, 500), stringBuilder.ToString());
        }
    }
} 
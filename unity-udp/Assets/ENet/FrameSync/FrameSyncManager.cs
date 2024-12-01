using UnityEngine;
using System.Collections.Generic;

namespace MENet.FrameSync
{
    public class FrameSyncManager
    {
        public const float FRAME_TIME = 1.0f / 30.0f;
        public static readonly FixedPoint FIXED_FRAME_TIME = FixedPoint.FromRaw((long)(FRAME_TIME * FixedPoint.PRECISION));
        private const int BUFFER_FRAMES = 3;

        private Queue<InputCommand> inputBuffer;
        private Dictionary<int, PlayerState> playerStates;
        protected int currentFrame;
        private float accumulator;
        private bool isGameStarted;
        private bool isPlayingRecord;
        private uint randomSeed;

        public int CurrentFrame { get { return currentFrame; } }

        public FrameSyncManager()
        {
            inputBuffer = new Queue<InputCommand>();
            playerStates = new Dictionary<int, PlayerState>();
            currentFrame = 0;
            accumulator = 0;
            isGameStarted = true;
            isPlayingRecord = false;
        }

        public void ResetFrame()
        {
            currentFrame = 0;
            accumulator = 0;
            inputBuffer.Clear();
            isPlayingRecord = true;
            Debug.Log("[FrameSync] 重置幀數為0，進入回放模式");
        }

        public void Update(float deltaTime)
        {
            if (!isGameStarted) return;
            if (isPlayingRecord) return;

            accumulator += deltaTime;
            while (accumulator >= FRAME_TIME)
            {
                // 處理當前幀的輸入
                while (inputBuffer.Count > 0)
                {
                    var command = inputBuffer.Dequeue();
                    if (command.Frame <= currentFrame)
                    {
                        ExecuteCommand(command);
                    }
                    else
                    {
                        inputBuffer.Enqueue(command);
                        break;
                    }
                }

                // 更新遊戲邏輯
                UpdateGameLogic();
                currentFrame++;
                accumulator -= FRAME_TIME;

                Debug.Log("[FrameSync] 更新幀 - 當前幀:" + currentFrame + 
                    " 輸入緩衝:" + inputBuffer.Count +
                    " 回放模式:" + isPlayingRecord);
            }
        }

        protected virtual void ProcessFrame()
        {
            while (inputBuffer.Count > 0)
            {
                InputCommand command = inputBuffer.Peek();
                if (command.Frame <= currentFrame)
                {
                    ExecuteCommand(inputBuffer.Dequeue());
                    if (Debug.isDebugBuild)
                    {
                        Debug.Log("[FrameSync] 執行命令 - 幀:" + currentFrame + " 玩家:" + command.PlayerID);
                    }
                }
                else
                {
                    break;
                }
            }

            UpdateGameLogic();
            currentFrame++;
        }

        private void ExecuteCommand(InputCommand command)
        {
            PlayerState state;
            if (playerStates.TryGetValue(command.PlayerID, out state))
            {
                var oldPos = state.Position;
                state.ApplyCommand(command);
                Debug.Log("[FrameSync] 應用命令到玩家狀態 - 玩家:" + command.PlayerID + 
                    " 從:" + oldPos.ToVector3() + 
                    " 到:" + state.Position.ToVector3() +
                    " 幀:" + command.Frame);
            }
            else
            {
                Debug.LogWarning("[FrameSync] 找不到玩家狀態 - 玩家:" + command.PlayerID);
            }
        }

        private void UpdateGameLogic()
        {
            foreach (KeyValuePair<int, PlayerState> kvp in playerStates)
            {
                kvp.Value.Update(FRAME_TIME);

                if (ENetManager.Instance != null)
                {
                    NetworkPlayer netPlayer = ENetManager.Instance.GetPlayerObject(kvp.Key);
                    if (netPlayer != null)
                    {
                        netPlayer.UpdateState(kvp.Value);
                        Debug.Log("[FrameSync] 同步狀態到網絡玩家 - 玩家:" + kvp.Key);
                    }
                }
            }
        }

        public GameSnapshot CreateSnapshot()
        {
            GameSnapshot snapshot = new GameSnapshot();
            snapshot.Frame = currentFrame;
            snapshot.PlayerStates = new Dictionary<int, PlayerState>(playerStates);
            snapshot.RandomSeed = randomSeed;
            return snapshot;
        }

        public void RestoreFromSnapshot(GameSnapshot snapshot)
        {
            currentFrame = snapshot.Frame;
            playerStates = new Dictionary<int, PlayerState>(snapshot.PlayerStates);
            randomSeed = snapshot.RandomSeed;
        }

        protected PlayerState GetPlayerState(int playerId)
        {
            PlayerState state;
            playerStates.TryGetValue(playerId, out state);
            return state;
        }

        public void AddPlayerState(int playerId)
        {
            if (!playerStates.ContainsKey(playerId))
            {
                playerStates.Add(playerId, new FPSPlayerState(playerId));
                Debug.Log("[FrameSync] 添加玩家狀態 - 玩家:" + playerId);
            }
        }

        public void AddInputCommand(InputCommand command)
        {
            if (!isPlayingRecord)
            {
                inputBuffer.Enqueue(command);
            }
            else
            {
                ExecuteCommand(command);
                UpdateGameLogic();
                
                if (command.Frame >= currentFrame)
                {
                    currentFrame = command.Frame + 1;
                }
                
                Debug.Log("[FrameSync] 回放命令 - 幀:" + command.Frame + 
                    " 玩家:" + command.PlayerID +
                    " 當前幀:" + currentFrame);
            }
        }
    }
} 
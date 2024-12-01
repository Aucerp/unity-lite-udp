using System.Collections.Generic;
using System.IO;
using UnityEngine;
using MENet.FrameSync;

namespace MENet
{
    public class MovementRecorder
    {
        private List<MoveCommand> moveHistory;
        private const int MAX_HISTORY = 300;
        private string recordFilePath;
        private string currentRecordFile;
        private StreamWriter currentWriter;

        public MovementRecorder()
        {
            moveHistory = new List<MoveCommand>();
        }

        public void StartNewRecording()
        {
            moveHistory.Clear();
            if (currentWriter != null)
            {
                currentWriter.Close();
                currentWriter = null;
            }
            CreateNewRecordFile();
        }

        private void CreateNewRecordFile()
        {
            try
            {
                string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string directory = Path.Combine(Application.streamingAssetsPath, "Record");
                
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                    Debug.Log("[MovementRecorder] 創建目錄: " + directory);
                }
                
                currentRecordFile = Path.Combine(directory, "record_" + timestamp + ".txt");
                recordFilePath = currentRecordFile;
                
                currentWriter = new StreamWriter(currentRecordFile, false);
                currentWriter.WriteLine("# Movement Record File - " + timestamp);
                currentWriter.Flush();

                Debug.Log("[MovementRecorder] 創建記錄文件: " + currentRecordFile + 
                    "\n檔案大小: " + new FileInfo(currentRecordFile).Length + " bytes" +
                    "\n完整路徑: " + Path.GetFullPath(currentRecordFile));
            }
            catch (System.Exception e)
            {
                Debug.LogError("[MovementRecorder] 創建記錄文件失敗: " + e.Message + "\n" + e.StackTrace + 
                    "\n嘗試創建的路徑: " + (currentRecordFile ?? "null"));
            }
        }

        public void RecordMove(MoveCommand cmd)
        {
            if (string.IsNullOrEmpty(currentRecordFile) || !File.Exists(currentRecordFile))
            {
                StartNewRecording();
            }

            moveHistory.Add(cmd);
            if (moveHistory.Count > MAX_HISTORY)
            {
                moveHistory.RemoveAt(0);
            }

            string record = string.Format("F:{0},P:{1},Pos:({2};{3};{4}),Dir:({5};{6};{7}),J:{8}",
                cmd.Frame,
                cmd.PlayerID,
                cmd.Position.x.Raw,
                cmd.Position.y.Raw,
                cmd.Position.z.Raw,
                cmd.Direction.x.Raw,
                cmd.Direction.y.Raw,
                cmd.Direction.z.Raw,
                cmd.IsJumping ? 1 : 0
            );

            try
            {
                if (currentWriter != null)
                {
                    currentWriter.WriteLine(record);
                    currentWriter.Flush();
                    
                    // 檢查檔案是否真的被寫入
                    if (File.Exists(currentRecordFile))
                    {
                        long fileSize = new FileInfo(currentRecordFile).Length;
                        Debug.Log("[MovementRecorder] 寫入記錄成功: " + record + 
                            "\n檔案大小: " + fileSize + " bytes" +
                            "\n檔案路徑: " + currentRecordFile);
                    }
                    else
                    {
                        Debug.LogError("[MovementRecorder] 檔案不存在，但應該已經創建: " + currentRecordFile + 
                            "\n完整路徑: " + Path.GetFullPath(currentRecordFile));
                        StartNewRecording();
                    }
                }
                else
                {
                    Debug.LogError("[MovementRecorder] Writer 未初始化");
                    StartNewRecording();
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("[MovementRecorder] 寫入記錄失敗: " + e.Message + "\n" + e.StackTrace + 
                    "\n當前文件: " + currentRecordFile);
                StartNewRecording();
            }
        }

        public void PrepareForPlayback()
        {
            try
            {
                // 確保所有寫入操作完成
                if (currentWriter != null)
                {
                    currentWriter.Flush();
                    currentWriter.Close();
                    currentWriter = null;
                    
                    // 等待文件系統完成寫入
                    System.Threading.Thread.Sleep(200);
                    
                    if (File.Exists(recordFilePath))
                    {
                        Debug.Log("[MovementRecorder] 完成所有寫入操作，準備回放" + 
                            "\n檔案: " + recordFilePath + 
                            "\n完整路徑: " + Path.GetFullPath(recordFilePath));
                    }
                    else
                    {
                        Debug.LogError("[MovementRecorder] 檔案不存在: " + recordFilePath + 
                            "\n完整路徑: " + Path.GetFullPath(recordFilePath));
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("[MovementRecorder] 準備回放時發生錯誤: " + e.Message + "\n" + e.StackTrace);
            }
        }

        public void PlaybackFrom(int startFrame, System.Action<MoveCommand> onPlayback)
        {
            try
            {
                // 確保所有寫入完成
                PrepareForPlayback();

                if (string.IsNullOrEmpty(recordFilePath))
                {
                    Debug.LogError("[MovementRecorder] 沒有可用的記錄文件");
                    return;
                }

                if (!File.Exists(recordFilePath))
                {
                    Debug.LogError("[MovementRecorder] 記錄文件不存在: " + recordFilePath + 
                        "\n完整路徑: " + Path.GetFullPath(recordFilePath));
                    return;
                }

                Debug.Log("[MovementRecorder] 準備讀取文件: " + recordFilePath + 
                    "\n完整路徑: " + Path.GetFullPath(recordFilePath));

                // 等待一小段時間確保檔案系統已完成寫入
                System.Threading.Thread.Sleep(200);

                string[] lines = File.ReadAllLines(recordFilePath);
                Debug.Log("[MovementRecorder] 讀取到 " + lines.Length + " 條記錄");

                if (lines.Length <= 1)
                {
                    Debug.LogWarning("[MovementRecorder] 記錄文件是空的或只有頭部資訊");
                    return;
                }

                int validRecords = 0;
                foreach (string line in lines)
                {
                    if (!string.IsNullOrEmpty(line) && !line.StartsWith("#"))
                    {
                        MoveCommand cmd = ParseRecord(line);
                        if (cmd.Frame >= startFrame && onPlayback != null)
                        {
                            onPlayback(cmd);
                            validRecords++;
                        }
                    }
                }

                Debug.Log("[MovementRecorder] 回放完成，處理了 " + validRecords + " 條有效記錄" +
                    "\n檔案路徑: " + recordFilePath);
            }
            catch (System.Exception e)
            {
                Debug.LogError("[MovementRecorder] 讀取記錄失敗: " + e.Message + "\n" + e.StackTrace + 
                    "\n檔案路徑: " + recordFilePath);
            }
        }

        private MoveCommand ParseRecord(string record)
        {
            try
            {
                Debug.Log("[MovementRecorder] 開始解析記錄: " + record);
                
                string[] parts = record.Split(',');
                if (parts.Length < 5)
                {
                    throw new System.Exception("記錄格式錯誤，部分數量不足: " + parts.Length);
                }

                // 解析幀號和玩家ID
                string[] frameParts = parts[0].Split(':');
                string[] playerParts = parts[1].Split(':');
                if (frameParts.Length < 2 || playerParts.Length < 2)
                {
                    throw new System.Exception("幀號或玩家ID格式錯誤");
                }

                int frame = int.Parse(frameParts[1]);
                int playerID = int.Parse(playerParts[1]);

                // 解析位置，使用分號分隔
                string posStr = parts[2].Split(':')[1].Trim('(', ')');
                string[] posValues = posStr.Split(';');
                if (posValues.Length < 3)
                {
                    throw new System.Exception("位置數據格式錯誤: " + posStr);
                }

                // 解析方向，使用分號分隔
                string dirStr = parts[3].Split(':')[1].Trim('(', ')');
                string[] dirValues = dirStr.Split(';');
                if (dirValues.Length < 3)
                {
                    throw new System.Exception("方向數據格式錯誤: " + dirStr);
                }

                // 創建向量
                Vector3 position = new Vector3(
                    FixedPoint.FromRaw(long.Parse(posValues[0])).ToFloat(),
                    FixedPoint.FromRaw(long.Parse(posValues[1])).ToFloat(),
                    FixedPoint.FromRaw(long.Parse(posValues[2])).ToFloat()
                );

                Vector3 direction = new Vector3(
                    FixedPoint.FromRaw(long.Parse(dirValues[0])).ToFloat(),
                    FixedPoint.FromRaw(long.Parse(dirValues[1])).ToFloat(),
                    FixedPoint.FromRaw(long.Parse(dirValues[2])).ToFloat()
                );

                // 解析跳躍狀態
                string[] jumpParts = parts[4].Split(':');
                if (jumpParts.Length < 2)
                {
                    throw new System.Exception("跳躍狀態格式錯誤");
                }
                bool isJumping = jumpParts[1] == "1";

                MoveCommand cmd = MoveCommand.Create(playerID, frame, position, direction, isJumping);
                Debug.Log("[MovementRecorder] 解析記錄成功: " + 
                    "\n幀號: " + frame +
                    "\n玩家ID: " + playerID +
                    "\n位置: " + position +
                    "\n方向: " + direction +
                    "\n跳躍: " + isJumping);
                return cmd;
            }
            catch (System.Exception e)
            {
                Debug.LogError("[MovementRecorder] 解析記錄失敗: " + record + 
                    "\n錯誤: " + e.Message + 
                    "\n堆疊: " + e.StackTrace);
                return new MoveCommand();
            }
        }

        ~MovementRecorder()
        {
            if (currentWriter != null)
            {
                currentWriter.Close();
                currentWriter = null;
            }
        }
    }
} 
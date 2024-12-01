using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace MENet.FrameSync
{
    public class MovementRecorder
    {
        private List<MoveCommand> moveHistory;
        private const int MAX_HISTORY = 300; // 存儲10秒的歷史 (30fps * 10)

        public MovementRecorder()
        {
            moveHistory = new List<MoveCommand>();
        }

        public void RecordMove(MoveCommand cmd)
        {
            moveHistory.Add(cmd);
            if (moveHistory.Count > MAX_HISTORY)
            {
                moveHistory.RemoveAt(0);
            }
        }

        public void PlaybackFrom(int startFrame, System.Action<MoveCommand> onPlayback)
        {
            IEnumerable<MoveCommand> moves = moveHistory
                .Where(m => m.Frame >= startFrame)
                .OrderBy(m => m.Frame);

            foreach (MoveCommand move in moves)
            {
                onPlayback(move);
            }
        }

        public void Clear()
        {
            moveHistory.Clear();
        }
    }
} 
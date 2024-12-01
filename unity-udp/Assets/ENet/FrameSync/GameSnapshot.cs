using System.Collections.Generic;

namespace MENet.FrameSync
{
    public class GameSnapshot
    {
        public int Frame { get; set; }
        public Dictionary<int, PlayerState> PlayerStates { get; set; }
        public uint RandomSeed { get; set; }

        public GameSnapshot()
        {
            PlayerStates = new Dictionary<int, PlayerState>();
        }
    }
} 
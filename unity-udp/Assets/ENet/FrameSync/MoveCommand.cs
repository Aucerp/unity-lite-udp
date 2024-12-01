using UnityEngine;

namespace MENet.FrameSync
{
    public struct MoveCommand
    {
        public int Frame { get; set; }
        public int PlayerID { get; set; }
        public FixedVector3 Position { get; set; }
        public FixedVector3 Direction { get; set; }
        public bool IsJumping { get; set; }

        public static MoveCommand Create(int playerId, int frame, Vector3 pos, Vector3 dir, bool jumping)
        {
            return new MoveCommand
            {
                PlayerID = playerId,
                Frame = frame,
                Position = FixedVector3.FromVector3(pos),
                Direction = FixedVector3.FromVector3(dir.normalized),
                IsJumping = jumping
            };
        }

        public InputCommand ToInputCommand()
        {
            return InputCommand.CreateMoveCommand(
                PlayerID,
                Frame,
                Direction,
                Position
            );
        }
    }
} 
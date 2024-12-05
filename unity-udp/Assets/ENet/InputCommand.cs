namespace MENet.FrameSync
{
    public struct InputCommand
    {
        public int PlayerID { get; set; }
        public int Frame { get; set; }
        public CommandType Type { get; set; }
        public FixedVector3 Direction { get; set; }
        public FixedVector3 Position { get; set; }
        public FixedQuaternion Rotation { get; set; }
        public int ActionID { get; set; }

        public static InputCommand CreateMoveCommand(int playerId, int frame, FixedVector3 direction)
        {
            return CreateMoveCommand(playerId, frame, direction, FixedVector3.FromInt(0, 0, 0));
        }

        public static InputCommand CreateMoveCommand(int playerId, int frame, FixedVector3 direction, FixedVector3 position)
        {
            return new InputCommand
            {
                PlayerID = playerId,
                Frame = frame,
                Type = CommandType.Move,
                Direction = direction,
                Position = position
            };
        }

        public static InputCommand CreateLookCommand(int playerId, int frame, FixedQuaternion rotation)
        {
            return new InputCommand
            {
                PlayerID = playerId,
                Frame = frame,
                Type = CommandType.Look,
                Rotation = rotation
            };
        }
    }

    public enum CommandType
    {
        None = 0,
        Move = 1,
        Look = 2,
        Jump = 3,
        Shoot = 4
    }
} 
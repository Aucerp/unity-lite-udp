using System;
using UnityEngine;
using MENet.FrameSync;
using System.Text;

namespace MENet.FrameSync.Commands
{
    public class MoveCommand
    {
        public int PlayerID { get; set; }
        public int Frame { get; set; }
        public FixedVector3 Position { get; set; }
        public FixedVector3 Direction { get; set; }
        public bool IsJumping { get; set; }

        private static StringBuilder _stringBuilder = new StringBuilder(128);

        public static MoveCommand Create(int playerID, int frame, Vector3 position, Vector3 direction, bool isJumping)
        {
            var cmd = new MoveCommand
            {
                PlayerID = playerID,
                Frame = frame,
                Position = FixedVector3.FromVector3(position),
                Direction = FixedVector3.FromVector3(direction),
                IsJumping = isJumping
            };
            return cmd;
        }

        public static MoveCommand FromString(string data)
        {
            string[] parts = data.Split(',');
            if (parts.Length < 8) return null;

            var cmd = new MoveCommand
            {
                PlayerID = int.Parse(parts[0]),
                Frame = int.Parse(parts[1]),
                Position = new FixedVector3(
                    FixedPoint.FromString(parts[2]),
                    FixedPoint.FromString(parts[3]),
                    FixedPoint.FromString(parts[4])
                ),
                Direction = new FixedVector3(
                    FixedPoint.FromString(parts[5]),
                    FixedPoint.FromString(parts[6]),
                    FixedPoint.FromString(parts[7])
                ),
                IsJumping = parts.Length > 8 && bool.Parse(parts[8])
            };
            return cmd;
        }

        public override string ToString()
        {
            _stringBuilder.Length = 0;
            _stringBuilder.Append(PlayerID)
                .Append(',')
                .Append(Frame)
                .Append(',')
                .Append(Position.x.ToFloat().ToString("F2"))
                .Append(',')
                .Append(Position.y.ToFloat().ToString("F2"))
                .Append(',')
                .Append(Position.z.ToFloat().ToString("F2"))
                .Append(',')
                .Append(Direction.x.ToFloat().ToString("F2"))
                .Append(',')
                .Append(Direction.y.ToFloat().ToString("F2"))
                .Append(',')
                .Append(Direction.z.ToFloat().ToString("F2"))
                .Append(',')
                .Append(IsJumping);
            
            return _stringBuilder.ToString();
        }
    }
} 
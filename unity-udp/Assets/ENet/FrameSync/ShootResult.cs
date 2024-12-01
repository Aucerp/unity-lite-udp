namespace MENet.FrameSync
{
    public struct ShootResult
    {
        public int ShooterID { get; set; }
        public int TargetID { get; set; }
        public int Damage { get; set; }
        public int Frame { get; set; }
    }

    public struct HitConfirmation
    {
        public int ShooterID { get; set; }
        public int TargetID { get; set; }
        public int Damage { get; set; }
        public int Frame { get; set; }
    }
} 
namespace HonVietThuThanh.Dev5
{
    public static class RoundResultTracker
    {
        public static int ActiveWaveIndex { get; private set; } = -1;
        public static int CompletedWaveIndex { get; private set; } = -1;
        public static int EnemiesDefeated { get; private set; }
        public static int KillGoldEarned { get; private set; }
        public static int InterestGoldEarned { get; private set; }
        public static int TotalGoldEarned => KillGoldEarned + InterestGoldEarned;

        public static void BeginRound(int waveIndex)
        {
            ActiveWaveIndex = waveIndex;
            CompletedWaveIndex = waveIndex;
            EnemiesDefeated = 0;
            KillGoldEarned = 0;
            InterestGoldEarned = 0;
        }

        public static void RecordEnemyKill(int goldReward)
        {
            if (ActiveWaveIndex < 0)
            {
                return;
            }

            EnemiesDefeated++;
            if (goldReward > 0)
            {
                KillGoldEarned += goldReward;
            }
        }

        public static void RecordInterestGold(int interestGold)
        {
            if (ActiveWaveIndex < 0 || interestGold <= 0)
            {
                return;
            }

            InterestGoldEarned += interestGold;
        }
    }
}

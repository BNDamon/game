namespace BlockMerge.Core
{
    /// <summary>Rewards clearing lines back-to-back, fast, over clearing them slowly and
    /// carefully — the thing that makes generic block-placement puzzles feel unhurried. Each
    /// clear within ComboWindowSeconds of the previous one raises the streak (and the score
    /// multiplier); waiting too long resets it. Time is fed in by the caller via Tick() so
    /// this stays engine-agnostic (no UnityEngine.Time dependency).</summary>
    public sealed class ComboTracker
    {
        public const float ComboWindowSeconds = 4f;
        private const float MultiplierStepPerCombo = 0.25f;
        private const float MaxMultiplier = 3f;

        private float _timeSinceLastClear = float.MaxValue;

        public int ComboCount { get; private set; }

        public float CurrentMultiplier =>
            ComboCount <= 1 ? 1f : System.Math.Min(1f + (ComboCount - 1) * MultiplierStepPerCombo, MaxMultiplier);

        public void Tick(float deltaTimeSeconds)
        {
            if (deltaTimeSeconds <= 0f) return;
            _timeSinceLastClear += deltaTimeSeconds;
            if (_timeSinceLastClear > ComboWindowSeconds && ComboCount != 0) ComboCount = 0;
        }

        /// <summary>Call when a line clear happens. Returns the resulting combo count (1 for
        /// a fresh/reset streak, 2+ for a chain) so the caller can decide whether to
        /// celebrate it.</summary>
        public int RegisterClear()
        {
            ComboCount = _timeSinceLastClear <= ComboWindowSeconds ? ComboCount + 1 : 1;
            _timeSinceLastClear = 0f;
            return ComboCount;
        }

        public void Reset()
        {
            ComboCount = 0;
            _timeSinceLastClear = float.MaxValue;
        }
    }
}

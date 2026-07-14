namespace BlockMerge.Core
{
    /// <summary>Rewards clearing lines back-to-back, fast, over clearing them slowly and
    /// carefully — the thing that makes generic block-placement puzzles feel unhurried. Each
    /// clear within the current combo window of the previous one raises the streak (and the
    /// score multiplier); waiting too long resets it. Time is fed in by the caller via Tick()
    /// so this stays engine-agnostic (no UnityEngine.Time dependency).
    ///
    /// The window itself isn't fixed: it starts at ComboWindowMax and shrinks toward
    /// ComboWindowMin over the course of a run (tracked by total elapsed play time, not
    /// score), so chaining gets progressively harder the longer a session goes on — a time-
    /// based difficulty ramp rather than a fixed, unhurried target.</summary>
    public sealed class ComboTracker
    {
        public const float ComboWindowMax = 4f;
        public const float ComboWindowMin = 2f;
        public const float ShrinkDurationSeconds = 240f;
        private const float MultiplierStepPerCombo = 0.25f;
        private const float MaxMultiplier = 3f;

        private float _timeSinceLastClear = float.MaxValue;
        private float _totalElapsedSeconds;

        public int ComboCount { get; private set; }

        public float CurrentMultiplier =>
            ComboCount <= 1 ? 1f : System.Math.Min(1f + (ComboCount - 1) * MultiplierStepPerCombo, MaxMultiplier);

        /// <summary>The combo window as of right now — ComboWindowMax at the start of a run,
        /// easing down to ComboWindowMin once ShrinkDurationSeconds of play have elapsed.</summary>
        public float CurrentWindowSeconds
        {
            get
            {
                float t = System.Math.Min(1f, _totalElapsedSeconds / ShrinkDurationSeconds);
                return ComboWindowMax - (ComboWindowMax - ComboWindowMin) * t;
            }
        }

        public void Tick(float deltaTimeSeconds)
        {
            if (deltaTimeSeconds <= 0f) return;
            _totalElapsedSeconds += deltaTimeSeconds;
            _timeSinceLastClear += deltaTimeSeconds;
            if (_timeSinceLastClear > CurrentWindowSeconds && ComboCount != 0) ComboCount = 0;
        }

        /// <summary>Call when a line clear happens. Returns the resulting combo count (1 for
        /// a fresh/reset streak, 2+ for a chain) so the caller can decide whether to
        /// celebrate it.</summary>
        public int RegisterClear()
        {
            ComboCount = _timeSinceLastClear <= CurrentWindowSeconds ? ComboCount + 1 : 1;
            _timeSinceLastClear = 0f;
            return ComboCount;
        }

        public void Reset()
        {
            ComboCount = 0;
            _timeSinceLastClear = float.MaxValue;
            _totalElapsedSeconds = 0f;
        }
    }
}

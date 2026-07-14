using UnityEngine;

namespace BlockMerge.Gameplay
{
    /// <summary>Persists the best score across app launches. Core's GameSession tracks
    /// Best in-memory only (engine-agnostic); this is the Unity-side load/save of it.</summary>
    public static class BestScoreStore
    {
        private const string Key = "BlockMerge.BestScore";

        public static int Load() => PlayerPrefs.GetInt(Key, 0);

        public static void Save(int best)
        {
            PlayerPrefs.SetInt(Key, best);
            PlayerPrefs.Save();
        }
    }
}

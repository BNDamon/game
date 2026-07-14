using UnityEngine;

namespace BlockMerge.Gameplay
{
    /// <summary>Minimal placeholder SFX: short synthesized tones generated at runtime, so
    /// there's audio feedback before any real sound design exists. Swap the generated clips
    /// for real audio assets later — call sites (Play(SfxKind)) won't need to change.</summary>
    public sealed class SfxPlayer : MonoBehaviour
    {
        public enum SfxKind
        {
            Place,
            Clear,
            Merge,
            PowerUp,
            GameOver
        }

        private AudioSource _source;
        private AudioClip _placeClip;
        private AudioClip _clearClip;
        private AudioClip _mergeClip;
        private AudioClip _powerUpClip;
        private AudioClip _gameOverClip;

        public static SfxPlayer Create(Transform parent)
        {
            var go = new GameObject("Sfx");
            go.transform.SetParent(parent, false);

            var player = go.AddComponent<SfxPlayer>();
            player._source = go.AddComponent<AudioSource>();
            player._source.playOnAwake = false;

            player._placeClip = GenerateTone(220f, 0.05f, 0.5f, rising: false);
            player._clearClip = GenerateTone(440f, 0.12f, 0.6f, rising: false);
            player._mergeClip = GenerateTone(660f, 0.2f, 0.6f, rising: true);
            player._powerUpClip = GenerateTone(330f, 0.25f, 0.7f, rising: true);
            player._gameOverClip = GenerateTone(180f, 0.4f, 0.6f, rising: false);

            return player;
        }

        public void Play(SfxKind kind)
        {
            var clip = kind switch
            {
                SfxKind.Place => _placeClip,
                SfxKind.Clear => _clearClip,
                SfxKind.Merge => _mergeClip,
                SfxKind.PowerUp => _powerUpClip,
                SfxKind.GameOver => _gameOverClip,
                _ => null
            };
            if (clip != null) _source.PlayOneShot(clip);
        }

        private static AudioClip GenerateTone(float frequency, float duration, float volume, bool rising)
        {
            const int sampleRate = 44100;
            int sampleCount = Mathf.Max(1, Mathf.RoundToInt(sampleRate * duration));
            var samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = i / (float)sampleRate;
                float freq = rising ? Mathf.Lerp(frequency, frequency * 1.6f, t / duration) : frequency;
                float envelope = 1f - i / (float)sampleCount; // linear fade-out avoids an audible click at the end
                samples[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * volume * envelope;
            }

            var clip = AudioClip.Create("Tone", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}

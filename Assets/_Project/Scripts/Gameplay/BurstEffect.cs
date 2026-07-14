using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace BlockMerge.Gameplay
{
    /// <summary>Cheap "particle" burst built from a handful of small UI squares instead of a
    /// real ParticleSystem — a world-space ParticleSystem would render behind our Screen
    /// Space - Overlay canvas, so faking it with UI elements keeps it compositing correctly
    /// with everything else. Played when a group of blocks merges into charge.</summary>
    public static class BurstEffect
    {
        private const int ParticleCount = 10;
        private const float Duration = 0.45f;

        public static void Play(MonoBehaviour runner, Transform parent, Vector2 anchoredPosition, Color color)
        {
            for (int i = 0; i < ParticleCount; i++)
            {
                var rect = UiFactory.CreateRect("Burst", parent);
                rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = anchoredPosition;
                rect.sizeDelta = new Vector2(16f, 16f);

                var image = rect.gameObject.AddComponent<Image>();
                image.sprite = UiFactory.RoundedSprite;
                image.type = Image.Type.Sliced;
                image.color = color;
                image.raycastTarget = false;

                float angle = 360f / ParticleCount * i + Random.Range(-15f, 15f);
                float distance = Random.Range(70f, 150f);
                var direction = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));

                runner.StartCoroutine(AnimateParticle(rect, image, anchoredPosition, direction * distance));
            }
        }

        private static IEnumerator AnimateParticle(RectTransform rect, Image image, Vector2 start, Vector2 offset)
        {
            float t = 0f;
            while (t < Duration)
            {
                t += Time.deltaTime;
                float p = Mathf.Clamp01(t / Duration);
                float eased = 1f - Mathf.Pow(1f - p, 2f);
                rect.anchoredPosition = start + offset * eased;
                float scale = Mathf.Lerp(1f, 0.15f, p);
                rect.localScale = new Vector3(scale, scale, 1f);
                var c = image.color;
                c.a = 1f - p;
                image.color = c;
                yield return null;
            }
            Object.Destroy(rect.gameObject);
        }
    }
}

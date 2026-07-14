using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace BlockMerge.Gameplay
{
    /// <summary>Small glowing embers drifting slowly upward across the whole screen,
    /// continuously, so the board never reads as a static screenshot even when the player
    /// isn't touching anything. Ties into the energy/charge theme (warm amber motes).</summary>
    public sealed class AmbientEmbers : MonoBehaviour
    {
        private static readonly Color EmberColor = new Color32(0xff, 0xb0, 0x5a, 0xff);

        private RectTransform _rect;

        public static AmbientEmbers Create(Transform parent)
        {
            var rect = UiFactory.CreateRect("AmbientEmbers", parent);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var embers = rect.gameObject.AddComponent<AmbientEmbers>();
            embers._rect = rect;
            embers.StartCoroutine(embers.SpawnLoop());
            return embers;
        }

        private IEnumerator SpawnLoop()
        {
            while (true)
            {
                yield return new WaitForSeconds(Random.Range(0.6f, 1.4f));
                SpawnEmber();
            }
        }

        private void SpawnEmber()
        {
            float width = _rect.rect.width;
            float height = _rect.rect.height;
            if (width <= 0f || height <= 0f) return; // not laid out yet

            var emberRect = UiFactory.CreateRect("Ember", _rect);
            float size = Random.Range(6f, 14f);
            emberRect.sizeDelta = new Vector2(size, size);
            emberRect.anchorMin = emberRect.anchorMax = new Vector2(0.5f, 0.5f);

            float startX = Random.Range(-width / 2f, width / 2f);
            float startY = -height / 2f - Random.Range(0f, 80f);
            emberRect.anchoredPosition = new Vector2(startX, startY);

            var image = emberRect.gameObject.AddComponent<Image>();
            image.sprite = UiFactory.RoundedSprite;
            image.type = Image.Type.Sliced;
            image.color = new Color(EmberColor.r, EmberColor.g, EmberColor.b, 0f);
            image.raycastTarget = false;

            StartCoroutine(DriftEmber(emberRect, image, height));
        }

        private IEnumerator DriftEmber(RectTransform emberRect, Image image, float screenHeight)
        {
            float duration = Random.Range(6f, 11f);
            float wobbleAmplitude = Random.Range(10f, 30f);
            float wobbleSpeed = Random.Range(0.5f, 1.2f);
            float riseDistance = screenHeight + 160f;
            var start = emberRect.anchoredPosition;
            float maxAlpha = Random.Range(0.12f, 0.3f);

            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                float p = Mathf.Clamp01(t / duration);
                float y = start.y + riseDistance * p;
                float x = start.x + Mathf.Sin(t * wobbleSpeed * Mathf.PI) * wobbleAmplitude;
                emberRect.anchoredPosition = new Vector2(x, y);

                float fade = p < 0.15f ? p / 0.15f : p > 0.85f ? (1f - p) / 0.15f : 1f;
                var c = image.color;
                c.a = maxAlpha * fade;
                image.color = c;

                yield return null;
            }
            if (emberRect != null) Destroy(emberRect.gameObject);
        }
    }
}

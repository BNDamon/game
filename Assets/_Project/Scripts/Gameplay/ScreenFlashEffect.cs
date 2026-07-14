using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace BlockMerge.Gameplay
{
    /// <summary>A quick full-screen color flash — intensity scales with how hot a combo is,
    /// so a big chain reads as a bigger moment even glanced at from the corner of an eye.</summary>
    public static class ScreenFlashEffect
    {
        public static void Play(MonoBehaviour runner, Transform canvasTransform, Color color, float peakAlpha, float duration = 0.18f)
        {
            var rect = UiFactory.CreateRect("ScreenFlash", canvasTransform);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.SetAsLastSibling();

            var image = rect.gameObject.AddComponent<Image>();
            image.sprite = UiFactory.SolidSprite;
            image.color = new Color(color.r, color.g, color.b, 0f);
            image.raycastTarget = false;

            runner.StartCoroutine(Animate(rect, image, peakAlpha, duration));
        }

        private static IEnumerator Animate(RectTransform rect, Image image, float peakAlpha, float duration)
        {
            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                float p = Mathf.Clamp01(t / duration);
                float alpha = p < 0.3f ? Mathf.Lerp(0f, peakAlpha, p / 0.3f) : Mathf.Lerp(peakAlpha, 0f, (p - 0.3f) / 0.7f);
                var c = image.color;
                c.a = alpha;
                image.color = c;
                yield return null;
            }
            Object.Destroy(rect.gameObject);
        }
    }
}

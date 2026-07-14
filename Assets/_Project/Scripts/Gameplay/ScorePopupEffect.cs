using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace BlockMerge.Gameplay
{
    /// <summary>A "+N" that rises and fades where a scoring line/merge just happened —
    /// the moment-to-moment feedback that a HUD number ticking up doesn't give you.</summary>
    public static class ScorePopupEffect
    {
        private static readonly Color TextColor = new Color32(0xff, 0xd3, 0x4d, 0xff);
        private const float Duration = 0.8f;
        private const float RiseDistance = 100f;

        public static void Play(MonoBehaviour runner, Transform parent, Vector2 anchoredPosition, int amount)
        {
            if (amount <= 0) return;

            var text = UiFactory.CreateText("ScorePopup", parent, $"+{amount}", 44, TextColor);
            var rect = text.rectTransform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.localScale = Vector3.one * 0.6f;
            text.raycastTarget = false;

            runner.StartCoroutine(AnimatePopup(rect, text, anchoredPosition));
        }

        private static IEnumerator AnimatePopup(RectTransform rect, Text text, Vector2 start)
        {
            float t = 0f;
            while (t < Duration)
            {
                t += Time.deltaTime;
                float p = Mathf.Clamp01(t / Duration);

                rect.anchoredPosition = start + new Vector2(0f, RiseDistance * p);

                float scaleP = Mathf.Clamp01(p / 0.25f);
                float scale = Mathf.Lerp(0.6f, 1f, 1f - Mathf.Pow(1f - scaleP, 2f));
                rect.localScale = new Vector3(scale, scale, 1f);

                var c = text.color;
                c.a = p < 0.55f ? 1f : 1f - (p - 0.55f) / 0.45f;
                text.color = c;

                yield return null;
            }
            Object.Destroy(rect.gameObject);
        }
    }
}

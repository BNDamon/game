using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace BlockMerge.Gameplay
{
    /// <summary>Big, bold "COMBO x3" callout for chained clears — bigger and punchier than
    /// the plain "+N" score popup, since this is the moment meant to hook the player on
    /// playing fast instead of slow and careful.</summary>
    public static class ComboPopupEffect
    {
        private static readonly Color ComboColor = new Color32(0xff, 0x8a, 0x3d, 0xff);
        private const float Duration = 0.6f;

        public static void Play(MonoBehaviour runner, Transform parent, Vector2 anchoredPosition, int comboCount)
        {
            if (comboCount < 2) return;

            var text = UiFactory.CreateText("ComboPopup", parent, $"COMBO x{comboCount}", 56, ComboColor);
            var rect = text.rectTransform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition + new Vector2(0f, 70f);
            rect.localScale = Vector3.one * 1.6f;
            text.raycastTarget = false;
            text.fontStyle = FontStyle.Bold;

            runner.StartCoroutine(AnimatePopup(rect, text));
        }

        private static IEnumerator AnimatePopup(RectTransform rect, Text text)
        {
            float t = 0f;
            while (t < Duration)
            {
                t += Time.deltaTime;
                float p = Mathf.Clamp01(t / Duration);

                float punchIn = Mathf.Clamp01(p / 0.2f);
                float settle = 1f - Mathf.Pow(1f - punchIn, 3f);
                float scale = Mathf.Lerp(1.6f, 1f, settle);
                rect.localScale = new Vector3(scale, scale, 1f);
                rect.anchoredPosition += new Vector2(0f, 40f * Time.deltaTime);

                var c = text.color;
                c.a = p < 0.6f ? 1f : 1f - (p - 0.6f) / 0.4f;
                text.color = c;

                yield return null;
            }
            Object.Destroy(rect.gameObject);
        }
    }
}

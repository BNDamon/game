using System;
using System.Collections;
using BlockMerge.Core;
using UnityEngine;
using UnityEngine.UI;

namespace BlockMerge.Gameplay
{
    public sealed class PowerUpBar : MonoBehaviour
    {
        // Charged accent styling (matches the meter's fill color) — these buttons only ever
        // appear once the meter is full, so they should read as "ready to use", not chrome.
        private static readonly Color ButtonColor = new Color32(0xff, 0xd3, 0x4d, 0xff);
        private static readonly Color TextColor = new Color32(0x14, 0x16, 0x1c, 0xff);

        public event Action<PowerUpType> PowerUpChosen;

        public static PowerUpBar Create(Transform parent)
        {
            var rect = UiFactory.CreateRect("PowerUpBar", parent);
            var layout = rect.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 10f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            var layoutElement = rect.gameObject.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = 90f;

            var bar = rect.gameObject.AddComponent<PowerUpBar>();

            bar.AddButton(rect, "Bomb", PowerUpType.Bomb);
            bar.AddButton(rect, "Line Clear", PowerUpType.Line);
            bar.AddButton(rect, "Wipe Color", PowerUpType.ColorWipe);

            rect.gameObject.SetActive(false);
            return bar;
        }

        private void AddButton(Transform parent, string label, PowerUpType type)
        {
            var button = UiFactory.CreateButton(label, parent, label, ButtonColor, TextColor);
            var layoutElement = button.gameObject.AddComponent<LayoutElement>();
            layoutElement.preferredWidth = 220f;
            layoutElement.preferredHeight = 80f;
            button.onClick.AddListener(() => PowerUpChosen?.Invoke(type));
        }

        public void SetAvailable(bool available)
        {
            gameObject.SetActive(available);
            if (available)
            {
                StopAllCoroutines();
                StartCoroutine(BreathingPulseLoop());
            }
        }

        private IEnumerator BreathingPulseLoop()
        {
            while (true)
            {
                float scale = 1f + (Mathf.Sin(Time.time * 2.2f) * 0.5f + 0.5f) * 0.05f;
                transform.localScale = new Vector3(scale, scale, 1f);
                yield return null;
            }
        }
    }
}

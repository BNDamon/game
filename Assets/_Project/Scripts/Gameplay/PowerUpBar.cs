using System;
using BlockMerge.Core;
using UnityEngine;
using UnityEngine.UI;

namespace BlockMerge.Gameplay
{
    public sealed class PowerUpBar : MonoBehaviour
    {
        private static readonly Color ButtonColor = new Color32(0x1e, 0x21, 0x29, 0xff);
        private static readonly Color TextColor = new Color32(0xee, 0xf0, 0xf4, 0xff);

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

        public void SetAvailable(bool available) => gameObject.SetActive(available);
    }
}

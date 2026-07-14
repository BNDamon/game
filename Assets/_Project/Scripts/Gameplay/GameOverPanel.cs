using System;
using UnityEngine;
using UnityEngine.UI;

namespace BlockMerge.Gameplay
{
    public sealed class GameOverPanel : MonoBehaviour
    {
        private static readonly Color BackgroundColor = new Color(0.04f, 0.043f, 0.055f, 0.92f);
        private static readonly Color SummaryColor = new Color32(0x8a, 0x90, 0xa0, 0xff);
        private static readonly Color RestartBg = new Color32(0xff, 0xd3, 0x4d, 0xff);
        private static readonly Color RestartFg = new Color32(0x14, 0x16, 0x1c, 0xff);

        public event Action RestartClicked;

        private Text _summaryText;

        public static GameOverPanel Create(Transform parent)
        {
            var rect = UiFactory.CreateRect("GameOverPanel", parent);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var background = rect.gameObject.AddComponent<Image>();
            background.sprite = UiFactory.SolidSprite;
            background.color = BackgroundColor;

            var panel = rect.gameObject.AddComponent<GameOverPanel>();

            var layout = rect.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.spacing = 16f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            UiFactory.CreateText("Title", rect, "Game Over", 48, Color.white);
            panel._summaryText = UiFactory.CreateText("Summary", rect, "", 28, SummaryColor);

            var restartButton = UiFactory.CreateButton("RestartButton", rect, "Play Again", RestartBg, RestartFg);
            var restartLayout = restartButton.gameObject.AddComponent<LayoutElement>();
            restartLayout.preferredWidth = 260f;
            restartLayout.preferredHeight = 90f;
            restartButton.onClick.AddListener(() => panel.RestartClicked?.Invoke());

            rect.gameObject.SetActive(false);
            return panel;
        }

        public void Show(int score, int best)
        {
            _summaryText.text = $"Score: {score}  •  Best: {best}";
            gameObject.SetActive(true);
        }

        public void Hide() => gameObject.SetActive(false);
    }
}

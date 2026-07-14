using UnityEngine;
using UnityEngine.UI;

namespace BlockMerge.Gameplay
{
    public sealed class HudView : MonoBehaviour
    {
        private static readonly Color PanelColor = new Color32(0x1e, 0x21, 0x29, 0xff);
        private static readonly Color TrackColor = new Color32(0x2a, 0x2e, 0x38, 0xff);
        private static readonly Color TextColor = new Color32(0xee, 0xf0, 0xf4, 0xff);
        private static readonly Color MutedColor = new Color32(0x8a, 0x90, 0xa0, 0xff);
        private static readonly Color MeterColor = new Color32(0xff, 0xd3, 0x4d, 0xff);

        private Text _scoreText;
        private Text _bestText;
        private RectTransform _meterFill;

        public static HudView Create(Transform parent)
        {
            var root = UiFactory.CreateRect("Hud", parent);
            root.gameObject.AddComponent<VerticalLayoutGroup>().spacing = 10f;
            var rootLayoutElement = root.gameObject.AddComponent<LayoutElement>();
            rootLayoutElement.preferredHeight = 170f;

            var hud = root.gameObject.AddComponent<HudView>();

            var scoreRow = UiFactory.CreateRect("ScoreRow", root);
            var scoreLayout = scoreRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            scoreLayout.spacing = 12f;
            scoreLayout.childForceExpandWidth = true;
            var scoreRowElement = scoreRow.gameObject.AddComponent<LayoutElement>();
            scoreRowElement.preferredHeight = 90f;

            CreateStat(scoreRow, "Score", out hud._scoreText);
            CreateStat(scoreRow, "Best", out hud._bestText);

            var meterBg = UiFactory.CreatePanel("MeterBg", root, PanelColor);
            var meterBgElement = meterBg.gameObject.AddComponent<LayoutElement>();
            meterBgElement.preferredHeight = 60f;

            var meterTrack = UiFactory.CreatePanel("MeterTrack", meterBg.transform, TrackColor);
            var trackRect = meterTrack.rectTransform;
            trackRect.anchorMin = new Vector2(0f, 0.3f);
            trackRect.anchorMax = new Vector2(1f, 0.7f);
            trackRect.offsetMin = new Vector2(12f, 0f);
            trackRect.offsetMax = new Vector2(-12f, 0f);

            var meterFillImage = UiFactory.CreatePanel("MeterFill", meterTrack.transform, MeterColor);
            var fillRect = meterFillImage.rectTransform;
            fillRect.anchorMin = new Vector2(0f, 0f);
            fillRect.anchorMax = new Vector2(0f, 1f);
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
            hud._meterFill = fillRect;

            return hud;
        }

        private static void CreateStat(Transform parent, string label, out Text valueText)
        {
            var box = UiFactory.CreatePanel(label + "Box", parent, PanelColor);
            var layout = box.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.spacing = 4f;

            UiFactory.CreateText(label + "Label", box.transform, label.ToUpperInvariant(), 20, MutedColor);
            valueText = UiFactory.CreateText(label + "Value", box.transform, "0", 36, TextColor);
        }

        public void SetScore(int score) => _scoreText.text = score.ToString();
        public void SetBest(int best) => _bestText.text = best.ToString();

        public void SetMeter(int value, int max)
        {
            float fraction = max <= 0 ? 0f : Mathf.Clamp01((float)value / max);
            _meterFill.anchorMax = new Vector2(fraction, 1f);
        }
    }
}

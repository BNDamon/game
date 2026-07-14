using System.Collections;
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
        private RectTransform _meterBgRect;
        private Image _meterGlow;
        private Coroutine _meterCoroutine;
        private Coroutine _scoreCoroutine;
        private float _currentFraction;
        private bool _wasFull;

        public static HudView Create(Transform parent)
        {
            var root = UiFactory.CreateRect("Hud", parent);
            var rootLayout = root.gameObject.AddComponent<VerticalLayoutGroup>();
            rootLayout.spacing = 8f;
            rootLayout.childControlWidth = true;
            rootLayout.childControlHeight = true;
            rootLayout.childForceExpandWidth = true;
            rootLayout.childForceExpandHeight = false;
            var rootLayoutElement = root.gameObject.AddComponent<LayoutElement>();
            rootLayoutElement.preferredHeight = 112f;

            var hud = root.gameObject.AddComponent<HudView>();

            var scoreRow = UiFactory.CreateRect("ScoreRow", root);
            var scoreLayout = scoreRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            scoreLayout.spacing = 10f;
            scoreLayout.childAlignment = TextAnchor.MiddleLeft;
            scoreLayout.childControlWidth = true;
            scoreLayout.childControlHeight = true;
            scoreLayout.childForceExpandWidth = false;
            scoreLayout.childForceExpandHeight = true;
            var scoreRowElement = scoreRow.gameObject.AddComponent<LayoutElement>();
            scoreRowElement.preferredHeight = 60f;

            // Score is the hero stat: no box, just bold glowing text, so it doesn't read as
            // "one of two identical panels" the way the old twin score/best boxes did.
            var scoreHero = UiFactory.CreateRect("ScoreHero", scoreRow);
            var scoreHeroLayout = scoreHero.gameObject.AddComponent<VerticalLayoutGroup>();
            scoreHeroLayout.childAlignment = TextAnchor.MiddleLeft;
            scoreHeroLayout.spacing = -2f;
            scoreHeroLayout.childControlWidth = true;
            scoreHeroLayout.childControlHeight = true;
            scoreHeroLayout.childForceExpandWidth = false;
            scoreHeroLayout.childForceExpandHeight = false;
            var scoreHeroElement = scoreHero.gameObject.AddComponent<LayoutElement>();
            scoreHeroElement.flexibleWidth = 1f;
            scoreHeroElement.preferredHeight = 60f;

            UiFactory.CreateText("ScoreLabel", scoreHero, "SCORE", 14, MutedColor, TextAnchor.MiddleLeft);
            hud._scoreText = UiFactory.CreateText("ScoreValue", scoreHero, "0", 42, TextColor, TextAnchor.MiddleLeft);
            hud._scoreText.fontStyle = FontStyle.Bold;

            // Best is secondary: plain floating text pinned to the right, no panel — matches
            // ScoreHero's boxless treatment so nothing in the HUD reads as "a box" anymore.
            var bestBadge = UiFactory.CreateRect("BestBadge", scoreRow);
            var bestBadgeLayout = bestBadge.gameObject.AddComponent<VerticalLayoutGroup>();
            bestBadgeLayout.childAlignment = TextAnchor.MiddleRight;
            bestBadgeLayout.spacing = -2f;
            bestBadgeLayout.childControlWidth = true;
            bestBadgeLayout.childControlHeight = true;
            bestBadgeLayout.childForceExpandWidth = false;
            bestBadgeLayout.childForceExpandHeight = false;
            var bestBadgeElement = bestBadge.gameObject.AddComponent<LayoutElement>();
            bestBadgeElement.preferredWidth = 150f;
            bestBadgeElement.preferredHeight = 60f;

            UiFactory.CreateText("BestLabel", bestBadge, "BEST", 14, MutedColor, TextAnchor.MiddleRight);
            hud._bestText = UiFactory.CreateText("BestValue", bestBadge, "0", 28, TextColor, TextAnchor.MiddleRight);
            hud._bestText.fontStyle = FontStyle.Bold;

            var meterBg = UiFactory.CreateRoundedPanel("MeterBg", root, PanelColor);
            var meterBgElement = meterBg.gameObject.AddComponent<LayoutElement>();
            meterBgElement.preferredHeight = 44f;
            hud._meterBgRect = meterBg.rectTransform;

            // A soft glow sits behind the track, larger than the box itself, so it can bloom
            // outward as charge builds without distorting the track's own shape.
            var glowRect = UiFactory.CreateRect("MeterGlow", meterBg.transform);
            glowRect.anchorMin = new Vector2(-0.06f, -0.3f);
            glowRect.anchorMax = new Vector2(1.06f, 1.3f);
            glowRect.offsetMin = Vector2.zero;
            glowRect.offsetMax = Vector2.zero;
            glowRect.SetAsFirstSibling();

            var glowImage = glowRect.gameObject.AddComponent<Image>();
            glowImage.sprite = UiFactory.RoundedSprite;
            glowImage.type = Image.Type.Sliced;
            glowImage.color = new Color(MeterColor.r, MeterColor.g, MeterColor.b, 0f);
            glowImage.raycastTarget = false;
            hud._meterGlow = glowImage;

            var meterTrack = UiFactory.CreateRoundedPanel("MeterTrack", meterBg.transform, TrackColor);
            var trackRect = meterTrack.rectTransform;
            trackRect.anchorMin = new Vector2(0f, 0.3f);
            trackRect.anchorMax = new Vector2(1f, 0.7f);
            trackRect.offsetMin = new Vector2(12f, 0f);
            trackRect.offsetMax = new Vector2(-12f, 0f);

            var meterFillImage = UiFactory.CreateRoundedPanel("MeterFill", meterTrack.transform, MeterColor);
            var fillRect = meterFillImage.rectTransform;
            fillRect.anchorMin = new Vector2(0f, 0f);
            fillRect.anchorMax = new Vector2(0f, 1f);
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
            hud._meterFill = fillRect;

            hud.StartCoroutine(hud.GlowPulseLoop());

            return hud;
        }

        /// <summary>Instant, no-animation set — use for initial load / restart.</summary>
        public void SetScore(int score) => _scoreText.text = score.ToString();
        public void SetBest(int best) => _bestText.text = best.ToString();

        /// <summary>Instant, no-animation set — use for initial load / restart.</summary>
        public void SetMeter(int value, int max)
        {
            float fraction = max <= 0 ? 0f : Mathf.Clamp01((float)value / max);
            _meterFill.anchorMax = new Vector2(fraction, 1f);
            _currentFraction = fraction;
            _wasFull = fraction >= 1f;
        }

        public void SetScoreAnimated(int score)
        {
            if (_scoreCoroutine != null) StopCoroutine(_scoreCoroutine);
            _scoreCoroutine = StartCoroutine(AnimateScore(score));
        }

        public void SetMeterAnimated(int value, int max)
        {
            float target = max <= 0 ? 0f : Mathf.Clamp01((float)value / max);
            bool justFilled = target >= 1f && !_wasFull;
            _wasFull = target >= 1f;

            if (_meterCoroutine != null) StopCoroutine(_meterCoroutine);
            _meterCoroutine = StartCoroutine(AnimateMeter(target));

            if (justFilled) StartCoroutine(FullChargeFlash());
        }

        private IEnumerator AnimateScore(int target)
        {
            int.TryParse(_scoreText.text, out var start);
            const float duration = 0.22f;
            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                _scoreText.text = Mathf.RoundToInt(Mathf.Lerp(start, target, t / duration)).ToString();
                yield return null;
            }
            _scoreText.text = target.ToString();
        }

        private IEnumerator AnimateMeter(float target)
        {
            float start = _meterFill.anchorMax.x;
            const float duration = 0.22f;
            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                float p = t / duration;
                float x = Mathf.Lerp(start, target, p);
                _meterFill.anchorMax = new Vector2(x, 1f);
                _currentFraction = x;
                yield return null;
            }
            _meterFill.anchorMax = new Vector2(target, 1f);
            _currentFraction = target;
        }

        /// <summary>Runs for the lifetime of the HUD, softly breathing the glow behind the
        /// meter — barely visible when charge is low, clearly pulsing once it's nearly full.</summary>
        private IEnumerator GlowPulseLoop()
        {
            while (true)
            {
                float pulse = (Mathf.Sin(Time.time * 3f) + 1f) * 0.5f;
                float alpha = _currentFraction * Mathf.Lerp(0.12f, 0.45f, pulse);
                var c = _meterGlow.color;
                c.a = alpha;
                _meterGlow.color = c;
                yield return null;
            }
        }

        private IEnumerator FullChargeFlash()
        {
            const float duration = 0.4f;
            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                float p = Mathf.Clamp01(t / duration);
                float pulse = Mathf.Sin(p * Mathf.PI);
                float scale = 1f + pulse * 0.06f;
                _meterBgRect.localScale = new Vector3(scale, scale, 1f);
                yield return null;
            }
            _meterBgRect.localScale = Vector3.one;
        }
    }
}

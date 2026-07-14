using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BlockMerge.Gameplay
{
    /// <summary>Small helpers for building flat-color UGUI elements entirely from code, so
    /// the "no sprites yet, flat colored squares" constraint doesn't require any art assets
    /// or a hand-authored scene — GameController builds the whole screen at runtime.</summary>
    public static class UiFactory
    {
        private const int RoundedTextureSize = 64;
        private const int RoundedCornerRadius = 16;

        private static Sprite _solidSprite;
        private static Sprite _roundedSprite;
        private static Sprite _roundedHighlightSprite;

        public static Sprite SolidSprite
        {
            get
            {
                if (_solidSprite == null)
                {
                    var texture = new Texture2D(4, 4);
                    var pixels = new Color32[16];
                    for (int i = 0; i < pixels.Length; i++) pixels[i] = new Color32(255, 255, 255, 255);
                    texture.SetPixels32(pixels);
                    texture.Apply();
                    texture.filterMode = FilterMode.Bilinear;
                    _solidSprite = Sprite.Create(texture, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f));
                }
                return _solidSprite;
            }
        }

        /// <summary>A 9-sliced rounded-rect sprite shared by every panel/cell/button in the
        /// game, so the whole UI reads as one consistent shape language instead of flat
        /// squares. Corner radius is fixed in source pixels, so it scales the same way
        /// (roughly constant on-screen roundedness) whether it's stretched to a HUD box or a
        /// tiny tray mini-cell.</summary>
        public static Sprite RoundedSprite
        {
            get
            {
                if (_roundedSprite == null)
                    _roundedSprite = CreateRoundedRectSprite(opaque: true);
                return _roundedSprite;
            }
        }

        /// <summary>Same rounded-rect shape, but the fill fades from a soft white highlight
        /// at the top to fully transparent below the midline — a cheap "glossy/lit from
        /// above" overlay laid on top of a flat-colored cell.</summary>
        public static Sprite RoundedHighlightSprite
        {
            get
            {
                if (_roundedHighlightSprite == null)
                    _roundedHighlightSprite = CreateRoundedRectSprite(opaque: false);
                return _roundedHighlightSprite;
            }
        }

        private static Sprite CreateRoundedRectSprite(bool opaque)
        {
            const int size = RoundedTextureSize;
            const int r = RoundedCornerRadius;

            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var pixels = new Color32[size * size];

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    bool inside = IsInsideRoundedRect(x, y, size, r);
                    byte alpha;
                    if (!inside)
                    {
                        alpha = 0;
                    }
                    else if (opaque)
                    {
                        alpha = 255;
                    }
                    else
                    {
                        float fromTop = y / (float)(size - 1); // 0 at bottom, 1 at top
                        float weight = Mathf.Clamp01((fromTop - 0.45f) / 0.55f);
                        alpha = (byte)Mathf.RoundToInt(weight * 140f);
                    }
                    pixels[y * size + x] = new Color32(255, 255, 255, alpha);
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply();
            texture.filterMode = FilterMode.Bilinear;
            texture.wrapMode = TextureWrapMode.Clamp;

            var border = new Vector4(r, r, r, r);
            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, border);
        }

        private static bool IsInsideRoundedRect(int x, int y, int size, int r)
        {
            bool nearLeft = x < r;
            bool nearRight = x >= size - r;
            bool nearTop = y < r;
            bool nearBottom = y >= size - r;

            if (!((nearLeft || nearRight) && (nearTop || nearBottom)))
                return true; // in the cross/core area, not near any corner

            int cx = nearLeft ? r : size - r - 1;
            int cy = nearTop ? r : size - r - 1;
            float dx = x - cx;
            float dy = y - cy;
            return dx * dx + dy * dy <= r * r;
        }

        public static void EnsureEventSystem()
        {
            if (Object.FindObjectOfType<EventSystem>() != null) return;
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }

        public static Canvas CreateCanvas(string name)
        {
            var go = new GameObject(name, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            return canvas;
        }

        public static RectTransform CreateRect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            return rect;
        }

        /// <summary>A flat, unrounded rectangle — use only for edge-to-edge backgrounds
        /// (screen background, full-screen overlays) where rounded corners would look wrong.
        /// Everything else should use CreateRoundedPanel.</summary>
        public static Image CreatePanel(string name, Transform parent, Color color)
        {
            var rect = CreateRect(name, parent);
            var image = rect.gameObject.AddComponent<Image>();
            image.sprite = SolidSprite;
            image.color = color;
            image.type = Image.Type.Simple;
            return image;
        }

        public static Image CreateRoundedPanel(string name, Transform parent, Color color)
        {
            var rect = CreateRect(name, parent);
            var image = rect.gameObject.AddComponent<Image>();
            image.sprite = RoundedSprite;
            image.color = color;
            image.type = Image.Type.Sliced;
            return image;
        }

        /// <summary>Adds a static, non-interactive glossy highlight overlay stretched over an
        /// existing rounded element (e.g. a board cell or mini-cell), giving it a lit-from-
        /// above look instead of a flat tint.</summary>
        public static Image AddRoundedHighlight(Transform parent)
        {
            var rect = CreateRect("Highlight", parent);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var image = rect.gameObject.AddComponent<Image>();
            image.sprite = RoundedHighlightSprite;
            image.type = Image.Type.Sliced;
            image.color = Color.white;
            image.raycastTarget = false;
            return image;
        }

        public static Text CreateText(string name, Transform parent, string content, int fontSize, Color color,
            TextAnchor anchor = TextAnchor.MiddleCenter)
        {
            var rect = CreateRect(name, parent);
            var text = rect.gameObject.AddComponent<Text>();
            text.text = content;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.color = color;
            text.alignment = anchor;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
        }

        public static Button CreateButton(string name, Transform parent, string label, Color background, Color foreground)
        {
            var image = CreateRoundedPanel(name, parent, background);
            var button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;

            var text = CreateText(name + "Label", image.transform, label, 24, foreground);
            var textRect = text.rectTransform;
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            return button;
        }
    }
}

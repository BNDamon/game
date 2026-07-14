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
        /// tiny tray mini-cell. A subtle brightness gradient (not a separate glossy overlay —
        /// that read as a dated skeuomorphic bevel) is baked directly into the RGB channels,
        /// so tinting it with Image.color still multiplies in a faint top-lit look for free.</summary>
        public static Sprite RoundedSprite
        {
            get
            {
                if (_roundedSprite == null)
                    _roundedSprite = CreateRoundedRectSprite();
                return _roundedSprite;
            }
        }

        private static Sprite CreateRoundedRectSprite()
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
                    if (!inside)
                    {
                        pixels[y * size + x] = new Color32(255, 255, 255, 0);
                        continue;
                    }

                    float fromTop = y / (float)(size - 1); // 0 at bottom, 1 at top
                    byte shade = (byte)Mathf.RoundToInt(Mathf.Lerp(208f, 255f, fromTop));
                    pixels[y * size + x] = new Color32(shade, shade, shade, 255);
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

        /// <summary>Multiplies a color's RGB toward black, leaving alpha untouched — used to
        /// derive a block's darker "side" shade from its own top-face color, so the extrusion
        /// reads as the same cube rather than a generic drop shadow.</summary>
        public static Color Darken(Color color, float factor) =>
            new Color(color.r * factor, color.g * factor, color.b * factor, color.a);

        /// <summary>An empty container holding two stretched rounded-rect children — a
        /// "Shadow" behind and a "Fill" in front — so a filled cell reads as an actual 3D
        /// cube: a bright top face plus a darker same-hue "side" peeking out directly beneath
        /// it, extruded by a fixed pixel depth (no horizontal offset, so it reads as a side
        /// face rather than a light-source drop shadow). The side starts fully transparent;
        /// callers toggle its alpha (and refresh its color when the fill color changes) to
        /// switch between "raised" (filled) and "flat" (empty) looks. A child can't render
        /// behind its own parent's Graphic in Unity's UI draw order, which is why this needs
        /// a plain container rather than the fill living directly on the returned rect.</summary>
        public static RectTransform CreateElevatedCell(string name, Transform parent, Color fillColor, out Image shadow, out Image fill)
        {
            return CreateElevatedCell(name, parent, fillColor, 8f, out shadow, out fill);
        }

        public static RectTransform CreateElevatedCell(string name, Transform parent, Color fillColor, float extrusionDepth, out Image shadow, out Image fill)
        {
            var container = CreateRect(name, parent);

            var shadowColor = Darken(fillColor, 0.5f);
            shadowColor.a = 0f;
            shadow = CreateRoundedPanel("Shadow", container, shadowColor);
            var shadowRect = shadow.rectTransform;
            shadowRect.anchorMin = Vector2.zero;
            shadowRect.anchorMax = Vector2.one;
            shadowRect.offsetMin = new Vector2(0f, -extrusionDepth);
            shadowRect.offsetMax = new Vector2(0f, -extrusionDepth);
            shadow.raycastTarget = false;

            fill = CreateRoundedPanel("Fill", container, fillColor);
            var fillRect = fill.rectTransform;
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;

            return container;
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

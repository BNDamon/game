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
        private static Sprite _solidSprite;

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

        public static Image CreatePanel(string name, Transform parent, Color color)
        {
            var rect = CreateRect(name, parent);
            var image = rect.gameObject.AddComponent<Image>();
            image.sprite = SolidSprite;
            image.color = color;
            image.type = Image.Type.Simple;
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
            var image = CreatePanel(name, parent, background);
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

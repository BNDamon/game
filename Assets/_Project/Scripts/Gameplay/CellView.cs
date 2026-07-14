using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BlockMerge.Gameplay
{
    public sealed class CellView : MonoBehaviour, IPointerClickHandler
    {
        private static readonly Color EmptyColor = new Color32(0x22, 0x25, 0x2d, 0xff);
        private static readonly Color PreviewValidColor = new Color32(0xf4, 0xf6, 0xf8, 0xff);
        private static readonly Color PreviewInvalidColor = new Color32(0xff, 0x55, 0x55, 0xff);

        /// <summary>Tap-to-target for armed power-ups. Piece placement is drag-based (see
        /// PieceTraySlotView) and doesn't use this.</summary>
        public event Action<CellView> Clicked;

        public int Row { get; private set; }
        public int Col { get; private set; }
        public RectTransform RectTransform { get; private set; }
        public Color CurrentColor => _currentColor;

        private Image _image;
        private Color _currentColor = EmptyColor;
        private bool _previewing;
        private bool _filled;

        public static CellView Create(Transform parent, int row, int col)
        {
            var image = UiFactory.CreateRoundedPanel($"Cell_{row}_{col}", parent, EmptyColor);

            var cell = image.gameObject.AddComponent<CellView>();
            cell._image = image;
            cell.RectTransform = image.rectTransform;
            cell.Row = row;
            cell.Col = col;
            return cell;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Clicked?.Invoke(this);
        }

        /// <summary>Sets the cell's actual logical color (null = empty). If a preview overlay
        /// is currently showing, the visible color doesn't change until the preview clears —
        /// this just updates what it reverts to.</summary>
        public void SetColor(Color? color)
        {
            _currentColor = color ?? EmptyColor;
            if (!_previewing) _image.color = _currentColor;
            _filled = color.HasValue;
            RestartBreathing();
        }

        /// <summary>A slow, subtle brightness pulse on filled cells so the board reads as
        /// alive even when nothing's being placed — restarted after every other animation
        /// interrupts it (they all StopAllCoroutines to take over the cell's visuals).</summary>
        private void RestartBreathing()
        {
            StopCoroutine(nameof(BreatheRoutine));
            if (_filled) StartCoroutine(nameof(BreatheRoutine));
        }

        private IEnumerator BreatheRoutine()
        {
            float phaseOffset = GetInstanceID() * 0.37f;
            while (true)
            {
                if (!_previewing)
                {
                    float pulse = (Mathf.Sin(Time.time * 1.6f + phaseOffset) + 1f) * 0.5f;
                    _image.color = Color.Lerp(_currentColor, Color.white, pulse * 0.12f);
                }
                yield return null;
            }
        }

        public void ShowPreview(bool valid)
        {
            _previewing = true;
            _image.color = valid ? PreviewValidColor : PreviewInvalidColor;
        }

        public void ClearPreview()
        {
            if (!_previewing) return;
            _previewing = false;
            _image.color = _currentColor;
        }

        public void PlayPlacementPop()
        {
            StopAllCoroutines();
            StartCoroutine(PlacementPopRoutine());
        }

        public void PlayClearAndEmpty()
        {
            StopAllCoroutines();
            StartCoroutine(ClearRoutine());
        }

        public void PlayMergeAndEmpty()
        {
            StopAllCoroutines();
            StartCoroutine(MergeRoutine());
        }

        private IEnumerator PlacementPopRoutine()
        {
            const float duration = 0.12f;
            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                float scale = Mathf.Lerp(1.25f, 1f, t / duration);
                transform.localScale = new Vector3(scale, scale, 1f);
                yield return null;
            }
            transform.localScale = Vector3.one;
            RestartBreathing();
        }

        private IEnumerator ClearRoutine()
        {
            const float duration = 0.16f;
            var startColor = _currentColor;
            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                float p = Mathf.Clamp01(t / duration);
                float scale = p < 0.5f ? Mathf.Lerp(1f, 1.15f, p / 0.5f) : Mathf.Lerp(1.15f, 0.3f, (p - 0.5f) / 0.5f);
                transform.localScale = new Vector3(scale, scale, 1f);
                float brighten = p < 0.5f ? p / 0.5f : (1f - p) / 0.5f;
                _image.color = Color.Lerp(startColor, Color.white, brighten * 0.8f);
                yield return null;
            }
            transform.localScale = Vector3.one;
            SetColor(null);
        }

        private IEnumerator MergeRoutine()
        {
            const float duration = 0.28f;
            var startColor = _currentColor;
            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                float p = Mathf.Clamp01(t / duration);
                float scale = p < 0.4f ? Mathf.Lerp(1f, 1.3f, p / 0.4f) : Mathf.Lerp(1.3f, 1f, (p - 0.4f) / 0.6f);
                transform.localScale = new Vector3(scale, scale, 1f);
                float brighten = p < 0.4f ? p / 0.4f : Mathf.Lerp(1f, 0f, (p - 0.4f) / 0.6f);
                _image.color = Color.Lerp(startColor, Color.white, brighten * 0.6f);
                yield return null;
            }
            transform.localScale = Vector3.one;
            SetColor(null);
        }
    }
}

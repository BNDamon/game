using System;
using System.Collections;
using BlockMerge.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BlockMerge.Gameplay
{
    public sealed class PieceTraySlotView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private static readonly Color Idle = new Color32(0x1e, 0x21, 0x29, 0xff);
        private static readonly Color Used = new Color32(0x1e, 0x21, 0x29, 0x50);
        private static readonly Color InvalidFlash = new Color32(0xff, 0x55, 0x55, 0xff);

        private const float SlotSize = 220f;
        private const float MiniCell = 26f;
        private const float MiniSpacing = 2f;

        public event Action<int, PointerEventData> DragStarted;
        public event Action<PointerEventData> DragMoved;
        public event Action<int, PointerEventData> DragEnded;

        public int Index { get; private set; }

        private Image _background;
        private RectTransform _shapeContainer;
        private Piece _currentPiece;

        public static PieceTraySlotView Create(Transform parent, int index)
        {
            var rect = UiFactory.CreateRect($"TraySlot_{index}", parent);
            var layoutElement = rect.gameObject.AddComponent<LayoutElement>();
            layoutElement.preferredWidth = SlotSize;
            layoutElement.preferredHeight = SlotSize;

            var background = rect.gameObject.AddComponent<Image>();
            background.sprite = UiFactory.RoundedSprite;
            background.type = Image.Type.Sliced;
            background.color = Idle;

            var shapeRect = UiFactory.CreateRect("Shape", rect);
            shapeRect.anchorMin = Vector2.zero;
            shapeRect.anchorMax = Vector2.one;
            shapeRect.offsetMin = Vector2.zero;
            shapeRect.offsetMax = Vector2.zero;

            var slot = rect.gameObject.AddComponent<PieceTraySlotView>();
            slot.Index = index;
            slot._background = background;
            slot._shapeContainer = shapeRect;

            return slot;
        }

        /// <summary>Renders the slot's piece. isBeingDragged hides the mini-shape (a floating
        /// ghost takes its place under the finger) without hiding the slot itself, so the
        /// tray layout doesn't jump around mid-drag.</summary>
        public void Render(Piece piece, bool isBeingDragged)
        {
            _currentPiece = piece;

            for (int i = _shapeContainer.childCount - 1; i >= 0; i--)
                Destroy(_shapeContainer.GetChild(i).gameObject);

            if (piece == null)
            {
                _background.color = Used;
                return;
            }

            _background.color = Idle;
            if (isBeingDragged) return;

            int maxRow = 0, maxCol = 0;
            foreach (var cell in piece.Cells)
            {
                maxRow = Mathf.Max(maxRow, cell.Row);
                maxCol = Mathf.Max(maxCol, cell.Col);
            }

            float shapeWidth = (maxCol + 1) * MiniCell + maxCol * MiniSpacing;
            float shapeHeight = (maxRow + 1) * MiniCell + maxRow * MiniSpacing;
            var color = BlockColorPalette.ToColor(piece.Color);

            foreach (var cellCoord in piece.Cells)
            {
                var cellRect = UiFactory.CreateRect("MiniCell", _shapeContainer);
                cellRect.sizeDelta = new Vector2(MiniCell, MiniCell);
                cellRect.anchorMin = cellRect.anchorMax = new Vector2(0.5f, 0.5f);

                float x = -shapeWidth / 2f + MiniCell / 2f + cellCoord.Col * (MiniCell + MiniSpacing);
                float y = shapeHeight / 2f - MiniCell / 2f - cellCoord.Row * (MiniCell + MiniSpacing);
                cellRect.anchoredPosition = new Vector2(x, y);

                var image = cellRect.gameObject.AddComponent<Image>();
                image.sprite = UiFactory.RoundedSprite;
                image.type = Image.Type.Sliced;
                image.color = color;
                UiFactory.AddRoundedHighlight(image.transform);
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_currentPiece == null) return;
            DragStarted?.Invoke(Index, eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_currentPiece == null) return;
            DragMoved?.Invoke(eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (_currentPiece == null) return;
            DragEnded?.Invoke(Index, eventData);
        }

        /// <summary>Brief red flash + shake so a failed drop is unmistakable, not just a
        /// silent snap-back.</summary>
        public void PlayInvalidDropFeedback()
        {
            StopCoroutine(nameof(InvalidDropRoutine));
            StartCoroutine(nameof(InvalidDropRoutine));
        }

        private IEnumerator InvalidDropRoutine()
        {
            const float duration = 0.28f;
            var baseColor = _background.color;
            var baseAnchoredPos = ((RectTransform)transform).anchoredPosition;
            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                float p = t / duration;
                _background.color = Color.Lerp(InvalidFlash, baseColor, p);
                float shake = Mathf.Sin(p * Mathf.PI * 6f) * (1f - p) * 8f;
                ((RectTransform)transform).anchoredPosition = baseAnchoredPos + new Vector2(shake, 0f);
                yield return null;
            }
            _background.color = baseColor;
            ((RectTransform)transform).anchoredPosition = baseAnchoredPos;
        }
    }
}

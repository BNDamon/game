using System;
using BlockMerge.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BlockMerge.Gameplay
{
    public sealed class PieceTraySlotView : MonoBehaviour, IPointerClickHandler
    {
        private static readonly Color Idle = new Color32(0x1e, 0x21, 0x29, 0xff);
        private static readonly Color Selected = new Color32(0xff, 0xd3, 0x4d, 0xff);

        private const float SlotSize = 220f;
        private const float MiniCell = 26f;
        private const float MiniSpacing = 2f;

        public event Action<int> Clicked;

        public int Index { get; private set; }

        private Image _background;
        private RectTransform _shapeContainer;

        public static PieceTraySlotView Create(Transform parent, int index)
        {
            var rect = UiFactory.CreateRect($"TraySlot_{index}", parent);
            var layoutElement = rect.gameObject.AddComponent<LayoutElement>();
            layoutElement.preferredWidth = SlotSize;
            layoutElement.preferredHeight = SlotSize;

            var background = rect.gameObject.AddComponent<Image>();
            background.sprite = UiFactory.SolidSprite;
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

        public void Render(Piece piece, bool selected)
        {
            for (int i = _shapeContainer.childCount - 1; i >= 0; i--)
                Destroy(_shapeContainer.GetChild(i).gameObject);

            _background.color = selected ? Selected : Idle;
            gameObject.SetActive(piece != null);
            if (piece == null) return;

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
                image.sprite = UiFactory.SolidSprite;
                image.color = color;
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (gameObject.activeSelf) Clicked?.Invoke(Index);
        }
    }
}

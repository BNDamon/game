using System;
using System.Collections.Generic;
using BlockMerge.Core;
using UnityEngine;
using UnityEngine.UI;

namespace BlockMerge.Gameplay
{
    public sealed class GridView : MonoBehaviour
    {
        public event Action<int, int> CellClicked;

        private CellView[,] _cells;
        private int _size;
        private readonly List<GridCoord> _previewedCells = new List<GridCoord>();

        public static GridView Create(Transform parent, int size, float areaSize)
        {
            var rect = UiFactory.CreateRect("GridView", parent);
            rect.sizeDelta = new Vector2(areaSize, areaSize);

            var layoutElement = rect.gameObject.AddComponent<LayoutElement>();
            layoutElement.preferredWidth = areaSize;
            layoutElement.preferredHeight = areaSize;

            var panel = rect.gameObject.AddComponent<Image>();
            panel.sprite = UiFactory.RoundedSprite;
            panel.type = Image.Type.Sliced;
            panel.color = new Color32(0x1e, 0x21, 0x29, 0xff);

            const float spacing = 4f;
            const float padding = 8f;
            var grid = rect.gameObject.AddComponent<GridLayoutGroup>();
            grid.padding = new RectOffset((int)padding, (int)padding, (int)padding, (int)padding);
            grid.spacing = new Vector2(spacing, spacing);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = size;
            float cell = (areaSize - padding * 2 - spacing * (size - 1)) / size;
            grid.cellSize = new Vector2(cell, cell);

            var view = rect.gameObject.AddComponent<GridView>();
            view._size = size;
            view._cells = new CellView[size, size];

            for (int r = 0; r < size; r++)
            {
                for (int c = 0; c < size; c++)
                {
                    var cellView = CellView.Create(rect, r, c);
                    cellView.Clicked += cv => view.CellClicked?.Invoke(cv.Row, cv.Col);
                    view._cells[r, c] = cellView;
                }
            }

            return view;
        }

        public void Render(Board board)
        {
            for (int r = 0; r < _size; r++)
            {
                for (int c = 0; c < _size; c++)
                {
                    var color = board[r, c];
                    _cells[r, c].SetColor(color.HasValue ? BlockColorPalette.ToColor(color.Value) : (Color?)null);
                }
            }
        }

        public CellView GetCell(int row, int col) => _cells[row, col];

        public bool TryGetCellAtScreenPoint(Vector2 screenPoint, Camera camera, out int row, out int col)
        {
            for (int r = 0; r < _size; r++)
            {
                for (int c = 0; c < _size; c++)
                {
                    if (RectTransformUtility.RectangleContainsScreenPoint(_cells[r, c].RectTransform, screenPoint, camera))
                    {
                        row = r;
                        col = c;
                        return true;
                    }
                }
            }
            row = -1;
            col = -1;
            return false;
        }

        /// <summary>Highlights the cells a piece would occupy if dropped with its top-left
        /// cell at `anchor`. Out-of-bounds cells are silently skipped (the drag can still be
        /// partially off-grid while hovering near an edge).</summary>
        public void ShowPlacementPreview(Piece piece, GridCoord anchor, bool valid)
        {
            ClearPlacementPreview();
            foreach (var offset in piece.Cells)
            {
                var coord = anchor.Offset(offset);
                if (coord.Row < 0 || coord.Row >= _size || coord.Col < 0 || coord.Col >= _size) continue;
                _cells[coord.Row, coord.Col].ShowPreview(valid);
                _previewedCells.Add(coord);
            }
        }

        public void ClearPlacementPreview()
        {
            foreach (var coord in _previewedCells)
                _cells[coord.Row, coord.Col].ClearPreview();
            _previewedCells.Clear();
        }
    }
}

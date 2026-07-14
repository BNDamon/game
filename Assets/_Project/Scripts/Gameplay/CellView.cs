using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BlockMerge.Gameplay
{
    public sealed class CellView : MonoBehaviour, IPointerClickHandler
    {
        private static readonly Color EmptyColor = new Color32(0x22, 0x25, 0x2d, 0xff);

        public event Action<CellView> Clicked;

        public int Row { get; private set; }
        public int Col { get; private set; }

        private Image _image;

        public static CellView Create(Transform parent, int row, int col)
        {
            var image = UiFactory.CreatePanel($"Cell_{row}_{col}", parent, EmptyColor);
            var cell = image.gameObject.AddComponent<CellView>();
            cell._image = image;
            cell.Row = row;
            cell.Col = col;
            return cell;
        }

        public void SetColor(Color? color)
        {
            _image.color = color ?? EmptyColor;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Clicked?.Invoke(this);
        }
    }
}

using System;
using BlockMerge.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BlockMerge.Gameplay
{
    public sealed class PieceTrayView : MonoBehaviour
    {
        public event Action<int, PointerEventData> DragStarted;
        public event Action<PointerEventData> DragMoved;
        public event Action<int, PointerEventData> DragEnded;

        private PieceTraySlotView[] _slots;

        public static PieceTrayView Create(Transform parent, int slotCount)
        {
            var rect = UiFactory.CreateRect("PieceTrayView", parent);
            var layoutElement = rect.gameObject.AddComponent<LayoutElement>();
            layoutElement.preferredHeight = 240f;

            var layout = rect.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 14f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            var view = rect.gameObject.AddComponent<PieceTrayView>();
            view._slots = new PieceTraySlotView[slotCount];
            for (int i = 0; i < slotCount; i++)
            {
                var slot = PieceTraySlotView.Create(rect, i);
                slot.DragStarted += (idx, data) => view.DragStarted?.Invoke(idx, data);
                slot.DragMoved += data => view.DragMoved?.Invoke(data);
                slot.DragEnded += (idx, data) => view.DragEnded?.Invoke(idx, data);
                view._slots[i] = slot;
            }

            return view;
        }

        public void Render(Piece[] tray, int? draggingIndex)
        {
            for (int i = 0; i < _slots.Length; i++)
                _slots[i].Render(tray[i], draggingIndex.HasValue && draggingIndex.Value == i);
        }

        public void PlayInvalidDropFeedback(int index) => _slots[index].PlayInvalidDropFeedback();
    }
}

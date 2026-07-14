using BlockMerge.Core;
using UnityEngine;

namespace BlockMerge.Gameplay
{
    /// <summary>Maps Core's engine-agnostic BlockColor to actual RGB. This is the one place
    /// the prototype's COLORS array becomes a Unity Color — Core itself never touches
    /// UnityEngine types.</summary>
    public static class BlockColorPalette
    {
        private static readonly Color32[] Colors =
        {
            new Color32(0xff, 0x5d, 0x6c, 0xff), // Red
            new Color32(0x4d, 0xd0, 0xe1, 0xff), // Cyan
            new Color32(0xff, 0xd3, 0x4d, 0xff), // Yellow
            new Color32(0x7c, 0x5c, 0xff, 0xff), // Purple
            new Color32(0x5d, 0xdc, 0x7a, 0xff), // Green
        };

        public static Color32 ToColor32(BlockColor color) => Colors[(int)color];
        public static Color ToColor(BlockColor color) => ToColor32(color);
    }
}

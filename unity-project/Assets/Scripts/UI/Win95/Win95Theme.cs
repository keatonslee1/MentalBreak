using UnityEngine;

namespace MentalBreak.UI.Win95
{
    /// <summary>
    /// Central configuration for Windows 95 UI theme colors and settings.
    /// </summary>
    public static class Win95Theme
    {
        // Windows 95 Color Palette
        public static readonly Color ColorWhite = new Color32(255, 255, 255, 255);
        public static readonly Color ColorLightHighlight = new Color32(240, 240, 240, 255);
        public static readonly Color ColorLightGray = new Color32(223, 223, 223, 255);
        public static readonly Color ColorGray = new Color32(192, 192, 192, 255);       // #C0C0C0 - main background (Figma)
        public static readonly Color ColorMidGray = new Color32(129, 129, 129, 255);    // #818181 (Figma)
        public static readonly Color ColorDarkGray = new Color32(126, 126, 126, 255);   // #7E7E7E
        public static readonly Color ColorDark = new Color32(38, 38, 38, 255);          // #262626
        public static readonly Color ColorBlack = new Color32(0, 0, 0, 255);
        public static readonly Color ColorTitleActive = new Color32(0, 0, 128, 255);    // Navy blue
        public static readonly Color ColorTitleInactive = new Color32(128, 128, 128, 255);

        // Semantic colors
        public static readonly Color ButtonFace = ColorGray;
        public static readonly Color ButtonHighlight = ColorWhite;
        public static readonly Color ButtonShadow = ColorMidGray;
        public static readonly Color ButtonDarkShadow = ColorBlack;
        public static readonly Color WindowBackground = ColorGray;
        public static readonly Color WindowText = ColorBlack;
        public static readonly Color TitleBarActive = ColorTitleActive;
        public static readonly Color TitleBarInactive = ColorTitleInactive;
        public static readonly Color TitleBarText = ColorWhite;

        // ContentArea sunken border colors (from Figma CSS)
        public static readonly Color SunkenBorderDarkOuter = new Color32(0, 0, 0, 255);        // #000000
        public static readonly Color SunkenBorderDarkInner = new Color32(128, 128, 128, 255);  // #808080
        public static readonly Color SunkenBorderLightOuter = new Color32(193, 193, 193, 255); // #C1C1C1
        public static readonly Color SunkenBorderLightInner = new Color32(255, 255, 255, 255); // #FFFFFF

        // Sprite paths
        public const string SpritePath = "Graphics/UI/Win95/";

        // Standard sizes (2x scale for visibility)
        public const int TitleBarHeight = 36;
        public const int MenuBarHeight = 32;
        public const int StatusBarHeight = 32;
        public const int ButtonHeight = 28;          // Title bar button height
        public const int TitleBarButtonWidth = 32;   // Title bar button width
        public const int BorderWidth = 8;

        /// <summary>
        /// Load a Win95 sprite from Resources.
        /// </summary>
        public static Sprite LoadSprite(string spriteName)
        {
            return Resources.Load<Sprite>(SpritePath + spriteName);
        }
    }
}

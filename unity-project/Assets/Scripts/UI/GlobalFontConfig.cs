using TMPro;
using UnityEngine;

/// <summary>
/// Stores references to the game's global fonts.
/// This asset is generated/updated in-editor by GlobalFontConfigGenerator.
/// </summary>
[CreateAssetMenu(menuName = "Mental Break/Global Font Config", fileName = "GlobalFontConfig")]
public sealed class GlobalFontConfig : ScriptableObject
{
    [Header("TextMeshPro")]
    public TMP_FontAsset primarySdfFont;

    [Header("Legacy UI Text")]
    public Font primaryTtfFont;
}




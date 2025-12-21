using System;
using System.Text;
using UnityEngine;

/// <summary>
/// Utility class for exporting and importing save data as Base64-encoded strings.
/// Enables copy/paste save sharing between devices and manual backup.
/// </summary>
public static class SaveExporter
{
    // Prefix to identify Mental Break save strings
    private const string SAVE_STRING_PREFIX = "MBSAVE_";

    // Current export format version (for future migration support)
    private const int EXPORT_VERSION = 1;

    /// <summary>
    /// Export SaveData to a Base64-encoded string that can be copied/pasted.
    /// </summary>
    /// <param name="data">The save data to export</param>
    /// <returns>A portable Base64 string, or null if export fails</returns>
    public static string ExportToString(SaveData data)
    {
        if (data == null)
        {
            Debug.LogError("SaveExporter: Cannot export null SaveData");
            return null;
        }

        try
        {
            // Serialize to JSON
            string json = JsonUtility.ToJson(data, false); // Compact format

            // Convert to bytes
            byte[] bytes = Encoding.UTF8.GetBytes(json);

            // Encode to Base64
            string base64 = Convert.ToBase64String(bytes);

            // Add prefix for identification
            string exportString = $"{SAVE_STRING_PREFIX}{EXPORT_VERSION}_{base64}";

            Debug.Log($"SaveExporter: Exported save data ({bytes.Length} bytes → {exportString.Length} chars)");
            return exportString;
        }
        catch (Exception e)
        {
            Debug.LogError($"SaveExporter: Export failed: {e.Message}");
            return null;
        }
    }

    /// <summary>
    /// Import SaveData from a Base64-encoded string.
    /// </summary>
    /// <param name="exportString">The Base64 string to import</param>
    /// <returns>The deserialized SaveData, or null if import fails</returns>
    public static SaveData ImportFromString(string exportString)
    {
        if (string.IsNullOrEmpty(exportString))
        {
            Debug.LogError("SaveExporter: Cannot import empty string");
            return null;
        }

        try
        {
            string base64;

            // Check for prefix
            if (exportString.StartsWith(SAVE_STRING_PREFIX))
            {
                // Parse version and extract base64
                int underscoreIndex = exportString.IndexOf('_', SAVE_STRING_PREFIX.Length);
                if (underscoreIndex == -1)
                {
                    Debug.LogError("SaveExporter: Invalid export string format (missing version separator)");
                    return null;
                }

                string versionStr = exportString.Substring(SAVE_STRING_PREFIX.Length, underscoreIndex - SAVE_STRING_PREFIX.Length);
                if (!int.TryParse(versionStr, out int version))
                {
                    Debug.LogError($"SaveExporter: Invalid export version: {versionStr}");
                    return null;
                }

                // Version migration could be handled here in the future
                if (version > EXPORT_VERSION)
                {
                    Debug.LogWarning($"SaveExporter: Export version {version} is newer than supported version {EXPORT_VERSION}. Import may fail.");
                }

                base64 = exportString.Substring(underscoreIndex + 1);
            }
            else
            {
                // Try to parse as raw base64 (legacy support)
                base64 = exportString.Trim();
            }

            // Decode Base64
            byte[] bytes = Convert.FromBase64String(base64);

            // Convert to JSON string
            string json = Encoding.UTF8.GetString(bytes);

            // Deserialize
            SaveData data = JsonUtility.FromJson<SaveData>(json);

            if (data == null)
            {
                Debug.LogError("SaveExporter: Deserialization returned null");
                return null;
            }

            Debug.Log($"SaveExporter: Imported save data (version: {data.version}, node: {data.gameState?.currentNode})");
            return data;
        }
        catch (FormatException)
        {
            Debug.LogError("SaveExporter: Invalid Base64 format");
            return null;
        }
        catch (Exception e)
        {
            Debug.LogError($"SaveExporter: Import failed: {e.Message}");
            return null;
        }
    }

    /// <summary>
    /// Validate that a string is a valid save export string without fully parsing it.
    /// </summary>
    /// <param name="exportString">The string to validate</param>
    /// <returns>True if the string appears to be a valid save export</returns>
    public static bool ValidateSaveString(string exportString)
    {
        if (string.IsNullOrEmpty(exportString))
        {
            return false;
        }

        try
        {
            string base64;

            // Check for prefix
            if (exportString.StartsWith(SAVE_STRING_PREFIX))
            {
                int underscoreIndex = exportString.IndexOf('_', SAVE_STRING_PREFIX.Length);
                if (underscoreIndex == -1)
                {
                    return false;
                }
                base64 = exportString.Substring(underscoreIndex + 1);
            }
            else
            {
                base64 = exportString.Trim();
            }

            // Validate Base64 format
            byte[] bytes = Convert.FromBase64String(base64);

            // Quick validation: should be valid JSON starting with '{'
            string json = Encoding.UTF8.GetString(bytes);
            return json.TrimStart().StartsWith("{");
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Get the approximate size of an export string in a human-readable format.
    /// </summary>
    public static string GetExportSizeDescription(string exportString)
    {
        if (string.IsNullOrEmpty(exportString))
        {
            return "0 bytes";
        }

        int length = exportString.Length;
        if (length < 1024)
        {
            return $"{length} chars";
        }
        else
        {
            return $"{length / 1024f:F1} KB";
        }
    }
}

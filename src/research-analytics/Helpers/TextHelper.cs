namespace FxWebPortal.Helpers;

public static class TextHelper
{
    /// <summary>Returns a truncated string with trailing ellipsis, or empty string if input is empty.</summary>
    public static string Truncate(string? text, int maxLength)
    {
        if (string.IsNullOrEmpty(text)) return string.Empty;
        return text.Length <= maxLength ? text : text[..maxLength] + "…";
    }
}

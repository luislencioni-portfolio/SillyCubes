using UnityEngine;

/// <summary>
/// Unified custom attribute for displaying colored fields in the Inspector.
/// Supports two modes:
/// 1. Color only: [ColoredField(r, g, b)] - for enum, string, or any field without range
/// 2. Color with range: [ColoredField(min, max, r, g, b)] - for float/int fields with slider
///
/// Examples:
/// - [ColoredField(1f, 1f, 0f)] → Yellow text for enum/string
/// - [ColoredField(0f, 2f, 0f, 1f, 0f)] → Green slider from 0 to 2 for float/int
/// </summary>
public class ColoredFieldAttribute : PropertyAttribute
{
    public Color textColor;
    public float min;
    public float max;
    public bool hasRange;

    /// <summary>
    /// Constructor for color-only mode (no range slider).
    /// Use for enum, string, or any field that doesn't need a range constraint.
    /// </summary>
    /// <param name="r">Red component of text color (0-1)</param>
    /// <param name="g">Green component of text color (0-1)</param>
    /// <param name="b">Blue component of text color (0-1)</param>
    public ColoredFieldAttribute(float r, float g, float b)
    {
        textColor = new Color(r, g, b, 1f);
        hasRange = false;
        min = 0f;
        max = 0f;
    }

    /// <summary>
    /// Constructor for color with range mode (displays slider).
    /// Use for float or int fields that need range constraints.
    /// </summary>
    /// <param name="min">Minimum value for the slider</param>
    /// <param name="max">Maximum value for the slider</param>
    /// <param name="r">Red component of text color (0-1)</param>
    /// <param name="g">Green component of text color (0-1)</param>
    /// <param name="b">Blue component of text color (0-1)</param>
    public ColoredFieldAttribute(float min, float max, float r, float g, float b)
    {
        this.min = min;
        this.max = max;
        textColor = new Color(r, g, b, 1f);
        hasRange = true;
    }
}

/// <summary>
/// Represents the type of metric for animation purposes.
/// Order determines animation priority (lower = first).
/// </summary>
public enum MetricType
{
    Engagement = 0,
    Sanity = 1,
    Suspicion = 2
}

/// <summary>
/// Data structure representing a metric change event for animation.
/// </summary>
public struct MetricChange
{
    /// <summary>The type of metric that changed.</summary>
    public MetricType Type;

    /// <summary>The value before the change (0-100).</summary>
    public float OldValue;

    /// <summary>The value after the change (0-100).</summary>
    public float NewValue;

    /// <summary>
    /// Returns true if the metric increased, false if decreased.
    /// </summary>
    public bool IsIncrease => NewValue > OldValue;

    public MetricChange(MetricType type, float oldValue, float newValue)
    {
        Type = type;
        OldValue = oldValue;
        NewValue = newValue;
    }
}

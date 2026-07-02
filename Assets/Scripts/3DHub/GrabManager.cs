using UnityEngine;

/// <summary>
/// Static tracker ensuring only one HubGremurin can be picked up / held / charging
/// at any given time. No scene setup required - just exists automatically.
/// </summary>
public static class GrabManager
{
    public static HubGremurin CurrentlyHeld { get; private set; }

    public static bool TryClaim(HubGremurin grem)
    {
        if (CurrentlyHeld != null) return false;
        CurrentlyHeld = grem;
        return true;
    }

    public static void Release(HubGremurin grem)
    {
        if (CurrentlyHeld == grem)
            CurrentlyHeld = null;
    }
}
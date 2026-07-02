using UnityEngine;

public abstract class GremSecretEffect : ScriptableObject
{
    /// <summary>
    /// This method will execute after the 3-second spotlight dance finishes.
    /// </summary>
    public abstract void TriggerSecret(HubGremurin grem, TrackData track);
}
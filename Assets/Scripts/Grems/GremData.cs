using UnityEngine;

[CreateAssetMenu(fileName = "New GremData", menuName = "Gremurin/GremData")]
public class GremData : ScriptableObject
{
    public string gremName;
    public Sprite sprite;

    public float maxHealth = 3f;

    public float maxHunger = 100f;
    public float hungerRate = 2f;

    public float currencyOutputRate = 5f;
    public float currencyOutputAmount = 1f;

    public float wanderRadius = 1.5f;
    public float wanderPauseMin = 2f;
    public float wanderPauseMax = 5f;
    public float moveSpeed = 1.5f;

    public GremRole role;
}

public enum GremRole
{
    Producer,
    Fighter,
    Breeder,
    Support,
    Specialist
}
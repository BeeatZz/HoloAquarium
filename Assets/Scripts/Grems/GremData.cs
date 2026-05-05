using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New GremData", menuName = "Gremurin/GremData")]
public class GremData : ScriptableObject
{
    [Header("Identity")]
    public string gremName;
    public Sprite sprite;

    [Header("Pokedex Entry")]
    [TextArea(2, 4)]
    public string flavorText;
    public List<GremSkill> skills;

    [Header("Health")]
    public float maxHealth = 3f;

    [Header("Hunger")]
    public float maxHunger = 100f;
    public float hungerRate = 2f;

    [Header("Output")]
    public float currencyOutputRate = 5f;
    public float currencyOutputAmount = 1f;

    [Header("Movement")]
    public float wanderRadius = 1.5f;
    public float wanderPauseMin = 2f;
    public float wanderPauseMax = 5f;
    public float moveSpeed = 1.5f;

    [Header("Role")]
    public GremRole role;
}

[System.Serializable]
public class GremSkill
{
    public string skillName;
    [TextArea(1, 3)]
    public string skillDescription;
}

public enum GremRole
{
    Producer,
    Fighter,
    Breeder,
    Support,
    Specialist
}
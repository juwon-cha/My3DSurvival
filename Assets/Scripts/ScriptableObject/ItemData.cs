using System;
using UnityEngine;

public enum EItemType
{
    Equipable,
    Consumable,
    Resource
}

public enum EConsumableType
{
    Health,
    Hunger,
}

[Serializable]
public class ItemDataConsumable
{
    public EConsumableType Type;
    public float Value;
}

[CreateAssetMenu(fileName = "Item", menuName = "New Item")]
public class ItemData : ScriptableObject
{
    [Header("Item Info")]
    public string DisplayName;
    public string Description;
    public EItemType ItemType;
    public Sprite Icon;
    public GameObject DropPrefab;

    [Header("Stacking")]
    public bool CanStack;
    public int MaxStackAmount;

    [Header("Consumable")]
    public ItemDataConsumable[] Consumables;
}

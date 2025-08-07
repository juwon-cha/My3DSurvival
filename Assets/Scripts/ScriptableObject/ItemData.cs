using UnityEngine;

public enum EItemType
{
    Equipable,
    Consumable,
    Resource
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
}

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIInventory : MonoBehaviour
{
    public ItemSlot[] Slots;

    public GameObject InventoryWindow;
    public Transform SlotPanel;

    [Header("Select Item")]
    public TextMeshProUGUI SelectedItemName;
    public TextMeshProUGUI SelectedItemDescription;
    public TextMeshProUGUI SelectedStatName;
    public TextMeshProUGUI SelectedStatValue;
    public GameObject UseButton;
    public GameObject EquipButton;
    public GameObject UnEquipButton;
    public GameObject DropButton;

    private PlayerController controller;
    private PlayerCondition condition;

    private void Start()
    {
        controller = CharacterManager.Instance.Player.PlayerController;
        condition = CharacterManager.Instance.Player.PlayerCondition;

        InventoryWindow.SetActive(false);
        Slots = new ItemSlot[SlotPanel.childCount];

        for(int i = 0; i < Slots.Length; i++)
        {
            
        }
    }
}

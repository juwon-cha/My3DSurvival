using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class UIInventory : MonoBehaviour
{
    public ItemSlot[] Slots;

    public GameObject InventoryWindow;
    public Transform SlotPanel;
    public Transform DropPosition;

    [Header("Select Item")]
    public TextMeshProUGUI SelectedItemName;
    public TextMeshProUGUI SelectedItemDescription;
    public TextMeshProUGUI SelectedStatName;
    public TextMeshProUGUI SelectedStatValue;
    public GameObject UseButton;
    public GameObject EquipButton;
    public GameObject UnEquipButton;
    public GameObject DropButton;

    private PlayerController _controller;
    private PlayerCondition _condition;

    private ItemData _selectedItem;
    private int _selectedItemIndex = 0;

    private int CurEquipIndex;

    private void Start()
    {
        _controller = CharacterManager.Instance.Player.PlayerController;
        _condition = CharacterManager.Instance.Player.PlayerCondition;
        DropPosition = CharacterManager.Instance.Player.DropPosition;

        _controller.Inventory += Toggle;
        CharacterManager.Instance.Player.AddItem += AddItem;

        InventoryWindow.SetActive(false);
        Slots = new ItemSlot[SlotPanel.childCount];

        for(int i = 0; i < Slots.Length; i++)
        {
            Slots[i] = SlotPanel.GetChild(i).GetComponent<ItemSlot>();
            Slots[i].Index = i;
            Slots[i].Inventory = this;
        }

        ClearSelectedItemWindow();
    }

    void ClearSelectedItemWindow()
    {
        SelectedItemName.text = string.Empty;
        SelectedItemDescription.text = string.Empty;
        SelectedStatName.text = string.Empty;
        SelectedStatValue.text = string.Empty;

        UseButton.SetActive(false);
        EquipButton.SetActive(false);
        UnEquipButton.SetActive(false);
        DropButton.SetActive(false);
    }

    public void Toggle()
    {
        if(IsOpen())
        {
            InventoryWindow.SetActive(false);
        }
        else
        {
            InventoryWindow.SetActive(true);
        }
    }

    public bool IsOpen()
    {
        return InventoryWindow.activeInHierarchy;
    }

    private void AddItem()
    {
        ItemData data = CharacterManager.Instance.Player.ItemData;

        // 아이템이 중복 가능한지 체크
        if(data.CanStack)
        {
            ItemSlot slot = GetItemStack(data);
            if(slot != null)
            {
                ++slot.Quantity;

                UpdateUI();

                CharacterManager.Instance.Player.ItemData = null;
                return;
            }
        }

        // 비어있는 슬롯 가져옴
        ItemSlot emptySlot = GetEmptySlot(data);

        // 빈 슬롯 있다면
        if(emptySlot != null)
        {
            emptySlot.Item = data;
            emptySlot.Quantity = 1;

            UpdateUI();

            CharacterManager.Instance.Player.ItemData = null;
            return;
        }

        // 빈 슬롯 없다면 파밍한 아이템 버리기
        ThrowItem(data);

        CharacterManager.Instance.Player.ItemData = null;
    }

    private void UpdateUI()
    {
        for(int i = 0; i < Slots.Length; ++i)
        {
            if(Slots[i].Item != null)
            {
                Slots[i].Set();
            }
            else
            {
                Slots[i].Clear();
            }
        }
    }

    ItemSlot GetItemStack(ItemData data)
    {
        for(int i=0; i < Slots.Length; i++)
        {
            if (Slots[i].Item == data && Slots[i].Quantity < data.MaxStackAmount)
            {
                return Slots[i];
            }
        }

        return null;
    }

    ItemSlot GetEmptySlot(ItemData data)
    {
        for (int i = 0; i < Slots.Length; i++)
        {
            if (Slots[i].Item == null)
            {
                return Slots[i];
            }
        }

        return null;
    }

    private void ThrowItem(ItemData data)
    {
        Instantiate(data.DropPrefab, DropPosition.position, Quaternion.Euler(Vector3.one * Random.value * 360));
    }

    public void SelectItem(int index)
    {
        if (Slots[index].Item == null)
        {
            return;
        }

        _selectedItem = Slots[index].Item;
        _selectedItemIndex = index;

        SelectedStatName.text = _selectedItem.DisplayName;
        SelectedItemDescription.text = _selectedItem.Description;

        SelectedStatName.text = string.Empty;
        SelectedStatValue.text = string.Empty;

        for(int i = 0; i < _selectedItem.Consumables.Length; i++)
        {
            SelectedStatName.text += _selectedItem.Consumables[i].Type.ToString() + "\n";
            SelectedStatValue.text += _selectedItem.Consumables[i].Value.ToString() + "\n"; ;
        }

        UseButton.SetActive(_selectedItem.ItemType == EItemType.Consumable);
        EquipButton.SetActive(_selectedItem.ItemType == EItemType.Equipable && !Slots[index].Equipped);
        UnEquipButton.SetActive(_selectedItem.ItemType == EItemType.Equipable && Slots[index].Equipped);
        DropButton.SetActive(true);
    }

    public void OnUseButton()
    {
        if(_selectedItem.ItemType == EItemType.Consumable)
        {
            for(int i = 0; i < _selectedItem.Consumables.Length; ++i)
            {
                switch(_selectedItem.Consumables[i].Type)
                {
                    case EConsumableType.Health:
                        _condition.Heal(_selectedItem.Consumables[i].Value);
                        break;

                    case EConsumableType.Hunger:
                        _condition.Eat(_selectedItem.Consumables[i].Value);
                        break;

                    default:
                        break;
                }
            }

            RemoveSelectedItem();
        }
    }

    public void OnDropButton()
    {
        ThrowItem(_selectedItem);
        RemoveSelectedItem();
    }

    private void RemoveSelectedItem()
    {
        int selectedItemQuantity = --Slots[_selectedItemIndex].Quantity;

        if(selectedItemQuantity <= 0)
        {

            _selectedItem = null;
            Slots[_selectedItemIndex].Item = null;
            _selectedItemIndex = -1;
            ClearSelectedItemWindow();
        }

        UpdateUI();
    }

    public void OnEquipButton()
    {
        if (Slots[CurEquipIndex].Equipped)
        {
            UnEquip(CurEquipIndex);
        }

        Slots[_selectedItemIndex].Equipped = true;
        CurEquipIndex = _selectedItemIndex;
        CharacterManager.Instance.Player.Equip.EquipNew(_selectedItem);
        UpdateUI();

        SelectItem(_selectedItemIndex);
    }

    public void OnUnEquipButton()
    {
        UnEquip(_selectedItemIndex);
    }

    private void UnEquip(int index)
    {
        Slots[index].Equipped = false;
        CharacterManager.Instance.Player.Equip.UnEquip();
        UpdateUI();

        if(_selectedItemIndex == index)
        {
            SelectItem(_selectedItemIndex);
        }
    }
}

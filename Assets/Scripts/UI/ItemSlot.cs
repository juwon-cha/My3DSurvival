using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemSlot : MonoBehaviour
{
    public ItemData Item;

    public Button button;
    public Image Icon;
    public TextMeshProUGUI QuantityText;
    private Outline _outline;

    public UIInventory Inventory;

    public int Index;
    public bool Equipped;
    public int Quantity;

    private void Awake()
    {
        _outline = GetComponent<Outline>();
    }

    private void OnEnable()
    {
        _outline.enabled = Equipped;
    }

    public void Set()
    {
        Icon.gameObject.SetActive(true);
        Icon.sprite = Item.Icon;
        Debug.Log(QuantityText.ToString());
        QuantityText.text = Quantity > 1 ? Quantity.ToString() : string.Empty;

        if(_outline != null)
        {
            _outline.enabled = Equipped;
        }
    }

    public void Clear()
    {
        Item = null;
        Icon.gameObject.SetActive(false);
        QuantityText.text = string.Empty;
    }

    public void OnClickButton()
    {
        Inventory.SelectItem(Index);
    }
}

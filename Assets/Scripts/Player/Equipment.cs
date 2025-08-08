using UnityEngine;
using UnityEngine.InputSystem;

public class Equipment : MonoBehaviour
{
    public Equip CurEquip;
    public Transform EquipParent;

    private PlayerController _controller;
    private PlayerCondition _condition;

    private void Start()
    {
        _controller = GetComponent<PlayerController>();
        _condition = GetComponent<PlayerCondition>();
    }

    public void EquipNew(ItemData data)
    {
        // 이미 장착된 아이템이 있다면 해제
        UnEquip();

        CurEquip = Instantiate(data.EquipPrefab, EquipParent).GetComponent<Equip>();
    }

    public void UnEquip()
    {
        if(CurEquip != null)
        {
            Destroy(CurEquip.gameObject);
            CurEquip = null;
        }
    }

    public void OnAttackInput(InputAction.CallbackContext context)
    {
        if(context.phase == InputActionPhase.Performed && CurEquip != null && _controller.CanLook)
        {
            CurEquip.OnAttackInput();
        }
    }
}

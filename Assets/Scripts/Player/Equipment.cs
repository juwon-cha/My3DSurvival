using UnityEngine;
using UnityEngine.InputSystem;

public class Equipment : MonoBehaviour
{
    public Equip CurEquip;
    public Transform WeaponSocket;
    public Transform HelmetSocket;

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

        switch (CurEquip.Type)
        {
            case EEquipType.Weapon:
                CurEquip = Instantiate(data.EquipPrefab, WeaponSocket).GetComponent<Equip>();
                break;

            case EEquipType.Armor:
                CurEquip = Instantiate(data.EquipPrefab, HelmetSocket).GetComponent<Equip>();
                ActivateSpeedBuff();
                break;

            default:
                break;
        }
        
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

    private void ActivateSpeedBuff()
    {
        if (CurEquip.Type == EEquipType.Armor)
        {
            EquipTool armor = (EquipTool)CurEquip;

            PlayerController controller = CharacterManager.Instance.Player.PlayerController;
            if(controller != null)
            {
                controller.MoveSpeed *= armor.BuffValue;
            }
        }
    }

    private void DeactivateSpeedBuff()
    {

    }
}

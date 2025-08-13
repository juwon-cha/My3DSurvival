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

        Transform socket = null;
        switch (data.EquipType)
        {
            case EEquipType.Weapon:
                socket = WeaponSocket;
                break;
            case EEquipType.Armor:
                socket = HelmetSocket;
                break;
        }

        if (socket != null)
        {
            CurEquip = Instantiate(data.EquipPrefab, socket).GetComponent<Equip>();

            // 버프가 있다면 활성화
            if (data.EquipType == EEquipType.Armor)
            {
                ActivateSpeedBuff();
            }
        }
    }

    public void UnEquip()
    {
        if(CurEquip != null)
        {
            // 장비 파괴 전 버프 비활성화
            if (CurEquip is EquipTool armor)
            {
                DeactivateBuff();
            }

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
        EquipTool armor = (EquipTool)CurEquip;

        PlayerController controller = CharacterManager.Instance.Player.PlayerController;
        if (controller != null)
        {
            controller.MoveSpeed *= armor.BuffValue;
            controller.SprintSpeed *= armor.BuffValue;
        }
    }

    private void DeactivateBuff()
    {
        EquipTool armor = (EquipTool)CurEquip;
        PlayerController controller = CharacterManager.Instance.Player.PlayerController;
        if (controller != null && armor.BuffValue != 0) // 0으로 나누는 것 방지
        {
            controller.MoveSpeed /= armor.BuffValue;
            controller.SprintSpeed /= armor.BuffValue;
        }
    }
}

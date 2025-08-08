using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipTool : Equip
{
    public float AttackRate;
    private bool isAttacking;
    public float AttackDistance;
    public float UseStamina;

    [Header("Resource Gathering")]
    public bool DoesGatherResources;

    [Header("Combat")]
    public bool DoesDealDamage;
    public int Damage;

    private Animator _animator;
    private Camera _camera;

    private void Start()
    {
        _animator = GetComponent<Animator>();
        _camera = Camera.main;
    }

    public override void OnAttackInput()
    {
        if(!isAttacking)
        {
            if(CharacterManager.Instance.Player.PlayerCondition.UseStamina(UseStamina))
            {
                isAttacking = true;
                _animator.SetTrigger("Attack");
                Invoke("OnCanAttack", AttackRate);
            }
        }
    }

    private void OnCanAttack()
    {
        isAttacking = false;
    }

    public void OnHit()
    {
        Ray ray = _camera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        RaycastHit hit;

        if(Physics.Raycast(ray, out hit, AttackDistance))
        {
            if(DoesGatherResources && hit.collider.TryGetComponent(out Resource resource))
            {
                resource.Gather(hit.point, hit.normal);
            }
        }
    }
}

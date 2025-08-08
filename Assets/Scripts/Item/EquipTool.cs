using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipTool : Equip
{
    public float AttackRate;
    private bool isAttacking;
    public float AttackDistance;

    [Header("Resource Gathering")]
    public bool DoesGatherResources;

    [Header("Combat")]
    public bool DoesDealDamage;
    public int Damage;
}

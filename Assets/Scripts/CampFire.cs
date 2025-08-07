using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CampFire : MonoBehaviour
{
    public int Damage;
    public float damageRate;

    List<IDamagable> things = new List<IDamagable>();

    private void Start()
    {
        InvokeRepeating("DealDamage", 0, damageRate);
    }

    private void DealDamage()
    {
        for(int i = 0; i < things.Count; i++)
        {
            things[i].TakePhysicalDamage(Damage);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent(out IDamagable damagable))
        {
            things.Add(damagable);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out IDamagable damagable))
        {
            things.Remove(damagable);
        }
    }
}

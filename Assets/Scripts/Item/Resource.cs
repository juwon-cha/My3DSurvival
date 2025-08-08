using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Resource : MonoBehaviour
{
    public ItemData ItemToGive;
    public int QuantityPerHit = 1;
    public int Capacity;

    public void Gather(Vector3 hitPoint, Vector3 hitNormal)
    {
        for(int i = 0; i < QuantityPerHit; i++)
        {
            if(Capacity <= 0)
            {
                break;
            }

            --Capacity;

            Instantiate(ItemToGive.DropPrefab, hitPoint + Vector3.up, Quaternion.LookRotation(hitNormal, Vector3.up));
        }
    }
}

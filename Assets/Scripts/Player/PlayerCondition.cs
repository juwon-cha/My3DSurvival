using System;
using UnityEngine;

public interface IDamagable
{
    void TakePhysicalDamage(int damage);
}

public class PlayerCondition : MonoBehaviour, IDamagable
{
    public UICondition UICondition;

    public Condition Health { get { return UICondition.Health; } }
    public Condition Hunger { get { return UICondition.Hunger; } }
    public Condition Stamina { get { return UICondition.Stamina; } }

    public float NoHungerHealthDecay;

    public event Action OnTakeDamage;

    private void Update()
    {
        Hunger.Subtract(Hunger.PassiveValue * Time.deltaTime);
        Stamina.Add(Stamina.PassiveValue * Time.deltaTime);

        if(Hunger.CurValue <= 0)
        {
            Health.Subtract(NoHungerHealthDecay * Time.deltaTime);
        }

        if(Health.CurValue <= 0)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        Health.Add(amount);
    }

    public void Eat(float amount)
    {
        Hunger.Add(amount);
    }

    public void Die()
    {
        Debug.Log("Die!");
    }

    public void TakePhysicalDamage(int damage)
    {
        Health.Subtract(damage);
        OnTakeDamage?.Invoke();
    }
}

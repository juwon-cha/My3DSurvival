using System;
using System.Collections;
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

    public bool IsInvincible { get; private set; } = false;
    public bool IsDoubleJumpActive { get; private set; } = false;

    private Coroutine _doubleJumpCoroutine;
    private Coroutine _invincibilityCoroutine;
    private Coroutine _speedUpCoroutine;

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
        // 무적 상태라면 데미지 무시
        if(IsInvincible)
        {
            return;
        }

        Health.Subtract(damage);
        OnTakeDamage?.Invoke();
    }

    public bool UseStamina(float amount)
    {
        if(Stamina.CurValue - amount < 0)
        {
            return false;
        }

        Stamina.Subtract(amount);

        return true;
    }

    public void ActivateDoubleJump(float duration)
    {
        if (_doubleJumpCoroutine != null)
        {
            StopCoroutine(_doubleJumpCoroutine);
        }

        _doubleJumpCoroutine = StartCoroutine(DoubleJumpRoutine(duration));
    }

    public void ActivateInvincibility(float duration)
    {
        if (_invincibilityCoroutine != null)
        {
            StopCoroutine(_invincibilityCoroutine);
        }

        _invincibilityCoroutine = StartCoroutine(InvincibilityRoutine(duration));
    }

    public void ActivateSpeedUp(float duration)
    {
        if (_speedUpCoroutine != null)
        {
            StopCoroutine(_speedUpCoroutine);
        }

        _speedUpCoroutine = StartCoroutine(SpeedUpRoutine(duration));
    }

    private IEnumerator DoubleJumpRoutine(float duration)
    {
        IsDoubleJumpActive = true;
        Debug.Log("DoubleJump Activated!");

        yield return new WaitForSeconds(duration);

        IsDoubleJumpActive = false;
        Debug.Log("DoubleJump Deactivated!");
    }

    private IEnumerator InvincibilityRoutine(float duration)
    {
        IsInvincible = true;
        Debug.Log("Invincibility Activated!");

        yield return new WaitForSeconds(duration);

        IsInvincible = false;
        Debug.Log("Invincibility Deactivated!");
    }

    private IEnumerator SpeedUpRoutine(float duration)
    {
        PlayerController controller = CharacterManager.Instance.Player.PlayerController;
        float originalMoveSpeed = controller.MoveSpeed;
        float originalSprintSpeed = controller.SprintSpeed;

        controller.MoveSpeed *= 2.0f; // 속도 2배 증가
        controller.SprintSpeed *= 2.0f;
        Debug.Log("SpeedUp Activated!");

        yield return new WaitForSeconds(duration);

        controller.MoveSpeed = originalMoveSpeed;
        controller.SprintSpeed = originalSprintSpeed;
        Debug.Log("SpeedUp Deactivated!");
    }
}

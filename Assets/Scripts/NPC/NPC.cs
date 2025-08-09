using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public enum EAIState
{
    Idle,
    Wandering,
    Attacking
}

public class NPC : MonoBehaviour, IDamagable
{
    [Header("Stat")]
    public int Health;
    public float WalkSpeed;
    public float RunSpeed;
    public ItemData[] DropOnDeath;

    [Header("AI")]
    private NavMeshAgent _agent;
    public float DetectDistance; // 목표 지점까지 거리
    private EAIState _aiState;

    [Header("Wandering")]
    public float MinWanderDistance;
    public float MaxWanderDistance;
    public float MinWanderWaitTime; // 새로운 목표 지점을 정할 때 기다리는 시간(랜덤 min - max)
    public float MaxWanderWaitTime;

    [Header("Combat")]
    public int Damage;
    public float AttackRate; // 공격 사이의 텀
    private float _lastAttackTime;
    public float AttackDistance; // 공격 가능 거리

    private float _playerDistance;

    public float FieldOfView = 120f; // 플레이어 탐지 시야 각

    private Animator _animator;
    private SkinnedMeshRenderer[] _meshRenderers;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
        _meshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
    }

    private void Start()
    {
        SetState(EAIState.Wandering);
    }

    private void Update()
    {
        // 플레이어와의 거리 계산
        _playerDistance = Vector3.Distance(transform.position, CharacterManager.Instance.Player.transform.position);

        _animator.SetBool("Moving", _aiState != EAIState.Idle);

        switch (_aiState)
        {
            case EAIState.Idle:
            case EAIState.Wandering:
                PassiveUpdate();
                break;

            case EAIState.Attacking:
                AttackingUpdate();
                break;

            default:
                break;
        }
    }

    public void SetState(EAIState state)
    {
        _aiState = state;

        switch (_aiState)
        {
            case EAIState.Idle:
                _agent.speed = WalkSpeed;
                _agent.isStopped = true;
                break;

            case EAIState.Wandering:
                _agent.speed = WalkSpeed;
                _agent.isStopped = false;
                break;

            case EAIState.Attacking:
                _agent.speed = RunSpeed;
                _agent.isStopped = false;
                break;

            default:
                break;
        }

        _animator.speed = _agent.speed / WalkSpeed;
    }

    private void PassiveUpdate()
    {
        if (_aiState == EAIState.Wandering && _agent.remainingDistance < 0.1f)
        {
            SetState(EAIState.Idle);
            Invoke("WanderToNewLocation", Random.Range(MinWanderWaitTime, MaxWanderWaitTime));
        }

        if (_playerDistance < DetectDistance)
        {
            SetState(EAIState.Attacking);
        }
    }

    private void WanderToNewLocation()
    {
        // 새로운 목표 지점 정해서 이동
        if (_aiState != EAIState.Idle)
        {
            return;
        }

        SetState(EAIState.Wandering);
        _agent.SetDestination(GetWanderLocation());
    }

    private Vector3 GetWanderLocation()
    {
        NavMeshHit hit;

        NavMesh.SamplePosition(transform.position + (Random.onUnitSphere * Random.Range(MinWanderDistance, MaxWanderDistance)), out hit, MaxWanderDistance, NavMesh.AllAreas);

        int count = 0;

        while (Vector3.Distance(transform.position, hit.position) < DetectDistance)
        {
            NavMesh.SamplePosition(transform.position + (Random.onUnitSphere * Random.Range(MinWanderDistance, MaxWanderDistance)), out hit, MaxWanderDistance, NavMesh.AllAreas);
            ++count;

            if (count == 30)
            {
                break;
            }
        }

        return hit.position;
    }

    private void AttackingUpdate()
    {
        if (_playerDistance < AttackDistance && IsPlayerInFieldOfView())
        {
            _agent.isStopped = true;
            if (Time.time - _lastAttackTime > AttackRate)
            {
                _lastAttackTime = Time.time;
                CharacterManager.Instance.Player.PlayerController.GetComponent<IDamagable>().TakePhysicalDamage(Damage);
                _animator.speed = 1;
                _animator.SetTrigger("Attack");
            }
        }
        else
        {
            // 공격 범위에서 멀어졌지만 탐지 범위 안에 있을 때
            if (_playerDistance < DetectDistance)
            {
                // 플레이어 쫓아감
                _agent.isStopped = false;
                NavMeshPath path = new NavMeshPath();
                if (_agent.CalculatePath(CharacterManager.Instance.Player.transform.position, path))
                {
                    _agent.SetDestination(CharacterManager.Instance.Player.transform.position);
                }
                else
                {
                    _agent.SetDestination(transform.position);
                    _agent.isStopped = true;
                    SetState(EAIState.Wandering);
                }
            }
            else // 공격하다가 플레이어가 멀어졌을 때
            {
                _agent.SetDestination(transform.position);
                _agent.isStopped = true;
                SetState(EAIState.Wandering);
            }
        }
    }

    private bool IsPlayerInFieldOfView()
    {
        Vector3 directionToPlayer = CharacterManager.Instance.Player.transform.position - transform.position;
        float angle = Vector3.Angle(transform.forward, directionToPlayer);

        return angle < FieldOfView * 0.5f;
    }

    public void TakePhysicalDamage(int damage)
    {
        Health -= damage;

        if (Health <= 0)
        {
            Die();
        }

        // 데미지 효과
        StartCoroutine(DamageFlash());
    }

    public void Die()
    {
        // 죽은 후 아이템 드랍
        for(int i = 0; i < DropOnDeath.Length; i++)
        {
            Instantiate(DropOnDeath[i].DropPrefab, transform.position + Vector3.up * 2, Quaternion.identity);
        }

        Destroy(gameObject);
    }

    private IEnumerator DamageFlash()
    {
        for(int i = 0; i < _meshRenderers.Length; i++)
        {
            _meshRenderers[i].material.color = new Color(1.0f, 0.6f, 0.6f);
        }

        yield return new WaitForSeconds(0.1f);

        for(int i = 0; i < _meshRenderers.Length; i++)
        {
            _meshRenderers[i].material.color = Color.white;
        }
    }
}

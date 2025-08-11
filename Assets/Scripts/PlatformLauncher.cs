using System.Collections;
using UnityEngine;

public class PlatformLauncher : MonoBehaviour
{
    [Header("발사 설정")]
    [Tooltip("플레이어를 밀어내는 힘의 크기")]
    [SerializeField] private float _launchForce = 20f;

    [Tooltip("플레이어가 발판에 머물러야 하는 시간 (초)")]
    [SerializeField] private float _launchDelay = 2.0f;

    [Tooltip("발사 방향을 결정하는 자식 오브젝트의 Transform")]
    [SerializeField] private Transform _directionIndicator;

    // 현재 실행 중인 코루틴을 저장할 변수
    private Coroutine _launchCoroutine;

    // 플레이어가 충돌을 시작했을 때 호출
    private void OnCollisionEnter(Collision collision)
    {
        // 충돌한 것이 플레이어인지 확인
        if (collision.collider.CompareTag("Player"))
        {
            // 이미 발사가 진행 중이라면 중복 실행 방지
            if (_launchCoroutine != null)
            {
                return;
            }

            // 플레이어의 Rigidbody를 가져옴
            Rigidbody playerRigidbody = collision.gameObject.GetComponent<Rigidbody>();
            if (playerRigidbody != null)
            {
                // LaunchPlayer 코루틴을 시작하고 나중에 중지할 수 있도록 변수에 저장
                _launchCoroutine = StartCoroutine(LaunchPlayer(playerRigidbody));
            }
        }
    }

    // 플레이어가 충돌에서 벗어났을 때 호출
    private void OnCollisionExit(Collision collision)
    {
        // 충돌에서 벗어난 것이 플레이어인지 확인
        if (collision.collider.CompareTag("Player"))
        {
            // 발사 대기 중이었다면(코루틴이 실행 중이었다면) 발사 취소
            if (_launchCoroutine != null)
            {
                StopCoroutine(_launchCoroutine);
                _launchCoroutine = null; // 변수 초기화
            }
        }
    }

    // 시간 지연 후 플레이어를 발사하는 코루틴
    private IEnumerator LaunchPlayer(Rigidbody playerRigidbody)
    {
        // _launchDelay 만큼 대기
        yield return new WaitForSeconds(_launchDelay);

        PlayerController playerController = playerRigidbody.GetComponent<PlayerController>();
        if(playerController != null)
        {
            // _directionIndicator의 정면 방향을 발사 방향으로 사용
            Vector3 launchDirection = _directionIndicator.forward;

            playerController.Launch(launchDirection, _launchForce);
        }

        // 발사가 완료 -> 코루틴 변수 초기화
        _launchCoroutine = null;
    }

    private void OnDrawGizmos()
    {
        if (_directionIndicator != null)
        {
            // 기즈모의 색상 설정
            Gizmos.color = Color.red;

            // 방향 지시자의 위치에서 앞쪽 방향으로 레이를 그림
            Gizmos.DrawRay(_directionIndicator.position, _directionIndicator.forward * 5f);
        }
    }
}
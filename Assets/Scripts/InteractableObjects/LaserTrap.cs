using System.Collections;
using UnityEngine;

public class LaserTrap : MonoBehaviour
{
    [SerializeField] private float _distance = 1f;
    [SerializeField] private LayerMask _layerMask;
    [SerializeField][Range(1, 360)] private int _numberOfRays = 8; // 원형으로 발사할 레이저 개수
    [SerializeField] private float _raycastOffsetY = 0.5f; // 레이캐스트 시작 높이
    [SerializeField] private float _checkInterval = 0.1f; // 검사 주기
    [SerializeField] private WarningIndicator _warningIndicator;

    private bool _isPlayerBeingDetected = false;

    private void Start()
    {
        StartCoroutine(CheckPlayerRoutine());
    }

    private IEnumerator CheckPlayerRoutine()
    {
        while(true)
        {
            bool isDetectedThisCycle = false;

            Vector3 startPosition = transform.position + (transform.up * _raycastOffsetY);
            float angleStep = 360f / _numberOfRays;

            for (int i = 0; i < _numberOfRays; i++)
            {
                float currentAngle = angleStep * i;
                Vector3 direction = Quaternion.Euler(0, currentAngle, 0) * transform.forward;

                if (Physics.Raycast(startPosition, direction, _distance, _layerMask))
                {
                    isDetectedThisCycle = true;
                    break;
                }
            }

            // 이전 프레임과 현재 프레임의 감지 상태를 비교하여 경고를 시작하거나 멈춥니다.
            if (isDetectedThisCycle && !_isPlayerBeingDetected)
            {
                // 이번에 처음 감지되었다면: 경고 시작
                _warningIndicator.StartWarning();
            }
            else if (!isDetectedThisCycle && _isPlayerBeingDetected)
            {
                // 감지되다가 이제는 감지되지 않는다면: 경고 중지
                _warningIndicator.StopWarning();
            }

            // 현재 감지 상태 저장
            _isPlayerBeingDetected = isDetectedThisCycle;

            yield return new WaitForSeconds(_checkInterval);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        Vector3 startPosition = transform.position + (transform.up * _raycastOffsetY);
        float angleStep = 360f / _numberOfRays;

        for (int i = 0; i < _numberOfRays; i++)
        {
            float currentAngle = angleStep * i;
            Vector3 direction = Quaternion.Euler(0, currentAngle, 0) * transform.forward;

            // Gizmos로 레이를 그립니다.
            Gizmos.DrawRay(startPosition, direction * _distance);
        }
    }
}

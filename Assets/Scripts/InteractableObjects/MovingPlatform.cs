using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [SerializeField] private float _moveSpeed;
    [SerializeField] private Transform _endPosition;

    private Vector3 _startPosition;
    private Vector3 _targetWorldPosition;

    private Rigidbody _rigidbody;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();

        // 시작 위치의 월드 좌표 저장
        _startPosition = transform.position;

        if (_endPosition != null)
        {
            _targetWorldPosition = _endPosition.position;
        }
        else
        {
            Debug.LogError("종료 위치(_endPosition)가 할당되지 않았습니다!", this.gameObject);
        }
    }

    private void FixedUpdate()
    {
        MoveRoundTrip();
    }

    private void MoveRoundTrip()
    {
        // t라는 값이 0과 length 사이를 계속 왕복하게 만들어 준다.
        // 0과 1 사이를 왕복하는 값을 얻을 수 있는데 이 값을 비율로 사용하는 것
        float pingpongValue = Mathf.PingPong(Time.time * _moveSpeed, 1);

        // 발판이 있어야 할 현재 위치 계산
        // t 값이 0이면 a 위치를, 1이면 b 위치를, 0.5이면 a와 b의 중간 위치를 반환
        Vector3 moveVec = Vector3.Lerp(_startPosition, _targetWorldPosition, pingpongValue);

        _rigidbody.MovePosition(moveVec);
    }
}

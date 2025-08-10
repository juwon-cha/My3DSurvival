using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingPlatformHandler : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag("MovingPlatform"))
        {
            // 충돌 지점의 법선(normal) 벡터를 확인하여 플랫폼의 위에 착지했는지 검사
            // 법선 벡터는 충돌 표면에서 수직으로 뻗어 나오는 방향을 나타냄
            // 플레이어가 플랫폼 위에 제대로 섰다면, 이 벡터는 위쪽(Y축 양수)을 향해야 함
            // contact.normal.y > 0.9f 는 거의 평평한 윗면에 충돌했음을 의미한다
            foreach (ContactPoint contact in collision.contacts)
            {
                if (contact.normal.y > 0.9f)
                {
                    // 플랫폼 위에 성공적으로 착지했으므로, 플레이어를 플랫폼의 자식으로 만듦
                    // 이렇게 하면 플랫폼이 움직일 때 플레이어의 Transform도 함께 움직임
                    transform.SetParent(collision.transform);
                    return; // 자식으로 설정했으면 반복문을 빠져나온다
                }
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("MovingPlatform"))
        {
            transform.SetParent(null);
        }
    }
}

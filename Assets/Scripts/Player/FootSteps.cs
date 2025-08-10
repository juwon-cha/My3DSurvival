using UnityEngine;

public class FootSteps : MonoBehaviour
{
    public AudioClip[] FootStepClips;
    private AudioSource _audioSource;
    private Rigidbody _rigidbody;

    public float FootStepThreshold; // 움직이는지 아닌지 판단
    public float FootStepRate;
    private float _footStepTime;

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        // 장판 위라면 플레리어의 부모가 장판이 됨 -> 장판 위에서 소리가 안 나게 수정
        if(transform.parent != null)
        {
            return;
        }

        if(Mathf.Abs(_rigidbody.velocity.y) < 0.1f) // 땅에 붙어 있을 때
        {
            // 발자국 소리 재생
            // 플레이어가 움직이면 velocity.magnitude 변화량 증가 -> Threshold보다 커지면
            if (_rigidbody.velocity.magnitude > FootStepThreshold)
            {
                // 시간 측정
                if(Time.time - _footStepTime > FootStepRate)
                {
                    _footStepTime = Time.time;

                    // 발자국 소리 재생
                    _audioSource.PlayOneShot(FootStepClips[Random.Range(0, FootStepClips.Length)]);
                }
            }
        }
    }
}

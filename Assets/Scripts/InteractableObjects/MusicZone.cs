using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicZone : MonoBehaviour
{
    public AudioSource AudioSource;
    public float FadeTime;
    public float MaxVolume;
    private float _tragetVolume;

    private void Start()
    {
        _tragetVolume = 0;
        AudioSource = GetComponent<AudioSource>();
        AudioSource.volume = _tragetVolume;
        AudioSource.Play();
    }

    private void Update()
    {
        // 근사 값이 아닐 때
        if(!Mathf.Approximately(AudioSource.volume, _tragetVolume))
        {
            AudioSource.volume = Mathf.MoveTowards(AudioSource.volume, _tragetVolume, (MaxVolume / FadeTime) * Time.deltaTime);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            _tragetVolume = MaxVolume;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            _tragetVolume = 0;
        }
    }
}

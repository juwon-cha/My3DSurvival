using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class WarningIndicator : MonoBehaviour
{
    public Image Img;
    public float FlashSpeed;

    private Coroutine _flashingCoroutine;

    public void StartWarning()
    {
        // 이미 경고가 실행 중이면 아무것도 안함
        if (_flashingCoroutine != null)
        {
            return;
        }

        // UI 활성화
        gameObject.SetActive(true);
        Img.enabled = true;

        _flashingCoroutine = StartCoroutine(FlashRoutine());
    }

    public void StopWarning()
    {
        // 실행 중인 경고 코루틴이 있으면 중지
        if (_flashingCoroutine != null)
        {
            StopCoroutine(_flashingCoroutine);
            _flashingCoroutine = null;
        }

        // UI 비활성화
        gameObject.SetActive(false);
        Img.enabled = false;
    }

    private IEnumerator FlashRoutine()
    {
        // 경고가 멈추라는 명령을 받기 전까지 무한 반복
        while (true)
        {
            // 밝아졌다가
            yield return Fade(0f, 0.3f, FlashSpeed);
            // 어두워졌다가
            yield return Fade(0.3f, 0f, FlashSpeed);
        }
    }

    // 알파값을 부드럽게 변경하는 페이드 코루틴
    private IEnumerator Fade(float startAlpha, float endAlpha, float duration)
    {
        float timer = 0f;
        Color color = Img.color;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float progress = timer / duration;
            color.a = Mathf.Lerp(startAlpha, endAlpha, progress);
            Img.color = color;
            yield return null;
        }

        color.a = endAlpha;
        Img.color = color;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DamageIndicator : MonoBehaviour
{
    public Image Img;
    public float FlashSpeed;

    private Coroutine coroutine;

    private void Start()
    {
        CharacterManager.Instance.Player.PlayerCondition.OnTakeDamage += Flash;
    }

    public void Flash()
    {
        if(coroutine != null)
        {
            StopCoroutine(coroutine);
        }

        Img.enabled = true;
        Img.color = new Color(1f, 100f / 255f, 100f / 255f);
        coroutine = StartCoroutine(FadeAway());
    }

    private IEnumerator FadeAway()
    {
        float startApha = 0.3f;
        float alpha = startApha;

        while(alpha > 0)
        {
            alpha -= (startApha / FlashSpeed) * Time.deltaTime;
            Img.color = new Color(1f, 100f / 255f, 100f / 255f, alpha);
            yield return null;
        }

        Img.enabled = false;
    }
}

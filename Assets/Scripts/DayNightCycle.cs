using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    [Range(0.0f, 1.0f)]
    public float CurTime;
    public float FullDayLength;
    public float StartTime = 0.4f;
    private float timeRate;
    public Vector3 Noon; // 정오 Vector 90, 0, 0

    [Header("Sun")]
    public Light Sun;
    public Gradient SunColor;
    public AnimationCurve SunIntensity;

    [Header("Moon")]
    public Light Moon;
    public Gradient MoonColor;
    public AnimationCurve MoonIntensity;

    [Header("Other Lighting")]
    public AnimationCurve LightingIntensityMultiplier;
    public AnimationCurve ReflectionIntensityMultiplier;

    private void Start()
    {
        timeRate = 1.0f / FullDayLength; // 하루 길이 설정
        CurTime = StartTime; // 하루 길이의 40프로로 초기화
    }

    private void Update()
    {
        CurTime = (CurTime + timeRate * Time.deltaTime) % 1.0f;

        UpdateLight(Sun, SunColor, SunIntensity);
        UpdateLight(Moon, MoonColor, MoonIntensity);

        RenderSettings.ambientIntensity = LightingIntensityMultiplier.Evaluate(CurTime);
        RenderSettings.reflectionIntensity = ReflectionIntensityMultiplier.Evaluate(CurTime);
    }

    private void UpdateLight(Light lightSource, Gradient gradient, AnimationCurve intensityCurve)
    {
        float intensity = intensityCurve.Evaluate(CurTime);

        lightSource.transform.eulerAngles = (CurTime - (lightSource == Sun ? 0.25f : 0.75f)) * Noon * 4f;
        lightSource.color = gradient.Evaluate(CurTime);
        lightSource.intensity = intensity;

        GameObject go = lightSource.gameObject;
        if(lightSource.intensity == 0 && go.activeInHierarchy)
        {
            go.SetActive(false);
        }
        else if(lightSource.intensity > 0 && !go.activeInHierarchy)
        {
            go.SetActive(true);
        }
    }
}

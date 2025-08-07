using UnityEngine;
using UnityEngine.UI;

public class Condition : MonoBehaviour
{
    public float CurValue;
    public float StartValue;
    public float MaxValue;
    public float PassiveValue;
    public Image UIBar;

    private void Start()
    {
        CurValue = StartValue;
    }

    private void Update()
    {
        UIBar.fillAmount = GetPercentage();
    }

    private float GetPercentage()
    {
        return CurValue / MaxValue;
    }

    public void Add(float value)
    {
        CurValue = Mathf.Min(CurValue + value, MaxValue);
    }

    public void Subtract(float value)
    {
        CurValue = Mathf.Max(CurValue - value, 0);
    }
}

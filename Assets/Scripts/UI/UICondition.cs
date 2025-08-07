using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UICondition : MonoBehaviour
{
    public Condition Health;
    public Condition Hunger;
    public Condition Stamina;

    void Start()
    {
        CharacterManager.Instance.Player.PlayerCondition.UICondition = this;        
    }
}

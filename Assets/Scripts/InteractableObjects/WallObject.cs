using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallObject : MonoBehaviour, IInteractable
{
    public string GetInteractPrompt()
    {
        return $"[W] 키를 눌러 벽 타기";
    }

    public void OnInteract() { }
}

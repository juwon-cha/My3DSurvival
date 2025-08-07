using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInteractable
{
    public string GetInteractPrompt();
    public void OnInteract();
}

public class ItemObject : MonoBehaviour, IInteractable
{
    public ItemData ItemData;

    public string GetInteractPrompt()
    {
        string str = $"{ItemData.DisplayName}\n{ItemData.Description}";

        return str;
    }

    public void OnInteract()
    {
        CharacterManager.Instance.Player.ItemData = ItemData;
        CharacterManager.Instance.Player.AddItem?.Invoke();
        Destroy(gameObject); // 맵에서 삭제
    }
}

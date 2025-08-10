using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class Interaction : MonoBehaviour
{
    public float CheackRate = 0.05f;
    public float MaxCheckDistance;
    public LayerMask InteractLayer;
    private float _lastCheckTime;

    public GameObject CurInteractGameObject;
    private IInteractable _curInteractable;

    public TextMeshProUGUI PromptText;
    private Camera _camera;

    private void Start()
    {
        _camera = Camera.main;
    }

    private void Update()
    {
        if(Time.time - _lastCheckTime > CheackRate)
        {
            _lastCheckTime = Time.time;

            // 화면 정중앙
            Ray ray = _camera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
            RaycastHit hit;

            Debug.DrawRay(ray.origin, ray.direction * MaxCheckDistance, Color.red, CheackRate);

            if (Physics.Raycast(ray, out hit, MaxCheckDistance, InteractLayer))
            {
                // 이미 상호작용하는 오브젝트가 없을 때
                if (hit.collider.gameObject != CurInteractGameObject)
                {
                    // 현재 상호작용 오브젝트에 할당
                    CurInteractGameObject = hit.collider.gameObject;
                    _curInteractable = hit.collider.GetComponent<IInteractable>();

                    // 프롬프트에 출력
                    SetPromptText();
                }
            }
            else
            {
                CurInteractGameObject = null;
                _curInteractable = null;
                PromptText.gameObject.SetActive(false);
            }
        }
    }

    private void SetPromptText()
    {
        PromptText.gameObject.SetActive(true);
        PromptText.text = _curInteractable.GetInteractPrompt();
    }

    public void OnInteractInput(InputAction.CallbackContext context)
    {
        if(context.phase == InputActionPhase.Started && _curInteractable != null)
        {
            _curInteractable.OnInteract();
            CurInteractGameObject = null;
            _curInteractable = null;
            PromptText.gameObject.SetActive(false);
        }
    }
}

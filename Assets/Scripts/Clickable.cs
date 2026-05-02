using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class Clickable : MonoBehaviour
{
    public Action OnClick;

    void OnMouseDown()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;
        OnClick?.Invoke();
    }
}

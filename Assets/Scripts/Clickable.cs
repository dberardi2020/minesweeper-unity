using System;
using UnityEngine;

public class Clickable : MonoBehaviour
{
    public Action OnClick;
    void OnMouseDown() => OnClick?.Invoke();
}

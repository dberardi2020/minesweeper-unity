using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class CellView : MonoBehaviour
{
    public int Row;
    public int Col;

    public SpriteRenderer Background;
    public TextMeshPro Label;

    public Action<int, int> OnLeftClick;
    public Action<int, int> OnRightClick;

    static readonly Color ColorCovered  = new Color32(170, 170, 170, 255);
    static readonly Color ColorRevealed = new Color32( 85,  85,  85, 255);
    static readonly Color ColorMineHit  = new Color32(255,   0,   0, 255);

    static readonly Color[] NumberColors =
    {
        Color.clear,                          // 0 — unused
        new Color32(  0,   0, 255, 255),      // 1 blue
        new Color32(  0, 123,   0, 255),      // 2 green
        new Color32(255,   0,   0, 255),      // 3 red
        new Color32(  0,   0, 123, 255),      // 4 dark blue
        new Color32(123,   0,   0, 255),      // 5 dark red
        new Color32(  0, 123, 123, 255),      // 6 teal
        new Color32(  0,   0,   0, 255),      // 7 black
        new Color32(123, 123, 123, 255),      // 8 gray
    };

    void Awake()
    {
        Label.GetComponent<Renderer>().sortingOrder = 1;
    }

    void OnMouseDown()
    {
        OnLeftClick?.Invoke(Row, Col);
    }

    void OnMouseOver()
    {
        if (Mouse.current.rightButton.wasPressedThisFrame)
            OnRightClick?.Invoke(Row, Col);
    }

    public void Refresh(Cell cell, bool revealAll)
    {
        if (revealAll && cell.isMine && !cell.isRevealed)
            Set(ColorRevealed, "●", Color.white);
        else if (revealAll && cell.isFlagged && !cell.isMine)
            Set(ColorRevealed, "F✕", Color.white);
        else if (cell.isRevealed && cell.isMine)
            Set(ColorMineHit, "●", Color.white);
        else if (cell.isRevealed && cell.adjacentMines > 0)
            Set(ColorRevealed, cell.adjacentMines.ToString(), NumberColors[cell.adjacentMines]);
        else if (cell.isRevealed)
            Set(ColorRevealed, "", Color.white);
        else if (cell.isFlagged)
            Set(ColorCovered, "F", Color.white);
        else
            Set(ColorCovered, "", Color.white);
    }

    void Set(Color bg, string text, Color textColor)
    {
        Background.color = bg;
        Label.text = text;
        Label.color = textColor;
    }
}

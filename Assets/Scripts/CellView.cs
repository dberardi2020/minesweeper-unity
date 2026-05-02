using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class CellView : MonoBehaviour
{
    public int Row;
    public int Col;

    [Header("Child References")]
    public SpriteRenderer Background;
    public SpriteRenderer Icon;
    public TextMeshPro Label;

    [Header("Background Sprites")]
    public Sprite SpriteCovered;
    public Sprite SpriteRevealed1;
    public Sprite SpriteRevealed2;
    public Sprite SpriteRevealed3;
    public Sprite SpriteMineHit;

    [Header("Icon Sprites")]
    public Sprite SpriteFlag;
    public Sprite SpriteRabbit;
    public Sprite SpriteNumber1;
    public Sprite SpriteNumber2;
    public Sprite SpriteNumber3;

    [Header("Flower Sprites")]
    public Sprite[] SpriteFlowers;

    public Action<int, int> OnLeftClick;
    public Action<int, int> OnRightClick;

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
        Icon.sortingOrder = 1;
        Label.GetComponent<Renderer>().sortingOrder = 2;
    }

    void OnMouseDown() => OnLeftClick?.Invoke(Row, Col);

    void OnMouseOver()
    {
        if (Mouse.current.rightButton.wasPressedThisFrame)
            OnRightClick?.Invoke(Row, Col);
    }

    public void Refresh(Cell cell, bool revealAll)
    {
        if (revealAll && cell.isMine && !cell.isRevealed)
            Set(RevealedVariant(), SpriteRabbit, "", Color.white);
        else if (revealAll && cell.isFlagged && !cell.isMine)
            Set(SpriteCovered, SpriteFlag, "✕", Color.red);
        else if (cell.isRevealed && cell.isMine)
            Set(SpriteMineHit, SpriteRabbit, "", Color.white);
        else if (cell.isRevealed && cell.adjacentMines > 0)
            SetNumber(cell.adjacentMines);
        else if (cell.isRevealed)
            Set(RevealedVariant(), FlowerVariant(), "", Color.white);
        else if (cell.isFlagged)
            Set(SpriteCovered, SpriteFlag, "", Color.white);
        else
            Set(SpriteCovered, null, "", Color.white);
    }

    public void HideIcon()
    {
        Icon.enabled = false;
    }

    public void ShowFlower()
    {
        Sprite flower = FlowerVariant();
        if (flower == null) return;
        Icon.sprite = flower;
        Icon.enabled = true;
    }

    Sprite FlowerVariant()
    {
        if (SpriteFlowers == null || SpriteFlowers.Length == 0) return null;
        return SpriteFlowers[Mathf.Abs(Row * 7 + Col * 13) % SpriteFlowers.Length];
    }

    Sprite RevealedVariant()
    {
        return ((Row * 9 + Col) % 3) switch
        {
            0 => SpriteRevealed1,
            1 => SpriteRevealed2,
            _ => SpriteRevealed3,
        };
    }

    void SetNumber(int n)
    {
        Sprite numSprite = n switch { 1 => SpriteNumber1, 2 => SpriteNumber2, 3 => SpriteNumber3, _ => null };
        if (numSprite != null)
            Set(RevealedVariant(), numSprite, "", Color.white);
        else
            Set(RevealedVariant(), null, n.ToString(), NumberColors[n]);
    }

    void Set(Sprite bg, Sprite icon, string text, Color textColor)
    {
        Background.sprite = bg;
        Background.color = Color.white;
        Icon.sprite = icon;
        Icon.enabled = icon != null;
        Label.text = text;
        Label.color = textColor;
    }
}

using UnityEngine;

public class DigitDisplay : MonoBehaviour
{
    // Sprite indices: 0-9 = digits, 10 = minus sign
    public Sprite[] Sprites;

    SpriteRenderer[] _renderers;

    const float DigitW = 10f / 16f; // 10px wide at 16 PPU
    const float Pad    =  1f / 16f; // 1px side padding

    void Awake()
    {
        _renderers = new SpriteRenderer[3];
        for (int i = 0; i < 3; i++)
        {
            var go = new GameObject($"d{i}");
            go.transform.SetParent(transform, false);
            go.transform.localPosition = new Vector3(Pad + DigitW * i + DigitW * 0.5f, 0f, 0f);
            _renderers[i] = go.AddComponent<SpriteRenderer>();
            _renderers[i].sortingOrder = 2;
        }
    }

    public void Show(int value)
    {
        int clamped = Mathf.Clamp(value, -99, 999);
        bool negative = clamped < 0;
        int abs = Mathf.Abs(clamped);
        _renderers[0].sprite = negative ? Sprites[10] : Sprites[abs / 100];
        _renderers[1].sprite = Sprites[(abs / 10) % 10];
        _renderers[2].sprite = Sprites[abs % 10];
    }
}

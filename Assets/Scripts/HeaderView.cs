using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class HeaderView : MonoBehaviour
{
    [Header("TMP")]
    public TextMeshPro MineCounter;
    public TextMeshPro Timer;
    public TextMeshPro ResetLabel;
    public Clickable ResetButton;

    [Header("Sprite Header")]
    public Sprite HeaderMiddle;
    public Sprite[] DigitSprites; // 11 sprites: indices 0-9 = digits, 10 = minus

    public Action OnResetClick;

    // TMP sprite tags referencing the EmojiOne atlas (only these faces ship with TMP).
    // 😮 and 😵 are not in the atlas — replaced with closest available equivalents.
    const string FaceNormal = "<sprite name=\"263a\">";   // ☺
    const string FaceHeld   = "<sprite name=\"1f605\">";  // 😅 (closest to 😮)
    const string FaceWon    = "<sprite name=\"1f60e\">";  // 😎
    const string FaceLost   = "<sprite name=\"2639\">";   // ☹ (closest to 😵)

    GameState _state = GameState.Ready;
    GameObject _bgContainer;
    DigitDisplay _mineDisplay;
    DigitDisplay _timerDisplay;

    void Awake()
    {
        ResetButton.OnClick += () => OnResetClick?.Invoke();
    }

    void Update()
    {
        if (_state == GameState.Playing)
            ResetLabel.text = Mouse.current.leftButton.isPressed ? FaceHeld : FaceNormal;
    }

    public void Build(int cols)
    {
        if (_bgContainer  != null) Destroy(_bgContainer);
        if (_mineDisplay  != null) Destroy(_mineDisplay.gameObject);
        if (_timerDisplay != null) Destroy(_timerDisplay.gameObject);

        if (MineCounter != null) MineCounter.gameObject.SetActive(false);
        if (Timer       != null) Timer.gameObject.SetActive(false);

        float halfSpan = (cols - 1) / 2f;

        _bgContainer = new GameObject("Header_Bg");
        _bgContainer.transform.SetParent(transform, false);
        for (int c = 0; c < cols; c++)
        {
            var tile = new GameObject($"Tile_{c}");
            tile.transform.SetParent(_bgContainer.transform, false);
            tile.transform.localPosition = new Vector3(c - halfSpan, 0f, 0f);
            var sr = tile.AddComponent<SpriteRenderer>();
            sr.sprite = HeaderMiddle;
            sr.sortingOrder = 0;
        }

        // Mine counter — left side (will shift right by 1 tile when endcap arrives)
        var mineGo = new GameObject("MineDisplay");
        mineGo.transform.SetParent(transform, false);
        mineGo.transform.localPosition = new Vector3(-halfSpan, 0f, 0f);
        _mineDisplay = mineGo.AddComponent<DigitDisplay>();
        _mineDisplay.Sprites = DigitSprites;

        // Timer — right side (will shift left by 1 tile when endcap arrives)
        var timerGo = new GameObject("TimerDisplay");
        timerGo.transform.SetParent(transform, false);
        timerGo.transform.localPosition = new Vector3((cols - 3) / 2f, 0f, 0f);
        _timerDisplay = timerGo.AddComponent<DigitDisplay>();
        _timerDisplay.Sprites = DigitSprites;
    }

    public void Refresh(GameState state, int minesRemaining, int seconds)
    {
        _state = state;

        if (_mineDisplay != null)
        {
            _mineDisplay.Show(minesRemaining);
            _timerDisplay.Show(Mathf.Min(seconds, 999));
        }
        else
        {
            MineCounter.text = minesRemaining >= 0
                ? minesRemaining.ToString("D3")
                : minesRemaining.ToString();
            Timer.text = Mathf.Min(seconds, 999).ToString("D3");
        }

        if (state != GameState.Playing)
        {
            ResetLabel.text = state switch
            {
                GameState.Won  => FaceWon,
                GameState.Lost => FaceLost,
                _              => FaceNormal
            };
        }
    }
}

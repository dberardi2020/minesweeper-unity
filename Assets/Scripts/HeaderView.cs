using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class HeaderView : MonoBehaviour
{
    public TextMeshPro MineCounter;
    public TextMeshPro ResetLabel;
    public TextMeshPro Timer;
    public Clickable ResetButton;

    public Action OnResetClick;

    // TMP sprite tags referencing the EmojiOne atlas (only these faces ship with TMP).
    // 😮 and 😵 are not in the atlas — replaced with closest available equivalents.
    const string FaceNormal = "<sprite name=\"263a\">";   // ☺
    const string FaceHeld   = "<sprite name=\"1f605\">";  // 😅 (closest to 😮)
    const string FaceWon    = "<sprite name=\"1f60e\">";  // 😎
    const string FaceLost   = "<sprite name=\"2639\">";   // ☹ (closest to 😵)

    GameState _state = GameState.Ready;

    void Awake()
    {
        ResetButton.OnClick += () => OnResetClick?.Invoke();
    }

    void Update()
    {
        // Per-frame face update during play — shows held face while mouse is pressed
        if (_state == GameState.Playing)
            ResetLabel.text = Mouse.current.leftButton.isPressed ? FaceHeld : FaceNormal;
    }

    public void Refresh(GameState state, int minesRemaining, int seconds)
    {
        _state = state;

        MineCounter.text = minesRemaining >= 0
            ? minesRemaining.ToString("D3")
            : minesRemaining.ToString();

        Timer.text = Mathf.Min(seconds, 999).ToString("D3");

        // Update() owns the face label during play (handles held-face per-frame).
        // We only set it here when the state is not Playing.
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

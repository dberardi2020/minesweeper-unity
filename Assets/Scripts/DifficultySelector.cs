using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DifficultySelector : MonoBehaviour
{
    GameManager _gm;

    void Awake()
    {
        _gm = FindAnyObjectByType<GameManager>();
        BuildUI();
    }

    void BuildUI()
    {
        if (FindAnyObjectByType<EventSystem>() == null)
        {
            new GameObject("EventSystem")
                .AddComponent<EventSystem>()
                .gameObject.AddComponent<StandaloneInputModule>();
        }

        var root = new GameObject("DifficultySelector");
        var canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
        root.AddComponent<CanvasScaler>();
        root.AddComponent<GraphicRaycaster>();

        string[] labels = { "BEGINNER", "INTERMEDIATE", "EXPERT", "QUIT" };
        float btnWidth  = 130f;
        float btnHeight = 30f;
        float gap       = 6f;
        float marginX   = 8f;
        float marginY   = 8f;

        for (int i = 0; i < labels.Length; i++)
        {
            int idx = i;
            bool isQuit = i == labels.Length - 1;

            var btnGo = new GameObject(labels[i]);
            btnGo.transform.SetParent(root.transform, false);

            var rect = btnGo.AddComponent<RectTransform>();
            rect.anchorMin        = new Vector2(1f, 1f);
            rect.anchorMax        = new Vector2(1f, 1f);
            rect.pivot            = new Vector2(1f, 1f);
            rect.sizeDelta        = new Vector2(btnWidth, btnHeight);
            rect.anchoredPosition = new Vector2(-marginX, -(marginY + i * (btnHeight + gap)));

            var img = btnGo.AddComponent<Image>();
            img.color = isQuit
                ? new Color(0.25f, 0.08f, 0.08f, 0.9f)
                : new Color(0.1f, 0.1f, 0.1f, 0.85f);

            var btn = btnGo.AddComponent<Button>();
            var colors = btn.colors;
            colors.highlightedColor = isQuit
                ? new Color(0.45f, 0.12f, 0.12f, 1f)
                : new Color(0.3f, 0.3f, 0.3f, 1f);
            btn.colors = colors;

            if (isQuit)
                btn.onClick.AddListener(Quit);
            else
                btn.onClick.AddListener(() => _gm.SetDifficulty(
                    idx == 0 ? Difficulty.Beginner :
                    idx == 1 ? Difficulty.Intermediate :
                               Difficulty.Expert));

            var textGo = new GameObject("Label");
            textGo.transform.SetParent(btnGo.transform, false);
            var textRect = textGo.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            var tmp = textGo.AddComponent<TextMeshProUGUI>();
            tmp.text      = labels[i];
            tmp.fontSize  = 13;
            tmp.color     = Color.white;
            tmp.alignment = TextAlignmentOptions.Center;
        }
    }

    static void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}

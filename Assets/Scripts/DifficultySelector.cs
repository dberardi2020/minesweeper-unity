using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DifficultySelector : MonoBehaviour
{
    GameManager _gm;

    void Awake()
    {
        _gm = FindFirstObjectByType<GameManager>();
        BuildUI();
    }

    void BuildUI()
    {
        if (FindFirstObjectByType<EventSystem>() == null)
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

        string[] labels = { "BEGINNER", "INTERMEDIATE", "EXPERT" };
        Difficulty[] diffs = { Difficulty.Beginner, Difficulty.Intermediate, Difficulty.Expert };

        for (int i = 0; i < 3; i++)
        {
            int idx = i;
            var btnGo = new GameObject(labels[i]);
            btnGo.transform.SetParent(root.transform, false);

            var rect = btnGo.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(i / 3f, 1f);
            rect.anchorMax = new Vector2((i + 1) / 3f, 1f);
            rect.pivot     = new Vector2(0.5f, 1f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(-10f, 32f);

            var img = btnGo.AddComponent<Image>();
            img.color = new Color(0.1f, 0.1f, 0.1f, 0.85f);

            var btn = btnGo.AddComponent<Button>();
            var colors = btn.colors;
            colors.highlightedColor = new Color(0.3f, 0.3f, 0.3f, 1f);
            btn.colors = colors;
            btn.onClick.AddListener(() => _gm.SetDifficulty(diffs[idx]));

            var textGo = new GameObject("Label");
            textGo.transform.SetParent(btnGo.transform, false);
            var textRect = textGo.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            var tmp = textGo.AddComponent<TextMeshProUGUI>();
            tmp.text      = labels[i];
            tmp.fontSize  = 14;
            tmp.color     = Color.white;
            tmp.alignment = TextAlignmentOptions.Center;
        }
    }
}

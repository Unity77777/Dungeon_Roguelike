using UnityEngine;
using TMPro;

public class TooltipManager : MonoBehaviour
{
    public RectTransform tooltipPanel;
    public TextMeshProUGUI tooltipText;
    public Canvas canvas;

    public Vector2 fixedPosition = new Vector2(300, -50);

    private static TooltipManager instance;

    private void Awake()
    {
        instance = this;

        CanvasGroup cg = tooltipPanel.GetComponent<CanvasGroup>();
        if (cg == null) cg = tooltipPanel.gameObject.AddComponent<CanvasGroup>();
        cg.interactable = false;
        cg.blocksRaycasts = false;

        tooltipPanel.gameObject.SetActive(false);
    }

    public static void Show(string content)
    {
        instance.tooltipText.text = content;
        instance.tooltipPanel.gameObject.SetActive(true);
        instance.tooltipPanel.anchoredPosition = instance.fixedPosition;
    }

    public static void Hide()
    {
        instance.tooltipPanel.gameObject.SetActive(false);
    }
}
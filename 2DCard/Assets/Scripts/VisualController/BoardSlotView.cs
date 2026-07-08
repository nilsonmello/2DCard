using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BoardSlotView : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private TMP_Text label;
    [SerializeField] private Image background;

    [Header("Cores")]
    [SerializeField] private Color defaultColor = Color.white;
    [SerializeField] private Color placementColor = new Color(0.6f, 1f, 0.6f);
    [SerializeField] private Color selectedAttackerColor = Color.yellow;
    [SerializeField] private Color attackableColor = new Color(1f, 0.5f, 0.5f);
    [SerializeField] private Color artifactTargetColor = new Color(1f, 0.6f, 1f);
    [SerializeField] private Color usedColor = Color.gray;

    private System.Action onClick;

    public void Setup(System.Action onClickCallback)
    {
        onClick = onClickCallback;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onClick?.Invoke());
    }

    public void SetLabel(string text) => label.text = text;

    public void SetInteractable(bool interactable) => button.interactable = interactable;

    public void SetVisualState(SlotVisualState state)
    {
        background.color = state switch
        {
            SlotVisualState.Placement => placementColor,
            SlotVisualState.SelectedAttacker => selectedAttackerColor,
            SlotVisualState.Attackable => attackableColor,
            SlotVisualState.ArtifactTarget => artifactTargetColor,
            SlotVisualState.Used => usedColor,
            _ => defaultColor
        };
    }
}

public enum SlotVisualState
{
    Default,
    Placement,
    SelectedAttacker,
    Attackable,
    ArtifactTarget,
    Used
}
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ArtifactButtonView : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private TMP_Text label;
    [SerializeField] private Image background;
    [SerializeField] private Color defaultColor = new Color(0.7f, 0.9f, 1f);
    [SerializeField] private Color pendingColor = Color.yellow;

    public void Setup(ArtifactData artifact, bool isPending, System.Action<ArtifactData> onClickCallback)
    {
        label.text = $"{artifact.artifactName}\n{artifact.description}";
        background.color = isPending ? pendingColor : defaultColor;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onClickCallback?.Invoke(artifact));
    }
}
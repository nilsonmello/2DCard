using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HandCardView : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private TMP_Text label;
    [SerializeField] private Image background;
    [SerializeField] private Color defaultColor = Color.white;
    [SerializeField] private Color selectedColor = Color.yellow;

    private CardData card;

    public void Setup(CardData cardData, bool isSelected, System.Action<CardData> onClickCallback)
    {
        card = cardData;
        label.text = cardData.isWildcard ? "Coringa" : $"Carta [{cardData.value}]";
        background.color = isSelected ? selectedColor : defaultColor;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onClickCallback?.Invoke(card));
    }
}
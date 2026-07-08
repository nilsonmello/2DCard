using UnityEngine;

[CreateAssetMenu(fileName = "NewCard", menuName = "CardGame/Card")]
public class CardData : ScriptableObject
{
    public string cardName = "Carta";
    public int value = 1;
    public bool isWildcard = false;
}

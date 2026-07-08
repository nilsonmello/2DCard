using UnityEngine;

public static class CardFactory
{
    public static CardData CreateRandomCard(float wildcardChance = 0.15f)
    {
        bool isWildcard = Random.value < wildcardChance;
        int value = Random.Range(1, 11);

        CardData card = ScriptableObject.CreateInstance<CardData>();
        card.cardName = isWildcard ? "Coringa" : $"Carta {value}";
        card.value = value;
        card.isWildcard = isWildcard;
        return card;
    }
}

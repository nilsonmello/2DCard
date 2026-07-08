public class CardInstance
{
    public CardData data;
    public int EffectiveValue { get; private set; }

    public bool revealedToOpponent = false;

    public bool hasAttackedThisRound = false;

    public CardInstance(CardData data, int chosenWildcardValue = -1)
    {
        this.data = data;

        if (data.isWildcard)
        {
            if (chosenWildcardValue < 0)
            {
                UnityEngine.Debug.LogWarning($"Coringa '{data.cardName}' foi criado sem valor escolhido. Usando valor base.");
                EffectiveValue = data.value;
            }
            else
            {
                EffectiveValue = chosenWildcardValue;
            }
        }
        else
        {
            EffectiveValue = data.value;
        }
    }

    public override string ToString()
    {
        return $"{data.cardName} (valor {EffectiveValue})";
    }

    public void ApplyBonus(int amount)
    {
        EffectiveValue += amount;
    }
}
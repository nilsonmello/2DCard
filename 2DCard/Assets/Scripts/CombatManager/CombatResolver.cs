using UnityEngine;

public enum CombatResult
{
    AttackerWins,
    DefenderWins,
    Tie
}

public static class CombatResolver
{
    public static CombatResult ResolveAttack(
        BoardSlot attackerSlot, Board attackerBoard,
        BoardSlot defenderSlot, Board defenderBoard)
    {
        if (attackerSlot.IsEmpty || defenderSlot.IsEmpty)
        {
            Debug.LogWarning("Tentativa de combate com slot vazio.");
            return CombatResult.Tie;
        }

        int attackerValue = attackerSlot.occupiedCard.EffectiveValue;
        int defenderValue = defenderSlot.occupiedCard.EffectiveValue;

        Debug.Log($"[Combate] {attackerBoard.ownerName}'s {attackerSlot.occupiedCard} ataca " +
                  $"{defenderBoard.ownerName}'s {defenderSlot.occupiedCard}");

        CombatResult result;

        if (attackerValue > defenderValue)
        {
            result = CombatResult.AttackerWins;
        }
        else if (defenderValue > attackerValue)
        {
            result = CombatResult.DefenderWins;
        }
        else
        {
            result = CombatResult.Tie;
        }

        if (result == CombatResult.AttackerWins && defenderBoard.hasMirrorShield)
        {
            defenderBoard.hasMirrorShield = false;
            Debug.Log($"[Combate] Espelho de {defenderBoard.ownerName} ativado! Resultado convertido em empate.");
            result = CombatResult.Tie;
        }

        switch (result)
        {
            case CombatResult.AttackerWins:
                defenderBoard.RemoveCard(defenderSlot.row, defenderSlot.columnIndex);
                attackerSlot.occupiedCard.revealedToOpponent = true;
                Debug.Log($"[Combate] {attackerBoard.ownerName} venceu. Carta defensora removida.");
                break;

            case CombatResult.DefenderWins:
                attackerBoard.RemoveCard(attackerSlot.row, attackerSlot.columnIndex);
                defenderSlot.occupiedCard.revealedToOpponent = true;
                Debug.Log($"[Combate] {defenderBoard.ownerName} venceu. Carta atacante removida.");
                break;

            case CombatResult.Tie:
                attackerBoard.RemoveCard(attackerSlot.row, attackerSlot.columnIndex);
                defenderBoard.RemoveCard(defenderSlot.row, defenderSlot.columnIndex);
                Debug.Log("[Combate] Empate. Ambas as cartas removidas.");
                break;
        }

        return result;
    }

    public static void AttackKing(BoardSlot attackerSlot, Board attackerBoard, Board defenderBoard)
    {
        if (attackerSlot.IsEmpty)
        {
            Debug.LogWarning("Tentativa de atacar o Rei sem uma carta atacante.");
            return;
        }

        Debug.Log($"[Combate] {attackerBoard.ownerName}'s {attackerSlot.occupiedCard} ataca o Rei de " +
                  $"{defenderBoard.ownerName} diretamente pela coluna exposta!");

        attackerSlot.occupiedCard.revealedToOpponent = true;
        defenderBoard.isKingAlive = false;
    }
}
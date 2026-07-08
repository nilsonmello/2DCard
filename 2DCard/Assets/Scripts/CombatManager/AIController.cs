using System.Collections.Generic;
using UnityEngine;

public static class AIController
{
    private const int MinCardValue = 1;
    private const int MaxCardValue = 10;

    public static void PlaceCards(Board board, List<CardData> hand)
    {
        List<BoardSlot> emptySlots = board.GetEmptySlots();
        int cardsToPlace = Mathf.Min(hand.Count, emptySlots.Count);

        for (int i = 0; i < cardsToPlace; i++)
        {
            CardData card = hand[0];
            hand.RemoveAt(0);

            BoardSlot slot = emptySlots[i];
            int wildcardValue = card.isWildcard ? Random.Range(4, 11) : -1;
            CardInstance instance = new CardInstance(card, wildcardValue);

            board.PlaceCard(slot.row, slot.columnIndex, instance);
        }
    }

    public static bool TryOneAttack(Board aiBoard, Board enemyBoard)
    {
        if (enemyBoard.isKingAlive && enemyBoard.IsKingExposed())
        {
            BoardSlot kingAttacker = FindStrongestAvailableAttacker(aiBoard);
            if (kingAttacker != null)
            {
                kingAttacker.occupiedCard.hasAttackedThisRound = true;
                CombatResolver.AttackKing(kingAttacker, aiBoard, enemyBoard);
                return true;
            }
        }

        List<BoardSlot> targets = enemyBoard.GetAllAttackableSlots();
        Shuffle(targets);

        foreach (BoardSlot target in targets)
        {
            BoardSlot attacker = ChooseAttackerFor(aiBoard, target);
            if (attacker != null)
            {
                attacker.occupiedCard.hasAttackedThisRound = true;
                CombatResolver.ResolveAttack(attacker, aiBoard, target, enemyBoard);
                return true;
            }
        }

        return false;
    }

    private static BoardSlot ChooseAttackerFor(Board aiBoard, BoardSlot target)
    {
        bool targetKnown = target.occupiedCard.revealedToOpponent;

        if (targetKnown)
        {
            return FindCheapestWinningAttacker(aiBoard, target.occupiedCard.EffectiveValue);
        }

        BoardSlot strongest = FindStrongestAvailableAttacker(aiBoard);
        if (strongest == null) return null;

        float estimatedWinChance = Mathf.Clamp01(
            (float)(strongest.occupiedCard.EffectiveValue - MinCardValue) / (MaxCardValue - MinCardValue));

        float riskAppetite = Random.Range(0.6f, 1.0f);

        return (Random.value < estimatedWinChance * riskAppetite) ? strongest : null;
    }

    private static BoardSlot FindCheapestWinningAttacker(Board board, int targetValue)
    {
        BoardSlot best = null;
        int bestValue = int.MaxValue;

        foreach (SlotRow row in new[] { SlotRow.Front, SlotRow.Back })
        {
            for (int col = 0; col < 3; col++)
            {
                BoardSlot slot = board.GetSlot(row, col);
                if (slot.IsEmpty) continue;
                if (slot.occupiedCard.hasAttackedThisRound) continue;

                int value = slot.occupiedCard.EffectiveValue;
                if (value > targetValue && value < bestValue)
                {
                    best = slot;
                    bestValue = value;
                }
            }
        }

        return best;
    }

    private static BoardSlot FindStrongestAvailableAttacker(Board board)
    {
        BoardSlot best = null;
        int bestValue = -1;

        foreach (SlotRow row in new[] { SlotRow.Front, SlotRow.Back })
        {
            for (int col = 0; col < 3; col++)
            {
                BoardSlot slot = board.GetSlot(row, col);
                if (slot.IsEmpty) continue;
                if (slot.occupiedCard.hasAttackedThisRound) continue;

                int value = slot.occupiedCard.EffectiveValue;
                if (value > bestValue)
                {
                    best = slot;
                    bestValue = value;
                }
            }
        }

        return best;
    }

    private static void Shuffle(List<BoardSlot> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
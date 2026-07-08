using System.Collections.Generic;

public class Board
{
    public const int MaxAttacksPerRound = 3;

    public BoardSlot[,] slots;
    public bool isKingAlive = true;
    public string ownerName;

    public int pendingPlacementBonus = 0;
    public bool hasMirrorShield = false;

    public int attacksUsedThisRound = 0;

    public Board(string ownerName)
    {
        this.ownerName = ownerName;
        slots = new BoardSlot[2, 3];

        for (int col = 0; col < 3; col++)
        {
            slots[0, col] = new BoardSlot(SlotRow.Front, col);
            slots[1, col] = new BoardSlot(SlotRow.Back, col);
        }
    }

    public BoardSlot GetSlot(SlotRow row, int col)
    {
        int rowIndex = (row == SlotRow.Front) ? 0 : 1;
        return slots[rowIndex, col];
    }

    public bool PlaceCard(SlotRow row, int col, CardInstance card)
    {
        BoardSlot slot = GetSlot(row, col);
        if (!slot.IsEmpty)
        {
            UnityEngine.Debug.LogWarning($"[{ownerName}] Tentativa de colocar carta em espaço ocupado ({row}, col {col}).");
            return false;
        }

        if (pendingPlacementBonus != 0)
        {
            card.ApplyBonus(pendingPlacementBonus);
            UnityEngine.Debug.Log($"[{ownerName}] Bônus de +{pendingPlacementBonus} aplicado em {card}.");
            pendingPlacementBonus = 0;
        }

        slot.PlaceCard(card);
        return true;
    }

    public void RemoveCard(SlotRow row, int col)
    {
        GetSlot(row, col).ClearSlot();
    }

    public bool IsSlotAttackable(SlotRow row, int col)
    {
        BoardSlot targetSlot = GetSlot(row, col);
        if (targetSlot.IsEmpty) return false;

        if (row == SlotRow.Front) return true;

        BoardSlot frontSlot = GetSlot(SlotRow.Front, col);
        return frontSlot.IsEmpty;
    }

    public List<BoardSlot> GetAllAttackableSlots()
    {
        List<BoardSlot> attackable = new List<BoardSlot>();
        for (int col = 0; col < 3; col++)
        {
            if (IsSlotAttackable(SlotRow.Front, col)) attackable.Add(GetSlot(SlotRow.Front, col));
            if (IsSlotAttackable(SlotRow.Back, col)) attackable.Add(GetSlot(SlotRow.Back, col));
        }
        return attackable;
    }

    public bool IsKingExposed()
    {
        for (int col = 0; col < 3; col++)
        {
            bool frontEmpty = GetSlot(SlotRow.Front, col).IsEmpty;
            bool backEmpty = GetSlot(SlotRow.Back, col).IsEmpty;

            if (frontEmpty && backEmpty) return true;
        }
        return false;
    }

    public bool IsFullyEmpty()
    {
        for (int col = 0; col < 3; col++)
        {
            if (!GetSlot(SlotRow.Front, col).IsEmpty) return false;
            if (!GetSlot(SlotRow.Back, col).IsEmpty) return false;
        }
        return true;
    }

    public List<BoardSlot> GetEmptySlots()
    {
        List<BoardSlot> empty = new List<BoardSlot>();
        for (int col = 0; col < 3; col++)
        {
            if (GetSlot(SlotRow.Front, col).IsEmpty) empty.Add(GetSlot(SlotRow.Front, col));
            if (GetSlot(SlotRow.Back, col).IsEmpty) empty.Add(GetSlot(SlotRow.Back, col));
        }
        return empty;
    }

    public bool HasAttacksRemaining()
    {
        return attacksUsedThisRound < MaxAttacksPerRound;
    }

    public void ResetAttackFlags()
    {
        attacksUsedThisRound = 0;

        for (int col = 0; col < 3; col++)
        {
            BoardSlot front = GetSlot(SlotRow.Front, col);
            if (!front.IsEmpty) front.occupiedCard.hasAttackedThisRound = false;

            BoardSlot back = GetSlot(SlotRow.Back, col);
            if (!back.IsEmpty) back.occupiedCard.hasAttackedThisRound = false;
        }
    }
}
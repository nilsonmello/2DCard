public enum SlotRow
{
    Front,
    Back
}

public class BoardSlot
{
    public SlotRow row;
    public int columnIndex;
    public CardInstance occupiedCard;

    public bool IsEmpty => occupiedCard == null;

    public BoardSlot(SlotRow row, int columnIndex)
    {
        this.row = row;
        this.columnIndex = columnIndex;
        this.occupiedCard = null;
    }

    public void PlaceCard(CardInstance card)
    {
        occupiedCard = card;
    }

    public void ClearSlot()
    {
        occupiedCard = null;
    }
}

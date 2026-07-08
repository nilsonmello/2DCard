using UnityEngine;

public class PeekArtifact : ArtifactData
{
    public override void Execute(ArtifactContext context, BoardSlot target)
    {
        target.occupiedCard.revealedToOpponent = true;
        Debug.Log($"[Artefato] Olho Cego revelou: {target.occupiedCard}");
    }
}

public class SacrificeArtifact : ArtifactData
{
    public override void Execute(ArtifactContext context, BoardSlot target)
    {
        Debug.Log($"[Artefato] Sacrifício removeu {target.occupiedCard} da sua fileira da frente.");
        context.ownerBoard.RemoveCard(target.row, target.columnIndex);
    }
}

public class HungerArtifact : ArtifactData
{
    public int bonusAmount = 3;

    public override void Execute(ArtifactContext context, BoardSlot target)
    {
        context.ownerBoard.pendingPlacementBonus += bonusAmount;
        Debug.Log($"[Artefato] Fome ativado. Próxima carta colocada ganha +{bonusAmount}.");
    }
}

public class MirrorArtifact : ArtifactData
{
    public override void Execute(ArtifactContext context, BoardSlot target)
    {
        context.ownerBoard.hasMirrorShield = true;
        Debug.Log("[Artefato] Espelho ativado. Próxima derrota sua vira empate.");
    }
}

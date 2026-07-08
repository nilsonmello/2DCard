using UnityEngine;

public static class ArtifactFactory
{
    public static ArtifactData CreateRandomArtifact()
    {
        int roll = Random.Range(0, 4);

        switch (roll)
        {
            case 0:
                var peek = ScriptableObject.CreateInstance<PeekArtifact>();
                peek.artifactName = "Olho Cego";
                peek.description = "Dá uma espiada numa carta virada do inimigo";
                peek.requiresTarget = true;
                peek.targetIsOwnBoard = false;
                peek.usablePhase = ArtifactUsablePhase.Any;
                return peek;

            case 1:
                var sacrifice = ScriptableObject.CreateInstance<SacrificeArtifact>();
                sacrifice.artifactName = "Sacrifício";
                sacrifice.description = "Tira uma carta da sua fileira da frente";
                sacrifice.requiresTarget = true;
                sacrifice.targetIsOwnBoard = true;
                sacrifice.usablePhase = ArtifactUsablePhase.Any;
                return sacrifice;

            case 2:
                var hunger = ScriptableObject.CreateInstance<HungerArtifact>();
                hunger.artifactName = "Fome";
                hunger.description = "A próxima carta que você colocar sai com +3, só dá pra usar na hora de colocar cartas";
                hunger.requiresTarget = false;
                hunger.usablePhase = ArtifactUsablePhase.PlacementOnly;
                return hunger;

            default:
                var mirror = ScriptableObject.CreateInstance<MirrorArtifact>();
                mirror.artifactName = "Espelho";
                mirror.description = "Se você perder um combate essa rodada, vira empate";
                mirror.requiresTarget = false;
                mirror.usablePhase = ArtifactUsablePhase.Any;
                return mirror;
        }
    }
}
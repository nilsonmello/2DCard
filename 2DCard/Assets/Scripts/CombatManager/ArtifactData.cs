using UnityEngine;

public enum ArtifactUsablePhase
{
    Any,
    PlacementOnly,
    AttackOnly
}

public class ArtifactContext
{
    public Board ownerBoard;
    public Board enemyBoard;
}

public abstract class ArtifactData : ScriptableObject
{
    public string artifactName;
    [TextArea] public string description;

    public bool requiresTarget;

    public bool targetIsOwnBoard;

    public ArtifactUsablePhase usablePhase = ArtifactUsablePhase.Any;

    public abstract void Execute(ArtifactContext context, BoardSlot target);
}
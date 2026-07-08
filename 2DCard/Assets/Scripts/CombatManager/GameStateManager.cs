using System;
using System.Collections.Generic;
using UnityEngine;

public enum TurnPhase
{
    Placement,
    Attack,
    GameOver
}

// Puro estado + regras de fluxo de turno. Não sabe nada sobre Canvas/UI -
// só expõe getters públicos e métodos que a camada de UI chama, e avisa
// via evento sempre que algo muda pra UI redesenhar.
public class GameStateManager : MonoBehaviour
{
    private const int InitialHandSize = 6;
    private const int RefillHandSize = 3;

    private Board playerBoard;
    private Board enemyBoard;
    private List<CardData> playerHand;
    private List<CardData> enemyHand;
    private List<ArtifactData> playerArtifacts;

    private TurnPhase phase;
    private CardData selectedHandCard;
    private int pendingWildcardValue = 5;
    private BoardSlot selectedAttackerSlot;
    private ArtifactData pendingArtifact;
    private string statusMessage = "";

    private bool isPlayerAttackTurn = true;
    private bool playerPassedLastTurn = false;
    private bool aiPassedLastTurn = false;
    private bool isFirstPlacementPhase = true;

    // Avisa a UI (ou qualquer outro listener) que o estado mudou e precisa redesenhar.
    public event Action OnStateChanged;

    // ---------------- API pública de leitura (usada pelas Views) ----------------
    public Board PlayerBoard => playerBoard;
    public Board EnemyBoard => enemyBoard;
    public List<CardData> PlayerHand => playerHand;
    public List<ArtifactData> PlayerArtifacts => playerArtifacts;
    public TurnPhase Phase => phase;
    public string StatusMessage => statusMessage;
    public CardData SelectedHandCard => selectedHandCard;
    public int PendingWildcardValue => pendingWildcardValue;
    public BoardSlot SelectedAttackerSlot => selectedAttackerSlot;
    public ArtifactData PendingArtifact => pendingArtifact;
    public bool IsPlayerAttackTurn => isPlayerAttackTurn;

    void Start()
    {
        StartNewGame();
    }

    private void NotifyStateChanged() => OnStateChanged?.Invoke();

    // ---------------- Fluxo de jogo (igual à versão anterior) ----------------

    private void StartNewGame()
    {
        playerBoard = new Board("Jogador");
        enemyBoard = new Board("Inimigo");
        playerHand = new List<CardData>();
        enemyHand = new List<CardData>();
        playerArtifacts = new List<ArtifactData>();
        selectedHandCard = null;
        selectedAttackerSlot = null;
        pendingArtifact = null;
        statusMessage = "";

        isPlayerAttackTurn = true;
        playerPassedLastTurn = false;
        aiPassedLastTurn = false;
        isFirstPlacementPhase = true;

        StartPlacementPhase();
    }

    private void StartPlacementPhase()
    {
        int targetHandSize = isFirstPlacementPhase ? InitialHandSize : RefillHandSize;

        RefillHand(playerHand, targetHandSize);
        RefillHand(enemyHand, targetHandSize);

        isFirstPlacementPhase = false;

        phase = TurnPhase.Placement;
        statusMessage = "Bora colocar suas cartas nos espaços vazios";
        NotifyStateChanged();
    }

    private void RefillHand(List<CardData> hand, int targetHandSize)
    {
        while (hand.Count < targetHandSize)
        {
            hand.Add(CardFactory.CreateRandomCard());
        }
    }

    private void StartAttackPhase()
    {
        playerBoard.ResetAttackFlags();
        enemyBoard.ResetAttackFlags();

        phase = TurnPhase.Attack;
        isPlayerAttackTurn = true;
        playerPassedLastTurn = false;
        aiPassedLastTurn = false;

        statusMessage = $"Sua vez ataque uma carta do inimigo, vá pro Rei se ele tiver exposto, ou passa a vez (você tem até {Board.MaxAttacksPerRound} ataques essa rodada)";
        NotifyStateChanged();
    }

    private void ProcessAITurn()
    {
        bool aiActed = AIController.TryOneAttack(enemyBoard, playerBoard);
        aiPassedLastTurn = !aiActed;

        if (!playerBoard.isKingAlive || !enemyBoard.isKingAlive)
        {
            phase = TurnPhase.GameOver;
            statusMessage = !playerBoard.isKingAlive
                ? "Você perdeu o Rei caiu"
                : "Você venceu o Rei inimigo caiu"; 
            NotifyStateChanged();
            return;
        }

        if ((playerPassedLastTurn || !playerBoard.HasAttacksRemaining())
            && (aiPassedLastTurn || !enemyBoard.HasAttacksRemaining()))
        {
            StartPlacementPhase();
            return;
        }

        isPlayerAttackTurn = true;
        statusMessage = aiActed
            ? "A IA atacou sua vez de atacar ou passar"
            : "A IA passou (ou já usou os ataques dela) sua vez";
        NotifyStateChanged();
    }

    // ---------------- Ações públicas chamadas pela UI ----------------

    public void OnHandCardSelected(CardData card)
    {
        if (phase != TurnPhase.Placement || pendingArtifact != null) return;

        selectedHandCard = card;
        pendingWildcardValue = 5;
        NotifyStateChanged();
    }

    public void OnWildcardValueChanged(int value)
    {
        pendingWildcardValue = value;
        NotifyStateChanged();
    }

    public void OnFinishPlacementClicked()
    {
        if (phase != TurnPhase.Placement) return;

        AIController.PlaceCards(enemyBoard, enemyHand);
        selectedHandCard = null;
        StartAttackPhase();
    }

    public void OnPassClicked()
    {
        if (phase != TurnPhase.Attack || !isPlayerAttackTurn) return;

        selectedAttackerSlot = null;
        playerPassedLastTurn = true;
        isPlayerAttackTurn = false;
        statusMessage = "Você passou a vez";
        ProcessAITurn();
    }

    public void OnKingButtonClicked(Board targetBoard)
    {
        bool canAttack = phase == TurnPhase.Attack && isPlayerAttackTurn
                          && targetBoard.isKingAlive && targetBoard.IsKingExposed()
                          && selectedAttackerSlot != null
                          && playerBoard.HasAttacksRemaining();
        if (!canAttack) return;

        CardInstance attackerCard = selectedAttackerSlot.occupiedCard;
        CombatResolver.AttackKing(selectedAttackerSlot, playerBoard, targetBoard);
        attackerCard.hasAttackedThisRound = true;
        playerBoard.attacksUsedThisRound++;
        selectedAttackerSlot = null;
        GrantArtifactToPlayer();

        if (!targetBoard.isKingAlive)
        {
            phase = TurnPhase.GameOver;
            statusMessage = "Você venceu o Rei inimigo caiu";
            NotifyStateChanged();
            return;
        }

        playerPassedLastTurn = false;
        isPlayerAttackTurn = false;
        ProcessAITurn();
    }

    public void OnBoardSlotClicked(Board board, BoardSlot slot, bool isEnemyBoard)
    {
        if (pendingArtifact != null)
        {
            bool matchesBoard = pendingArtifact.targetIsOwnBoard ? !isEnemyBoard : isEnemyBoard;
            if (matchesBoard && !slot.IsEmpty)
            {
                ResolveArtifactTarget(slot);
            }
            return;
        }

        if (phase == TurnPhase.Placement && !isEnemyBoard && slot.IsEmpty && selectedHandCard != null)
        {
            int wildcardValue = selectedHandCard.isWildcard ? pendingWildcardValue : -1;
            CardInstance instance = new CardInstance(selectedHandCard, wildcardValue);
            playerBoard.PlaceCard(slot.row, slot.columnIndex, instance);
            playerHand.Remove(selectedHandCard);
            selectedHandCard = null;
            NotifyStateChanged();
        }
        else if (phase == TurnPhase.Attack && isPlayerAttackTurn && !isEnemyBoard && !slot.IsEmpty
                 && !slot.occupiedCard.hasAttackedThisRound && playerBoard.HasAttacksRemaining())
        {
            selectedAttackerSlot = slot;
            NotifyStateChanged();
        }
        else if (phase == TurnPhase.Attack && isPlayerAttackTurn && isEnemyBoard && selectedAttackerSlot != null
                 && board.IsSlotAttackable(slot.row, slot.columnIndex) && playerBoard.HasAttacksRemaining())
        {
            CardInstance attackerCard = selectedAttackerSlot.occupiedCard;
            CombatResolver.ResolveAttack(selectedAttackerSlot, playerBoard, slot, enemyBoard);
            attackerCard.hasAttackedThisRound = true;
            playerBoard.attacksUsedThisRound++;
            selectedAttackerSlot = null;
            GrantArtifactToPlayer();

            playerPassedLastTurn = false;
            isPlayerAttackTurn = false;
            ProcessAITurn();
        }
    }

    public void OnArtifactClicked(ArtifactData artifact)
    {
        if (pendingArtifact != null) return;

        if (!IsArtifactUsableNow(artifact))
        {
            statusMessage = $"'{artifact.artifactName}' não dá pra usar agora não";
            NotifyStateChanged();
            return;
        }

        ActivateArtifact(artifact);
    }

    private bool IsArtifactUsableNow(ArtifactData artifact)
    {
        switch (artifact.usablePhase)
        {
            case ArtifactUsablePhase.PlacementOnly:
                return phase == TurnPhase.Placement;
            case ArtifactUsablePhase.AttackOnly:
                return phase == TurnPhase.Attack;
            default:
                return true;
        }
    }

    public void OnCancelArtifactClicked()
    {
        pendingArtifact = null;
        statusMessage = "Beleza, cancelado";
        NotifyStateChanged();
    }

    public void OnRestartClicked()
    {
        StartNewGame();
    }

    // ---------------- Regras internas (iguais à versão anterior) ----------------

    private void GrantArtifactToPlayer()
    {
        const int maxArtifacts = 3;
        if (playerArtifacts.Count >= maxArtifacts)
        {
            Debug.Log("[Loot] Limite de 3 artefatos atingido loot perdido");
            statusMessage = "Seus artefatos já estão cheios (máx 3) esse aí foi perdido";
            return;
        }

        ArtifactData artifact = ArtifactFactory.CreateRandomArtifact();
        playerArtifacts.Add(artifact);
        Debug.Log($"[Loot] Você achou um artefato: {artifact.artifactName}");
    }

    private void ActivateArtifact(ArtifactData artifact)
    {
        if (!artifact.requiresTarget)
        {
            var context = new ArtifactContext { ownerBoard = playerBoard, enemyBoard = enemyBoard };
            artifact.Execute(context, null);
            playerArtifacts.Remove(artifact);
            NotifyStateChanged();
        }
        else
        {
            pendingArtifact = artifact;
            statusMessage = $"Escolhe o alvo pra usar '{artifact.artifactName}'";
            NotifyStateChanged();
        }
    }

    private void ResolveArtifactTarget(BoardSlot target)
    {
        var context = new ArtifactContext { ownerBoard = playerBoard, enemyBoard = enemyBoard };
        pendingArtifact.Execute(context, target);
        playerArtifacts.Remove(pendingArtifact);
        pendingArtifact = null;
        statusMessage = "Artefato usado";
        NotifyStateChanged();
    }
}
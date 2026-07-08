using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BoardView : MonoBehaviour
{
    [SerializeField] private bool isEnemyBoard;

    [Header("Slots - ordem: col 0, 1, 2")]
    [SerializeField] private BoardSlotView[] frontSlots = new BoardSlotView[3];
    [SerializeField] private BoardSlotView[] backSlots = new BoardSlotView[3];

    [SerializeField] private TMP_Text titleText;
    [SerializeField] private Button kingButton;
    [SerializeField] private TMP_Text kingButtonLabel;
    [SerializeField] private Image kingButtonBackground;

    private GameStateManager manager;
    private Board board;

    public void Init(GameStateManager gameStateManager, bool isEnemy)
    {
        manager = gameStateManager;
        isEnemyBoard = isEnemy;

        for (int col = 0; col < 3; col++)
        {
            int capturedCol = col;
            frontSlots[col].Setup(() => OnSlotClicked(SlotRow.Front, capturedCol));
            backSlots[col].Setup(() => OnSlotClicked(SlotRow.Back, capturedCol));
        }

        kingButton.onClick.RemoveAllListeners();
        kingButton.onClick.AddListener(() => manager.OnKingButtonClicked(board));

        kingButton.gameObject.SetActive(true);
    }

    private void OnSlotClicked(SlotRow row, int col)
    {
        BoardSlot slot = board.GetSlot(row, col);
        manager.OnBoardSlotClicked(board, slot, isEnemyBoard);
    }

    public void Refresh(Board boardData)
    {
        board = boardData;

        titleText.text = (isEnemyBoard ? "INIMIGO" : "VOCÊ") + (board.isKingAlive ? "" : " (rei caiu)");

        RefreshRow(frontSlots, SlotRow.Front);
        RefreshRow(backSlots, SlotRow.Back);

        RefreshKingButton();
    }

    private void RefreshRow(BoardSlotView[] views, SlotRow row)
    {
        for (int col = 0; col < 3; col++)
        {
            BoardSlot slot = board.GetSlot(row, col);
            ApplyState(views[col], slot, row, col);
        }
    }

    private string GetSlotLabel(BoardSlot slot)
    {
        if (slot.IsEmpty) return "vazio";
        bool hiddenFromPlayer = isEnemyBoard && !slot.occupiedCard.revealedToOpponent;
        return hiddenFromPlayer ? "??" : slot.occupiedCard.EffectiveValue.ToString();
    }

    private void ApplyState(BoardSlotView view, BoardSlot slot, SlotRow row, int col)
    {
        view.SetLabel(GetSlotLabel(slot));

        var pendingArtifact = manager.PendingArtifact;
        var phase = manager.Phase;

        bool interactable = false;
        SlotVisualState state = SlotVisualState.Default;

        if (pendingArtifact != null)
        {
            bool matchesBoard = pendingArtifact.targetIsOwnBoard ? !isEnemyBoard : isEnemyBoard;
            if (matchesBoard && !slot.IsEmpty)
            {
                interactable = true;
                state = SlotVisualState.ArtifactTarget;
            }
        }
        else if (phase == TurnPhase.Placement && !isEnemyBoard && slot.IsEmpty && manager.SelectedHandCard != null)
        {
            interactable = true;
            state = SlotVisualState.Placement;
        }
        else if (phase == TurnPhase.Attack && manager.IsPlayerAttackTurn && !isEnemyBoard && !slot.IsEmpty)
        {
            bool alreadyAttacked = slot.occupiedCard.hasAttackedThisRound;
            bool noAttacksLeft = !manager.PlayerBoard.HasAttacksRemaining();
            interactable = !alreadyAttacked && !noAttacksLeft;
            state = alreadyAttacked
                ? SlotVisualState.Used
                : (manager.SelectedAttackerSlot == slot ? SlotVisualState.SelectedAttacker : SlotVisualState.Default);
        }
        else if (phase == TurnPhase.Attack && manager.IsPlayerAttackTurn && isEnemyBoard
                && manager.SelectedAttackerSlot != null && board.IsSlotAttackable(row, col)
                && manager.PlayerBoard.HasAttacksRemaining())
        {
            interactable = true;
            state = SlotVisualState.Attackable;
        }

        view.SetInteractable(interactable);
        view.SetVisualState(state);
    }

    private void RefreshKingButton()
    {
        if (!board.isKingAlive)
        {
            kingButton.interactable = false;
            kingButtonLabel.text = "Rei caiu";
            kingButtonBackground.color = Color.black;
            return;
        }

        bool exposed = board.IsKingExposed();

        bool canAttack = isEnemyBoard && manager.Phase == TurnPhase.Attack && manager.IsPlayerAttackTurn
                        && exposed && manager.SelectedAttackerSlot != null;

        kingButton.interactable = canAttack;
        kingButtonBackground.color = exposed ? new Color(1f, 0.3f, 0.3f) : Color.gray;

        kingButtonLabel.text = isEnemyBoard
            ? (exposed ? "Rei exposto ataca aí" : "Rei protegido")
            : (exposed ? "Seu rei tá exposto" : "Rei protegido");
    }
}
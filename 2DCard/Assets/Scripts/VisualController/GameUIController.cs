using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameUIController : MonoBehaviour
{
    [SerializeField] private GameStateManager manager;

    [Header("Boards")]
    [SerializeField] private BoardView enemyBoardView;
    [SerializeField] private BoardView playerBoardView;

    [Header("Textos gerais")]
    [SerializeField] private TMP_Text phaseText;
    [SerializeField] private TMP_Text statusText;

    [Header("Mão do jogador")]
    [SerializeField] private Transform handPanel;
    [SerializeField] private HandCardView handCardPrefab;
    [SerializeField] private Slider wildcardSlider;
    [SerializeField] private TMP_Text wildcardValueText;

    [Header("Artefatos")]
    [SerializeField] private Transform artifactPanel;
    [SerializeField] private ArtifactButtonView artifactButtonPrefab;

    [Header("Botões de ação")]
    [SerializeField] private Button finishPlacementButton;
    [SerializeField] private Button passButton;
    [SerializeField] private Button cancelArtifactButton;
    [SerializeField] private Button restartButton;

    private readonly List<HandCardView> spawnedHandViews = new List<HandCardView>();
    private readonly List<ArtifactButtonView> spawnedArtifactViews = new List<ArtifactButtonView>();

    void Awake()
    {
        enemyBoardView.Init(manager, isEnemy: true);
        playerBoardView.Init(manager, isEnemy: false);

        finishPlacementButton.onClick.AddListener(manager.OnFinishPlacementClicked);
        passButton.onClick.AddListener(manager.OnPassClicked);
        cancelArtifactButton.onClick.AddListener(manager.OnCancelArtifactClicked);
        restartButton.onClick.AddListener(manager.OnRestartClicked);
        wildcardSlider.onValueChanged.AddListener(v => manager.OnWildcardValueChanged(Mathf.RoundToInt(v)));

        manager.OnStateChanged += Refresh;
    }

    void OnDestroy()
    {
        manager.OnStateChanged -= Refresh;
    }

    void Start()
    {
        Refresh();
    }

    private void Refresh()
    {
        phaseText.text = $"FASE: {manager.Phase}";
        statusText.text = manager.StatusMessage;

        bool gameOver = manager.Phase == TurnPhase.GameOver;
        restartButton.gameObject.SetActive(gameOver);

        enemyBoardView.gameObject.SetActive(!gameOver);
        playerBoardView.gameObject.SetActive(!gameOver);
        artifactPanel.gameObject.SetActive(!gameOver);

        if (gameOver)
        {
            handPanel.gameObject.SetActive(false);
            finishPlacementButton.gameObject.SetActive(false);
            passButton.gameObject.SetActive(false);
            cancelArtifactButton.gameObject.SetActive(false);
            return;
        }

        enemyBoardView.Refresh(manager.EnemyBoard);
        playerBoardView.Refresh(manager.PlayerBoard);
        RefreshArtifacts();

        bool hasPendingArtifact = manager.PendingArtifact != null;
        cancelArtifactButton.gameObject.SetActive(hasPendingArtifact);

        bool isPlacement = manager.Phase == TurnPhase.Placement && !hasPendingArtifact;
        handPanel.gameObject.SetActive(isPlacement);
        finishPlacementButton.gameObject.SetActive(isPlacement);
        RefreshHand(isPlacement);

        bool isAttackTurn = manager.Phase == TurnPhase.Attack && manager.IsPlayerAttackTurn && !hasPendingArtifact;
        passButton.gameObject.SetActive(isAttackTurn);
    }

    private void RefreshHand(bool isPlacement)
    {
        foreach (var view in spawnedHandViews) Destroy(view.gameObject);
        spawnedHandViews.Clear();

        if (!isPlacement) return;

        foreach (CardData card in manager.PlayerHand)
        {
            HandCardView view = Instantiate(handCardPrefab, handPanel);
            view.Setup(card, manager.SelectedHandCard == card, manager.OnHandCardSelected);
            spawnedHandViews.Add(view);
        }

        bool showSlider = manager.SelectedHandCard != null && manager.SelectedHandCard.isWildcard;
        wildcardSlider.gameObject.SetActive(showSlider);
        wildcardValueText.gameObject.SetActive(showSlider);

        if (showSlider)
        {
            wildcardSlider.SetValueWithoutNotify(manager.PendingWildcardValue);
            wildcardValueText.text = $"Valor do Coringa: {manager.PendingWildcardValue}";
        }
    }

    private void RefreshArtifacts()
    {
        foreach (var view in spawnedArtifactViews) Destroy(view.gameObject);
        spawnedArtifactViews.Clear();

        foreach (ArtifactData artifact in manager.PlayerArtifacts)
        {
            ArtifactButtonView view = Instantiate(artifactButtonPrefab, artifactPanel);
            view.Setup(artifact, manager.PendingArtifact == artifact, manager.OnArtifactClicked);
            spawnedArtifactViews.Add(view);
        }
    }
}
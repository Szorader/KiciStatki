
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;

public class ReadyUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Button readyButton;
    [SerializeField] private TMP_Text readyButtonText;
    [SerializeField] private TMP_Text playerStatusText;

    private PlayerReady localPlayer;

    private void Start()
    {
        if (readyButton != null)
            readyButton.onClick.AddListener(OnReadyClicked);

        PlayerReady.StateChanged += RefreshUI;
    }

    private void Update()
    {
        // Szukamy lokalnego gracza.
        if (localPlayer == null || !localPlayer.IsSpawned)
        {
            FindLocalPlayer();
        }

        // Odświeżamy UI na bieżąco.
        RefreshUI();
    }

    private void FindLocalPlayer()
    {
        PlayerReady[] players = FindObjectsByType<PlayerReady>(
            FindObjectsInactive.Exclude,
            FindObjectsSortMode.None
        );

        foreach (PlayerReady player in players)
        {
            if (player.IsSpawned && player.IsOwner)
            {
                localPlayer = player;
                break;
            }
        }
    }

    private void OnReadyClicked()
    {
        if (localPlayer == null)
            return;

        localPlayer.SetReady();
        RefreshUI();
    }

    private void RefreshUI()
    {
        if (playerStatusText == null)
            return;

        PlayerReady[] players = FindObjectsByType<PlayerReady>(
            FindObjectsInactive.Exclude,
            FindObjectsSortMode.None
        );

        int readyCount = 0;
        int playerCount = 0;

        foreach (PlayerReady player in players)
        {
            if (!player.IsSpawned)
                continue;

            playerCount++;

            if (player.IsReady.Value)
                readyCount++;
        }

        bool started = localPlayer != null &&
                       localPlayer.IsSpawned &&
                       localPlayer.GameStarted.Value;

        // Aktualizacja przycisku READY.
        if (localPlayer != null && localPlayer.IsSpawned)
        {
            bool isReady = localPlayer.IsReady.Value;

            if (readyButton != null)
                readyButton.interactable = !isReady && !started;

            if (readyButtonText != null)
                readyButtonText.text = isReady ? "READY!" : "READY";
        }

        // Po rozpoczęciu gry nie nadpisujemy tekstu.
        // TurnStatusUI przejmuje wtedy wyświetlanie statusu.
        if (started)
            return;

        string waitingMessage = playerCount < 2
            ? "Oczekiwanie na drugiego gracza..."
            : "Oczekiwanie na gotowość...";

        playerStatusText.text =
            $"Gotowi gracze: {readyCount}/2\n{waitingMessage}";
    }

    private void OnDestroy()
    {
        PlayerReady.StateChanged -= RefreshUI;

        if (readyButton != null)
            readyButton.onClick.RemoveListener(OnReadyClicked);
    }
}

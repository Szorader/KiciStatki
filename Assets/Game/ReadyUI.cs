
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ReadyUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Button readyButton;
    [SerializeField] private TMP_Text readyButtonText;
    [SerializeField] private TMP_Text playerStatusText;

    private PlayerReady localPlayer;

    private void Start()
    {
        readyButton.onClick.AddListener(OnReadyClicked);

        PlayerReady.StateChanged += RefreshUI;
    }

    private void Update()
    {
        // Czekamy na pojawienie się lokalnego gracza
        if (localPlayer == null)
        {
            PlayerReady[] players = FindObjectsOfType<PlayerReady>();

            foreach (PlayerReady player in players)
            {
                if (player.IsOwner && player.IsSpawned)
                {
                    localPlayer = player;
                    RefreshUI();
                    break;
                }
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
        PlayerReady[] players = FindObjectsOfType<PlayerReady>();

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
                       localPlayer.GameStarted.Value;

        if (localPlayer != null)
        {
            readyButton.interactable =
                !localPlayer.IsReady.Value && !started;

            readyButtonText.text =
                localPlayer.IsReady.Value ? "READY!" : "READY";
        }

        if (started)
        {
            playerStatusText.text = "GRA ROZPOCZĘTA!";
        }
        else
        {
            playerStatusText.text =
                $"Gotowi gracze: {readyCount}/{2}\n" +
                (playerCount < 2
                    ? "Oczekiwanie na drugiego gracza..."
                    : "Oczekiwanie na gotowość...");
        }
    }

    private void OnDestroy()
    {
        PlayerReady.StateChanged -= RefreshUI;

        if (readyButton != null)
            readyButton.onClick.RemoveListener(OnReadyClicked);
    }
}

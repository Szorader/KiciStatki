
using UnityEngine;
using TMPro;
using Unity.Netcode;

public class TurnStatusUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text statusText;

    private PlayerReady localPlayer;

    private void Update()
    {
        // Szukamy lokalnego gracza.
        if (localPlayer == null || !localPlayer.IsSpawned)
        {
            FindLocalPlayer();
            return;
        }

        UpdateStatus();
    }

    private void FindLocalPlayer()
    {
        PlayerReady[] players =
            FindObjectsByType<PlayerReady>(
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

    
    private void UpdateStatus()
    {
        if (statusText == null || localPlayer == null)
            return;

        // Gra zakończona – pokazujemy wynik.
        if (localPlayer.GameOver.Value)
        {
            ulong winnerId = localPlayer.WinnerClientId.Value;
            ulong localClientId = NetworkManager.Singleton.LocalClientId;

            if (winnerId == localClientId)
            {
                statusText.text = "WYGRAŁEŚ!";
                statusText.color = Color.green;
            }
            else
            {
                statusText.text = "PRZEGRAŁEŚ!";
                statusText.color = Color.red;
            }

            return;
        }

        // Gra jeszcze się nie rozpoczęła.
        // Nie zmieniamy tekstu – pozostawiamy go ReadyUI.
        if (!localPlayer.GameStarted.Value)
            return;

        // Sprawdzamy, czyja jest kolej.
        ulong currentTurn = localPlayer.CurrentTurnOwnerId.Value;
        ulong localId = NetworkManager.Singleton.LocalClientId;

        if (currentTurn == localId)
        {
            statusText.text = "TWOJA KOLEJ!";
            statusText.color = Color.green;
        }
        else
        {
            statusText.text = "KOLEJ PRZECIWNIKA";
            statusText.color = Color.yellow;
        }
    }

}

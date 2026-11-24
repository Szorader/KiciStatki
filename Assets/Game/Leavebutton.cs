using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.Services.Multiplayer;

public class Leavebutton : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private Button button;
    [SerializeField] private string lobbySceneName = "Lobby";

    private bool isLeaving;

    private void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();

        button.onClick.AddListener(LeaveGame);
    }

    public async void LeaveGame()
    {
        if (isLeaving)
            return;

        isLeaving = true;

        button.interactable = false;

        // Pobranie aktualnej sesji
        ISession session = MultiplayerManager.Instance != null
            ? MultiplayerManager.Instance.CurrentSession
            : null;

        // Natychmiastowe przejście do lobby
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

        try
        {
            if (session != null)
            {
                if (session.IsHost)
                {
                    await session.AsHost().DeleteAsync();
                    Debug.Log("[LEAVE] Host usunął pokój.");
                }
                else
                {
                    await session.LeaveAsync();
                    Debug.Log("[LEAVE] Klient opuścił pokój.");
                }

                //if (MultiplayerManager.Instance != null)
                    //MultiplayerManager.Instance.CurrentSession = null;
            }
        }
        catch (Exception e)
        {
            Debug.LogError("[LEAVE] Błąd opuszczania gry: " + e);
        }
        finally
        {
            isLeaving = false;
        }
    }

    private void OnDestroy()
    {
        if (button != null)
            button.onClick.RemoveListener(LeaveGame);
    }
}
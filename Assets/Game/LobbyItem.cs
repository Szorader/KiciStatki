using TMPro;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.UI;

public class LobbyItem : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text lobbyNameText;
    [SerializeField] private TMP_Text playersText;
    [SerializeField] private TMP_Text passwordText;

    [SerializeField] private Button joinButton;

    private string sessionId;

    private LobbyUI lobbyUI;

    public void Setup(
        ISessionInfo session,
        LobbyUI ownerUI)
    {
        lobbyUI = ownerUI;

        sessionId = session.Id;

        if (lobbyNameText != null)
        {
            lobbyNameText.text =
                session.Name;
        }

        if (playersText != null)
        {
            playersText.text =
                $"{session.MaxPlayers - session.AvailableSlots}" +
                $"/{session.MaxPlayers}";
        }

        if (passwordText != null)
        {
            passwordText.text =
                session.HasPassword
                    ? "🔒 HASŁO"
                    : "OTWARTY";
        }

        if (joinButton != null)
        {
            joinButton.onClick.RemoveAllListeners();

            joinButton.onClick.AddListener(
                Join
            );
        }
    }

    private void Join()
    {
        if (lobbyUI == null)
            return;

        lobbyUI.SelectLobby(
            sessionId
        );
    }
}
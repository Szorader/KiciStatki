using System.Collections.Generic;
using TMPro;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{
    [Header("PANELE")]
    [SerializeField] private GameObject browserPanel;
    [SerializeField] private GameObject createPanel;
    [SerializeField] private GameObject passwordPanel;

    [Header("TWORZENIE GRY")]
    [SerializeField] private TMP_InputField gameNameInput;
    [SerializeField] private TMP_InputField createPasswordInput;

    [Header("HASŁO DO GRY")]
    [SerializeField] private TMP_InputField joinPasswordInput;

    [Header("LISTA GIER")]
    [SerializeField] private Transform lobbyListParent;
    [SerializeField] private LobbyItem lobbyItemPrefab;

    [Header("STATUS")]
    [SerializeField] private TMP_Text statusText;

    private string selectedSessionId;

    private void Start()
    {
        if (MultiplayerManager.Instance == null)
        {
            Debug.LogError(
                "Brak MultiplayerManager!"
            );

            return;
        }

        MultiplayerManager.Instance
            .SessionsUpdated += OnSessionsUpdated;

        MultiplayerManager.Instance
            .StatusChanged += OnStatusChanged;

        ShowBrowser();

        Refresh();
    }

    private void OnDestroy()
    {
        if (MultiplayerManager.Instance == null)
            return;

        MultiplayerManager.Instance
            .SessionsUpdated -= OnSessionsUpdated;

        MultiplayerManager.Instance
            .StatusChanged -= OnStatusChanged;
    }

    // =========================================================
    // BROWSER
    // =========================================================

    public async void Refresh()
    {
        await MultiplayerManager.Instance
            .RefreshSessions();
    }

    private void OnSessionsUpdated(
        List<ISessionInfo> sessions)
    {
        ClearLobbyList();

        foreach (ISessionInfo session in sessions)
        {
            LobbyItem item =
                Instantiate(
                    lobbyItemPrefab,
                    lobbyListParent
                );

            item.Setup(
                session,
                this
            );
        }
    }

    private void ClearLobbyList()
    {
        for (int i = lobbyListParent.childCount - 1;
             i >= 0;
             i--)
        {
            Destroy(
                lobbyListParent.GetChild(i).gameObject
            );
        }
    }

    // =========================================================
    // CREATE
    // =========================================================

    public void ShowCreate()
    {
        browserPanel.SetActive(false);
        passwordPanel.SetActive(false);
        createPanel.SetActive(true);
    }

    public void ShowBrowser()
    {
        browserPanel.SetActive(true);
        createPanel.SetActive(false);
        passwordPanel.SetActive(false);
    }

    public async void CreateGame()
    {
        string gameName =
            gameNameInput.text.Trim();

        string password =
            createPasswordInput.text;

        bool success =
            await MultiplayerManager.Instance
                .CreateGame(
                    gameName,
                    password
                );

        if (success)
        {
            Debug.Log(
                "Gra utworzona."
            );
        }
    }

    // =========================================================
    // JOIN
    // =========================================================

    public void SelectLobby(
        string sessionId)
    {
        selectedSessionId = sessionId;

        browserPanel.SetActive(false);
        createPanel.SetActive(false);
        passwordPanel.SetActive(true);

        joinPasswordInput.text = "";
        joinPasswordInput.Select();
    }

    public async void JoinSelectedLobby()
    {
        if (string.IsNullOrEmpty(selectedSessionId))
        {
            Debug.LogError(
                "Nie wybrano pokoju."
            );

            return;
        }

        string password =
            joinPasswordInput.text;

        bool success =
            await MultiplayerManager.Instance
                .JoinGame(
                    selectedSessionId,
                    password
                );

        if (success)
        {
            Debug.Log(
                "Pomyślnie dołączono do gry."
            );
        }
    }

    public void CancelJoin()
    {
        selectedSessionId = null;

        ShowBrowser();
    }

    // =========================================================
    // STATUS
    // =========================================================

    private void OnStatusChanged(
        string message)
    {
        Debug.Log(
            "[MULTIPLAYER] " +
            message
        );

        if (statusText != null)
        {
            statusText.text = message;
        }
    }
}
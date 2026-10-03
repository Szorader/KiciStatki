using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Multiplayer;
using UnityEngine;

public class MultiplayerManager : MonoBehaviour
{
    public static MultiplayerManager Instance { get; private set; }

    public ISession CurrentSession { get; private set; }

    public bool IsInitialized { get; private set; }

    public event Action<List<ISessionInfo>> SessionsUpdated;
    public event Action<string> StatusChanged;

    private QuerySessionsResults currentQuery;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);
    }

    private async void Start()
    {
        await InitializeServices();
    }

    public async Task InitializeServices()
    {
        if (IsInitialized)
            return;

        try
        {
            StatusChanged?.Invoke("Łączenie z Unity Services...");

            await UnityServices.InitializeAsync();

            if (!AuthenticationService.Instance.IsSignedIn)
            {
                StatusChanged?.Invoke("Logowanie...");

                await AuthenticationService.Instance
                    .SignInAnonymouslyAsync();
            }

            IsInitialized = true;

            Debug.Log(
                "Unity Services OK. Player ID: " +
                AuthenticationService.Instance.PlayerId
            );

            StatusChanged?.Invoke("Połączono z usługami.");
        }
        catch (Exception e)
        {
            Debug.LogError(
                "Błąd inicjalizacji Unity Services:\n" + e
            );

            StatusChanged?.Invoke(
                "Błąd połączenia z Unity Services."
            );
        }
    }

    // =========================================================
    // CREATE GAME
    // =========================================================

    public async Task<bool> CreateGame(
        string gameName,
        string password)
    {
        if (!IsInitialized)
        {
            await InitializeServices();
        }

        if (string.IsNullOrWhiteSpace(gameName))
        {
            StatusChanged?.Invoke(
                "Podaj nazwę pokoju."
            );

            return false;
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            StatusChanged?.Invoke(
                "Podaj hasło."
            );

            return false;
        }

        // Unity wymaga obecnie hasła 8-64 znaki.
        if (password.Length < 8)
        {
            StatusChanged?.Invoke(
                "Hasło musi mieć minimum 8 znaków."
            );

            return false;
        }

        try
        {
            StatusChanged?.Invoke(
                "Tworzenie pokoju..."
            );

            SessionOptions options = new SessionOptions
            {
                Name = gameName,
                MaxPlayers = 2,
                IsPrivate = false,
                Password = password
            }.WithRelayNetwork();

            CurrentSession =
                await MultiplayerService.Instance
                    .CreateSessionAsync(options);

            Debug.Log(
                "Utworzono pokój: " +
                CurrentSession.Name
            );

            Debug.Log(
                "Session ID: " +
                CurrentSession.Id
            );

            Debug.Log(
                "Join Code: " +
                CurrentSession.Code
            );

            StatusChanged?.Invoke(
                "Pokój utworzony. Czekam na gracza..."
            );

            return true;
        }
        catch (SessionException e)
        {
            Debug.LogError(
                "Błąd tworzenia pokoju:\n" +
                e.Message
            );

            StatusChanged?.Invoke(
                "Nie udało się utworzyć pokoju."
            );

            return false;
        }
        catch (Exception e)
        {
            Debug.LogError(
                "Błąd:\n" +
                e
            );

            StatusChanged?.Invoke(
                "Wystąpił błąd."
            );

            return false;
        }
    }

    // =========================================================
    // SEARCH
    // =========================================================

    public async Task RefreshSessions()
    {
        if (!IsInitialized)
        {
            await InitializeServices();
        }

        try
        {
            StatusChanged?.Invoke(
                "Wyszukiwanie gier..."
            );

            QuerySessionsOptions options =
                new QuerySessionsOptions
                {
                    Count = 50
                };

            currentQuery =
                await MultiplayerService.Instance
                    .QuerySessionsAsync(options);

            List<ISessionInfo> sessions =
                new List<ISessionInfo>(
                    currentQuery.Sessions
                );

            Debug.Log(
                "Znaleziono pokoi: " +
                sessions.Count
            );

            SessionsUpdated?.Invoke(
                sessions
            );

            StatusChanged?.Invoke(
                "Znaleziono: " +
                sessions.Count +
                " pokoi."
            );
        }
        catch (SessionException e)
        {
            Debug.LogError(
                "Błąd wyszukiwania:\n" +
                e.Message
            );

            StatusChanged?.Invoke(
                "Nie udało się pobrać listy gier."
            );
        }
        catch (Exception e)
        {
            Debug.LogError(
                "Błąd:\n" +
                e
            );
        }
    }

    // =========================================================
    // JOIN GAME
    // =========================================================

    public async Task<bool> JoinGame(
        string sessionId,
        string password)
    {
        if (!IsInitialized)
        {
            await InitializeServices();
        }

        try
        {
            StatusChanged?.Invoke(
                "Dołączanie do pokoju..."
            );

            JoinSessionOptions options =
                new JoinSessionOptions
                {
                    Password = password
                };

            CurrentSession =
                await MultiplayerService.Instance
                    .JoinSessionByIdAsync(
                        sessionId,
                        options
                    );

            Debug.Log(
                "Dołączono do: " +
                CurrentSession.Name
            );

            StatusChanged?.Invoke(
                "Połączono z pokojem."
            );

            return true;
        }
        catch (SessionException e)
        {
            Debug.LogError(
                "Nie udało się dołączyć:\n" +
                e.Message
            );

            StatusChanged?.Invoke(
                "Błędne hasło lub nie można dołączyć."
            );

            return false;
        }
        catch (Exception e)
        {
            Debug.LogError(
                "Błąd:\n" +
                e
            );

            StatusChanged?.Invoke(
                "Wystąpił błąd podczas dołączania."
            );

            return false;
        }
    }

    // =========================================================
    // LEAVE
    // =========================================================

    public async Task LeaveGame()
    {
        if (CurrentSession == null)
            return;

        try
        {
            await CurrentSession.LeaveAsync();

            CurrentSession = null;

            Debug.Log(
                "Opuszczono pokój."
            );
        }
        catch (Exception e)
        {
            Debug.LogError(
                "Błąd opuszczania pokoju:\n" +
                e
            );
        }
    }

    private void OnDestroy()
    {
        currentQuery?.StopPolling();
    }
}
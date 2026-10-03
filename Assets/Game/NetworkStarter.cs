using Unity.Netcode;
using UnityEngine;

public class NetworkStarter : MonoBehaviour
{
    public void StartHostGame()
    {
        if (NetworkManager.Singleton == null)
        {
            Debug.LogError("Brak NetworkManager!");
            return;
        }

        if (NetworkManager.Singleton.IsListening)
            return;

        bool started =
            NetworkManager.Singleton.StartHost();

        Debug.Log(
            "StartHost: " + started
        );
    }

    public void StartClientGame()
    {
        if (NetworkManager.Singleton == null)
        {
            Debug.LogError("Brak NetworkManager!");
            return;
        }

        if (NetworkManager.Singleton.IsListening)
            return;

        bool started =
            NetworkManager.Singleton.StartClient();

        Debug.Log(
            "StartClient: " + started
        );
    }
}
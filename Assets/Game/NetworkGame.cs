using Unity.Netcode;
using UnityEngine;

public class NetworkGame : NetworkBehaviour
{
    public static NetworkGame Instance;

    private void Awake()
    {
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            Debug.Log("Serwer wystartował.");
        }

        if (IsClient)
        {
            Debug.Log("Klient połączył się.");
        }
    }
}
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class NetcodeBootstrap : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private string address = "127.0.0.1";
    [SerializeField] private ushort port = 7777;

    private NetworkManager manager;

    private void Awake()
    {
        Debug.Log("[NetcodeBootstrap] Awake");
        EnsureNetworkManager();
        RegisterCallbacks();
    }

    private void OnDestroy()
    {
        UnregisterCallbacks();
    }

    private void EnsureNetworkManager()
    {
        manager = NetworkManager.Singleton;
        if (manager == null)
        {
            var go = GameObject.Find("NetworkManager");
            if (go == null)
            {
                go = new GameObject("NetworkManager");
            }

            manager = go.GetComponent<NetworkManager>();
            if (manager == null)
            {
                manager = go.AddComponent<NetworkManager>();
            }
        }

        var transport = manager.GetComponent<UnityTransport>();
        if (transport == null)
        {
            transport = manager.gameObject.AddComponent<UnityTransport>();
        }

        if (manager.NetworkConfig == null)
        {
            manager.NetworkConfig = new NetworkConfig();
        }

        transport.SetConnectionData(address, port);
        manager.NetworkConfig.NetworkTransport = transport;

        var resolvedPrefab = ResolvePlayerPrefab();
        if (resolvedPrefab != null)
        {
            manager.NetworkConfig.PlayerPrefab = resolvedPrefab;
        }
        else
        {
            Debug.LogWarning("[NetcodeBootstrap] Player prefab is not assigned.");
        }
    }

    private GameObject ResolvePlayerPrefab()
    {
        if (playerPrefab != null)
        {
            return playerPrefab;
        }

#if UNITY_EDITOR
        playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/NetcodeBootstrap/PlayerCube.prefab");
        return playerPrefab;
#else
        return null;
#endif
    }

    private void RegisterCallbacks()
    {
        if (manager == null)
        {
            return;
        }

        manager.OnClientConnectedCallback -= OnClientConnected;
        manager.OnClientConnectedCallback += OnClientConnected;
    }

    private void UnregisterCallbacks()
    {
        if (manager == null)
        {
            return;
        }

        manager.OnClientConnectedCallback -= OnClientConnected;
    }

    private void OnClientConnected(ulong clientId)
    {
        Debug.Log($"[NetcodeBootstrap] Client connected: {clientId}");
    }

    private void OnClientDisconnected(ulong clientId)
    {
        Debug.Log($"[NetcodeBootstrap] Client disconnected: {clientId}");
    }

    private void OnGUI()
    {
        if (manager == null)
        {
            return;
        }

        const int x = 10;
        int y = 10;
        const int w = 160;
        const int h = 30;
        const int pad = 6;

        if (GUI.Button(new Rect(x, y, w, h), "Start Host"))
        {
            manager.StartHost();
        }
        y += h + pad;

        if (GUI.Button(new Rect(x, y, w, h), "Start Client"))
        {
            manager.StartClient();
        }
        y += h + pad;

        if (GUI.Button(new Rect(x, y, w, h), "Start Server"))
        {
            manager.StartServer();
        }
        y += h + pad;

        if (GUI.Button(new Rect(x, y, w, h), "Shutdown"))
        {
            manager.Shutdown();
        }
        y += h + pad;

        string roleLabel = "Stopped";
        if (manager.IsListening)
        {
            if (manager.IsHost)
            {
                roleLabel = "Host";
            }
            else if (manager.IsServer)
            {
                roleLabel = "Server";
            }
            else if (manager.IsClient)
            {
                roleLabel = "Client";
            }
            else
            {
                roleLabel = "Listening";
            }
        }

        GUI.Label(new Rect(x, y, w + 140, h), $"Role: {roleLabel}");
        y += h + pad;

        if (manager.IsListening && manager.IsClient)
        {
            GUI.Label(new Rect(x, y, w + 140, h), $"ClientId: {manager.LocalClientId}");
        }
    }
}

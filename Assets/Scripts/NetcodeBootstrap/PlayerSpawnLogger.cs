using UnityEngine;
using Unity.Netcode;

public class PlayerSpawnLogger : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        var role = GetRoleLabel();
        Debug.Log($"[PlayerSpawnLogger] Spawned. OwnerClientId={OwnerClientId} Role={role} IsOwner={IsOwner}");
        var renderer = EnsureVisual();
        ApplyRoleColor(renderer);
    }

    private string GetRoleLabel()
    {
        var manager = NetworkManager.Singleton;
        if (manager != null && OwnerClientId == NetworkManager.ServerClientId)
        {
            return "Host";
        }

        return "Client";
    }

    private MeshRenderer EnsureVisual()
    {
        var renderer = GetComponentInChildren<MeshRenderer>();
        if (renderer != null)
        {
            return renderer;
        }

        var visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
        visual.name = "Visual";
        visual.transform.SetParent(transform, false);
        var collider = visual.GetComponent<Collider>();
        if (collider != null)
        {
            Destroy(collider);
        }

        return visual.GetComponent<MeshRenderer>();
    }

    private void ApplyRoleColor(MeshRenderer renderer)
    {
        if (renderer == null)
        {
            return;
        }

        var manager = NetworkManager.Singleton;
        bool isHost = manager != null && OwnerClientId == NetworkManager.ServerClientId;
        var color = isHost ? new Color(1f, 0.75f, 0.2f) : new Color(0.2f, 0.6f, 1f);

        var material = renderer.material;
        if (material == null)
        {
            return;
        }

        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor("_BaseColor", color);
        }
        else if (material.HasProperty("_Color"))
        {
            material.SetColor("_Color", color);
        }
    }
}

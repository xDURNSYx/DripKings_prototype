using System;
using DripKings.Economy;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DripKings.Prototype
{
    // Deliberately standalone: does not alter the existing Netcode test scene or player prefab.
    public sealed class StreetRunPrototype : MonoBehaviour
    {
        private readonly Vector3[] route = { new Vector3(-9, .1f, 8), new Vector3(9, .1f, 8), new Vector3(9, .1f, -8) };
        private readonly GameObject[] markers = new GameObject[3];
        private StreetRunSession session;
        private Transform player;
        private Transform crown;
        private Camera view;
        private long balance;
        private bool ownsCrown;
        private bool pending;
        private string message = "Follow the gold beacons. Finish the route. Build your reputation.";
        private GUIStyle title, body, balanceStyle;

        private void Start()
        {
            session = new StreetRunSession(new LocalCurrencyService(new CurrencyLedger()));
            BuildWorld();
            RefreshMarkers();
        }

        private void BuildWorld()
        {
            RenderSettings.ambientLight = new Color(.4f, .43f, .6f);
            var light = new GameObject("Street lights").AddComponent<Light>();
            light.type = LightType.Directional;
            light.transform.rotation = Quaternion.Euler(50, -30, 0);
            light.intensity = 1.1f;
            Box("Street", Vector3.zero + Vector3.down * .2f, new Vector3(30, .3f, 30), new Color(.06f, .07f, .1f));
            for (int i = -2; i <= 2; i++)
            {
                Box("West block", new Vector3(-16, 2, i * 7), new Vector3(4, 4 + (i + 2) % 3, 5), new Color(.12f, .15f, .24f));
                Box("East block", new Vector3(16, 2, i * 7), new Vector3(4, 4 + (i + 3) % 3, 5), new Color(.16f, .12f, .23f));
                Box("Street stripe", new Vector3(0, .01f, i * 6), new Vector3(.12f, .05f, 2), new Color(.65f, .58f, .3f));
            }
            for (int i = 0; i < route.Length; i++)
                markers[i] = Box("Checkpoint " + (i + 1), route[i], new Vector3(2, .16f, 2), Color.gray);
            player = GameObject.CreatePrimitive(PrimitiveType.Capsule).transform;
            player.name = "Runner";
            player.position = new Vector3(-9, 1, -8);
            SetColor(player.gameObject, new Color(.3f, .85f, .95f));
            crown = Box("Neon crown", player.position + Vector3.up * 1.2f, new Vector3(1.1f, .2f, 1.1f), new Color(1, .7f, .1f)).transform;
            crown.SetParent(player, true);
            crown.gameObject.SetActive(false);
            view = new GameObject("Street camera").AddComponent<Camera>();
            view.clearFlags = CameraClearFlags.SolidColor;
            view.backgroundColor = new Color(.025f, .03f, .055f);
            view.orthographic = true;
            view.orthographicSize = 21;
            view.transform.position = new Vector3(0, 32, -22);
            view.transform.LookAt(Vector3.zero);
            view.gameObject.AddComponent<AudioListener>();
        }

        private static GameObject Box(string label, Vector3 position, Vector3 scale, Color color)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = label; go.transform.position = position; go.transform.localScale = scale;
            SetColor(go, color);
            return go;
        }

        private static void SetColor(GameObject go, Color color)
        {
            var renderer = go.GetComponent<Renderer>();
            if (renderer.sharedMaterial == null || renderer.sharedMaterial.shader.name != "Universal Render Pipeline/Lit")
            {
                var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                if (shader != null) renderer.material = new Material(shader);
            }
            renderer.material.color = color;
        }

        private void Update()
        {
            if (session == null || player == null || pending) return;
            Vector2 input = Vector2.zero;
            var keyboard = Keyboard.current;
            if (keyboard != null)
            {
                input.x = (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed ? 1 : 0) - (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed ? 1 : 0);
                input.y = (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed ? 1 : 0) - (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed ? 1 : 0);
            }
            if (Gamepad.current != null && Gamepad.current.leftStick.ReadValue().sqrMagnitude > input.sqrMagnitude)
                input = Gamepad.current.leftStick.ReadValue();
            Vector3 move = new Vector3(input.x, 0, input.y);
            move = Vector3.ClampMagnitude(move, 1);
            var position = player.position + move * (7 * Time.deltaTime);
            position.x = Mathf.Clamp(position.x, -13, 13);
            position.z = Mathf.Clamp(position.z, -13, 13);
            player.position = position;
            Vector3 target = route[session.NextCheckpoint];
            target.y = position.y;
            if (Vector3.Distance(position, target) < 1.5f) ReachCheckpoint();
        }

        private async void ReachCheckpoint()
        {
            pending = true;
            try
            {
                bool completed = await session.ReachCheckpointAsync(session.NextCheckpoint);
                balance = await session.GetBalanceAsync();
                message = completed ? "+100 KC. Route complete. Run it again to earn your crown." : "Checkpoint cleared. Head to the next gold beacon.";
                RefreshMarkers();
            }
            catch (Exception error) { message = error.Message; }
            finally { pending = false; }
        }

        private void RefreshMarkers()
        {
            for (int i = 0; i < markers.Length; i++)
            {
                bool active = i == session.NextCheckpoint;
                SetColor(markers[i], active ? new Color(1, .7f, .1f) : new Color(.18f, .23f, .3f));
                markers[i].transform.localScale = new Vector3(2, active ? 2 : .16f, 2);
            }
        }

        private async void BuyCrown()
        {
            pending = true;
            try
            {
                await session.BuyCrownAsync();
                balance = await session.GetBalanceAsync();
                ownsCrown = await session.OwnsCrownAsync();
                crown.gameObject.SetActive(ownsCrown);
                message = "Neon crown equipped. You earned it.";
            }
            catch (Exception error) { message = error.Message; }
            finally { pending = false; }
        }

        private void OnGUI()
        {
            if (session == null) return;
            if (title == null)
            {
                title = new GUIStyle(GUI.skin.label) { fontSize = 30, fontStyle = FontStyle.Bold };
                body = new GUIStyle(GUI.skin.label) { fontSize = 15, wordWrap = true };
                balanceStyle = new GUIStyle(title); balanceStyle.normal.textColor = new Color(1, .75f, .2f);
            }
            // Scale the HUD as a unit so controls remain reachable in small windows.
            var previous = GUI.matrix;
            float scale = Mathf.Min(Screen.width / 1000f, Screen.height / 700f);
            GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one * scale);
            GUI.Box(new Rect(18, 18, 470, 235), "");
            GUI.Label(new Rect(34, 28, 430, 44), "DRIP KINGS / STREET RUN", title);
            GUI.Label(new Rect(34, 76, 430, 28), "OFFLINE PROTOTYPE · SESSION PROGRESS", body);
            GUI.Label(new Rect(34, 112, 430, 40), balance + " KC", balanceStyle);
            GUI.Label(new Rect(34, 162, 430, 30), "Route " + (session.CompletedRuns + 1) + "  ·  Beacon " + (session.NextCheckpoint + 1) + "/3  ·  Reward 100 KC", body);
            GUI.Label(new Rect(34, 204, 430, 40), "WASD / arrows / left stick to move", body);
            GUI.Box(new Rect(18, 530, 650, 150), "");
            GUI.Label(new Rect(34, 542, 615, 45), message, body);
            GUI.enabled = !pending && !ownsCrown;
            if (GUI.Button(new Rect(34, 590, 265, 36), ownsCrown ? "NEON CROWN EQUIPPED" : "EQUIP NEON CROWN · 250 KC")) BuyCrown();
            GUI.enabled = true;
            GUI.Label(new Rect(34, 633, 615, 38), "Credits have no real-world value. No cash-out. Progress resets when this play session ends.", body);
            GUI.matrix = previous;
        }
    }
}

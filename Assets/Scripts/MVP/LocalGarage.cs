using UnityEngine;
using UnityEngine.InputSystem;

namespace DripKings.MVP
{
    /// <summary>Offline-only cosmetics and credits. No purchases, networking or real-world value.</summary>
    public sealed class LocalGarage : MonoBehaviour
    {
        private const string CreditsKey = "DripKings.MVP.Credits";
        private const string ColorKey = "DripKings.MVP.Color";
        private static readonly Color[] Palette = { Color.red, Color.blue, Color.green, Color.yellow, Color.white };

        // Shared saved values let the title-menu garage work before the racing scene is loaded.
        public const int PaintCost = 25;
        public static int PaintStyleCount => Palette.Length;
        public static int SavedCredits => Mathf.Max(0, PlayerPrefs.GetInt(CreditsKey, 100));
        public static int SavedPaintIndex => Mathf.Clamp(PlayerPrefs.GetInt(ColorKey, 0), 0, Palette.Length - 1);
        public static Color SavedPaintColor => Palette[SavedPaintIndex];

        public static bool BuySavedPaint()
        {
            int credits = SavedCredits;
            if (credits < PaintCost) return false;
            PlayerPrefs.SetInt(CreditsKey, credits - PaintCost);
            PlayerPrefs.SetInt(ColorKey, (SavedPaintIndex + 1) % Palette.Length);
            PlayerPrefs.Save();
            return true;
        }
        [SerializeField] private Renderer carRenderer;
        [SerializeField] private int startingCredits = 100;
        [SerializeField] private int paintCost = 25;
        private int credits;
        private int colorIndex;
        private Material runtimeMaterial;

        private void Awake()
        {
            credits = Mathf.Max(0, PlayerPrefs.GetInt(CreditsKey, startingCredits));
            colorIndex = Mathf.Clamp(PlayerPrefs.GetInt(ColorKey, 0), 0, Palette.Length - 1);
            if (carRenderer != null)
            {
                runtimeMaterial = carRenderer.material;
                ApplyPaint();
            }
        }

        private void Update()
        {
            if (DripKingsMenuController.IsGameplayActive && Keyboard.current != null && Keyboard.current.gKey.wasPressedThisFrame)
                BuyNextPaint();
        }

        public bool BuyNextPaint()
        {
            // Menu purchases may have changed PlayerPrefs since this scene loaded.
            // Refresh before spending so stale scene state cannot overwrite saved credits.
            credits = SavedCredits;
            colorIndex = SavedPaintIndex;
            // Serialized costs must never be negative or allow free purchases.
            int safePaintCost = Mathf.Max(1, paintCost);
            if (credits < safePaintCost) return false;
            credits -= safePaintCost;
            colorIndex = (colorIndex + 1) % Palette.Length;
            ApplyPaint();
            Save();
            return true;
        }

        public void AwardCredits(int amount)
        {
            if (amount <= 0) return;
            credits = SavedCredits;
            colorIndex = SavedPaintIndex;
            credits = (int)System.Math.Min((long)int.MaxValue, (long)credits + amount);
            ApplyPaint();
            Save();
        }

        private void ApplyPaint()
        {
            if (runtimeMaterial == null) return;
            if (runtimeMaterial.HasProperty("_BaseColor")) runtimeMaterial.SetColor("_BaseColor", Palette[colorIndex]);
            else if (runtimeMaterial.HasProperty("_Color")) runtimeMaterial.color = Palette[colorIndex];
        }

        private void Save()
        {
            PlayerPrefs.SetInt(CreditsKey, credits);
            PlayerPrefs.SetInt(ColorKey, colorIndex);
            PlayerPrefs.Save();
        }

        private void OnDestroy()
        {
            if (runtimeMaterial != null) Destroy(runtimeMaterial);
        }

        private void OnGUI()
        {
            if (!DripKingsMenuController.IsGameplayActive) return;
            // Read the shared save so menu purchases never leave stale HUD values.
            GUI.Label(new Rect(16, 70, 500, 25), $"Garage: {SavedCredits} credits | Paint {SavedPaintIndex + 1}/{Palette.Length} | G: repaint ({Mathf.Max(1, paintCost)})");
        }
    }
}

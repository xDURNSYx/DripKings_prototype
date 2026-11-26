using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace DripKings.MVP
{
    /// <summary>Persistent offline title screen, navigation hub, and in-game pause menu.</summary>
    public sealed class DripKingsMenuController : MonoBehaviour
    {
        public const string StartupScene = "DripKings_StartMenu";
        public const string RacingScene = "DripKings_MVP_Test";

        private enum Page { Title, Hub, Race, Garage, Newspaper }

        private static DripKingsMenuController instance;
        private Page page = Page.Title;
        private bool raceLoaded;
        private GarageVehiclePreview garagePreview;
        private DripKingsTabletView tabletView;

        private GUIStyle primaryButton;
        private GUIStyle secondaryButton;
        private Texture2D buttonTexture;
        private Texture2D buttonHoverTexture;
        private Texture2D secondaryTexture;
        private Texture2D secondaryHoverTexture;

        private static readonly Color Gold = new Color32(244, 171, 58, 255);
        private static readonly Color Cream = new Color32(244, 237, 220, 255);
        private static readonly Color Muted = new Color32(162, 170, 187, 255);
        private static readonly Color Night = new Color32(10, 15, 28, 255);

        // Other MVP components can keep their existing HUD and controls while a race is running.
        public static bool IsGameplayActive => instance == null || instance.page == Page.Race;
        public static bool IsGarageOpen => instance != null && instance.page == Page.Garage;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
            page = Page.Title;
            raceLoaded = SceneManager.GetActiveScene().name == RacingScene;
            Time.timeScale = 1f;
            SceneManager.sceneLoaded += OnSceneLoaded;
            garagePreview = gameObject.AddComponent<GarageVehiclePreview>();
            tabletView = gameObject.AddComponent<DripKingsTabletView>();
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            raceLoaded = scene.name == RacingScene;
        }

        private void OnDestroy()
        {
            if (instance != this) return;
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Time.timeScale = 1f;
            instance = null;
            if (buttonTexture != null) Destroy(buttonTexture);
            if (buttonHoverTexture != null) Destroy(buttonHoverTexture);
            if (secondaryTexture != null) Destroy(secondaryTexture);
            if (secondaryHoverTexture != null) Destroy(secondaryHoverTexture);
        }

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            if (keyboard == null) return;

            if (keyboard.escapeKey.wasPressedThisFrame)
            {
                if (page == Page.Race) OpenHub();
                else if (page == Page.Newspaper && tabletView != null && !tabletView.AtHome) tabletView.GoHome();
                else if (page == Page.Garage || page == Page.Newspaper) OpenHub();
                else if (page == Page.Hub && raceLoaded) OpenRace();
            }
            else if (page == Page.Title && keyboard.enterKey.wasPressedThisFrame)
            {
                OpenHub();
            }
        }

        private void OpenHub()
        {
            if (raceLoaded) Time.timeScale = 0f;
            page = Page.Hub;
        }

        private void OpenRace()
        {
            page = Page.Race;
            Time.timeScale = 1f;
            if (!raceLoaded)
            {
                SceneManager.LoadScene(RacingScene, LoadSceneMode.Single);
                raceLoaded = true;
            }
        }

        private void OpenGarage()
        {
            if (raceLoaded) Time.timeScale = 0f;
            page = Page.Garage;
        }

        private void OpenNewspaper()
        {
            if (raceLoaded) Time.timeScale = 0f;
            page = Page.Newspaper;
            if (tabletView != null) tabletView.GoHome();
        }

        private void BuyPaint()
        {
            LocalGarage garage = FindFirstObjectByType<LocalGarage>();
            if (garage != null)
                garage.BuyNextPaint();
            else
                LocalGarage.BuySavedPaint();
        }

        private void OnGUI()
        {
            EnsureStyles();

            Matrix4x4 oldMatrix = GUI.matrix;
            GUI.matrix = Matrix4x4.Scale(new Vector3(Screen.width / 1280f, Screen.height / 720f, 1f));

            if (page == Page.Race)
            {
                Fill(new Rect(1038, 18, 220, 58), new Color(0.04f, 0.06f, 0.11f, 0.91f));
                if (Button(new Rect(1050, 26, 196, 43), "MENU  [ESC]", false))
                    OpenHub();
            }
            else
            {
                DrawBackground();
                DrawBrand();

                switch (page)
                {
                    case Page.Title: DrawTitle(); break;
                    case Page.Hub: DrawHub(); break;
                    case Page.Garage: DrawGarage(); break;
                    case Page.Newspaper: DrawNewspaper(); break;
                }
            }

            GUI.matrix = oldMatrix;
        }

        private void DrawBackground()
        {
            Fill(new Rect(0, 0, 1280, 720), Night);
            for (int i = 0; i < 24; i++)
            {
                float x = i * 72f - 140f;
                Fill(new Rect(x, 0, 2, 720), new Color(0.18f, 0.22f, 0.32f, 0.12f));
            }
            Fill(new Rect(0, 0, 1280, 7), Gold);
            Fill(new Rect(0, 600, 1280, 3), new Color(0.96f, 0.62f, 0.15f, 0.55f));
            Fill(new Rect(0, 602, 1280, 118), new Color32(15, 22, 38, 255));
            Label(new Rect(60, 660, 740, 28), "A DURNS PRODUCTION   /   DEVELOP. UNITE. REINVENT. NAVIGATE. SUCCEED.", 15, Muted);
            Label(new Rect(946, 660, 270, 28), "LOCAL DEVELOPMENT BUILD", 14, Muted, TextAnchor.MiddleRight);
        }

        private void DrawBrand()
        {
            Fill(new Rect(58, 44, 9, 95), Gold);
            Label(new Rect(90, 36, 355, 77), "DRIP", 72, Cream, TextAnchor.MiddleLeft, FontStyle.Bold);
            Label(new Rect(312, 36, 370, 77), "KINGS", 72, Gold, TextAnchor.MiddleLeft, FontStyle.Bold);
            Label(new Rect(92, 111, 650, 32), "OWN THE ROAD. BUILD YOUR LEGACY.", 20, Muted);
            Fill(new Rect(90, 154, 1100, 1), new Color(1f, 1f, 1f, 0.14f));
        }

        private void DrawTitle()
        {
            Label(new Rect(100, 205, 1080, 62), "WELCOME TO THE STREETS", 40, Cream, TextAnchor.MiddleCenter, FontStyle.Bold);
            Label(new Rect(130, 270, 1020, 48), "Your race. Your ride. Your story.", 23, Muted, TextAnchor.MiddleCenter);
            if (Button(new Rect(450, 358, 380, 86), "START", true))
                OpenHub();
            Label(new Rect(355, 464, 570, 36), "PRESS ENTER OR SELECT START", 17, Muted, TextAnchor.MiddleCenter);
        }

        private void DrawHub()
        {
            Label(new Rect(74, 187, 1130, 48), "CHOOSE YOUR LANE", 36, Cream, TextAnchor.MiddleCenter, FontStyle.Bold);
            Label(new Rect(90, 237, 1100, 32), "Where do you want to go?", 19, Muted, TextAnchor.MiddleCenter);

            if (Button(new Rect(130, 316, 320, 112), raceLoaded ? "RESUME RACE" : "RACE", true))
                OpenRace();
            if (Button(new Rect(480, 316, 320, 112), "GARAGE", false))
                OpenGarage();
            if (Button(new Rect(830, 316, 320, 112), "TABLET", false))
                OpenNewspaper();

            Label(new Rect(130, 445, 320, 38), "DRIVE / DRIFT / CHECKPOINTS", 14, Muted, TextAnchor.MiddleCenter);
            Label(new Rect(480, 445, 320, 38), "CUSTOMIZE YOUR RIDE", 14, Muted, TextAnchor.MiddleCenter);
            Label(new Rect(830, 445, 320, 38), "APPS / SHOPS / CLASSIFIEDS", 14, Muted, TextAnchor.MiddleCenter);
            if (raceLoaded)
                Label(new Rect(360, 527, 560, 40), "RACE PAUSED  •  ESC TO RESUME", 19, Gold, TextAnchor.MiddleCenter);
        }

        private void DrawGarage()
        {
            DrawPanel("THE GARAGE", "MAKE IT YOURS");
            int credits = LocalGarage.SavedCredits;
            int paint = LocalGarage.SavedPaintIndex;
            if (garagePreview != null && garagePreview.Texture != null)
            {
                GUI.DrawTexture(new Rect(190, 311, 520, 208), garagePreview.Texture,
                    ScaleMode.ScaleToFit, false);
                Label(new Rect(365, 518, 330, 25), "ROTATING SHOWROOM CAR", 13, Muted,
                    TextAnchor.MiddleCenter);
            }

            Label(new Rect(744, 309, 330, 32), "LOCAL CREDITS", 17, Muted);
            Label(new Rect(744, 341, 330, 70), credits.ToString(), 47, Gold,
                TextAnchor.MiddleLeft, FontStyle.Bold);
            Label(new Rect(744, 402, 330, 35),
                "PAINT  " + (paint + 1) + " / " + LocalGarage.PaintStyleCount, 22, Cream);
            if (Button(new Rect(744, 448, 315, 64),
                "REPAINT  - " + LocalGarage.PaintCost + " CR", true))
                BuyPaint();
            if (credits < LocalGarage.PaintCost)
                Label(new Rect(744, 516, 340, 28), "Race to earn more credits.", 15, Gold);
            DrawBackButton();
        }

        private void DrawNewspaper()
        {
            if (tabletView != null)
                tabletView.Draw(BuyPaint);
            if (Button(new Rect(85, 622, 267, 37), "<  CLOSE TABLET", false))
                OpenHub();
        }

        private void DrawPanel(string heading, string subheading)
        {
            Fill(new Rect(160, 185, 960, 382), new Color32(25, 34, 51, 255));
            Fill(new Rect(160, 185, 960, 4), Gold);
            Label(new Rect(198, 203, 880, 72), heading, 40, Cream, TextAnchor.MiddleCenter, FontStyle.Bold);
            Label(new Rect(198, 269, 880, 32), subheading, 18, Muted, TextAnchor.MiddleCenter);
            Fill(new Rect(196, 298, 885, 1), new Color(1f, 1f, 1f, 0.2f));
        }

        private void DrawBackButton()
        {
            if (Button(new Rect(60, 525, 285, 57), "<  BACK TO MENU", false))
                OpenHub();
        }

        private bool Button(Rect bounds, string text, bool primary)
        {
            return GUI.Button(bounds, text, primary ? primaryButton : secondaryButton);
        }

        private static void Fill(Rect bounds, Color color)
        {
            Color previous = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(bounds, Texture2D.whiteTexture);
            GUI.color = previous;
        }

        private static void Label(Rect bounds, string text, int fontSize, Color color,
            TextAnchor anchor = TextAnchor.MiddleLeft, FontStyle weight = FontStyle.Normal)
        {
            var style = new GUIStyle(GUI.skin.label)
            {
                alignment = anchor,
                fontSize = fontSize,
                fontStyle = weight,
                wordWrap = true
            };
            style.normal.textColor = color;
            GUI.Label(bounds, text, style);
        }

        private void EnsureStyles()
        {
            if (primaryButton != null) return;

            buttonTexture = Solid(Gold);
            buttonHoverTexture = Solid(new Color32(255, 195, 102, 255));
            secondaryTexture = Solid(new Color32(41, 52, 73, 255));
            secondaryHoverTexture = Solid(new Color32(66, 80, 106, 255));

            primaryButton = new GUIStyle(GUI.skin.button)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 27,
                fontStyle = FontStyle.Bold
            };
            primaryButton.normal.background = buttonTexture;
            primaryButton.normal.textColor = Night;
            primaryButton.hover.background = buttonHoverTexture;
            primaryButton.hover.textColor = Night;
            primaryButton.active.background = buttonHoverTexture;
            primaryButton.active.textColor = Night;

            secondaryButton = new GUIStyle(primaryButton);
            secondaryButton.normal.background = secondaryTexture;
            secondaryButton.normal.textColor = Cream;
            secondaryButton.hover.background = secondaryHoverTexture;
            secondaryButton.hover.textColor = Gold;
            secondaryButton.active.background = secondaryHoverTexture;
            secondaryButton.active.textColor = Cream;
        }

        private static Texture2D Solid(Color color)
        {
            var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false)
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            texture.SetPixel(0, 0, color);
            texture.Apply();
            return texture;
        }
    }
}

using System;
using UnityEngine;

namespace DripKings.MVP
{
    /// <summary>
    /// A single offline tablet launcher for the existing simulated marketplace.
    /// Apps share the same local inventory/credit balance; none contacts a real store.
    /// </summary>
    public sealed class DripKingsTabletView : MonoBehaviour
    {
        public enum App { Home, DBay, DepBoys, DAutoZone, Jobs, Gazette }

        public App CurrentApp { get; private set; } = App.Home;
        public bool AtHome => CurrentApp == App.Home;

        private string notice = "All shops are fictional. Purchases use local game credits only.";

        private static readonly Color Frame = new Color32(5, 9, 17, 255);
        private static readonly Color Screen = new Color32(17, 26, 42, 255);
        private static readonly Color Tile = new Color32(35, 49, 71, 255);
        private static readonly Color Gold = new Color32(244, 171, 58, 255);
        private static readonly Color Cream = new Color32(247, 239, 222, 255);
        private static readonly Color Muted = new Color32(166, 182, 203, 255);

        public void OpenApp(App app)
        {
            if (!Enum.IsDefined(typeof(App), app)) return;
            CurrentApp = app;
            notice = app == App.Home
                ? "Select an app. Your credits and owned parts work across every shop."
                : "Offline simulator — no real payments, player listings or outside stores.";
        }

        public void GoHome() => OpenApp(App.Home);

        public void Draw(Action buyPaint)
        {
            Fill(new Rect(92, 172, 1096, 450), Frame);
            Fill(new Rect(100, 181, 1080, 430), Screen);
            Fill(new Rect(100, 181, 1080, 4), Gold);
            Text(new Rect(133, 197, 520, 35), "DK  /  DRIP KINGS TABLET", 24, Cream, FontStyle.Bold);
            Text(new Rect(787, 199, 208, 32),
                ClassifiedsMarket.Credits + " CREDITS", 18, Gold, FontStyle.Bold, TextAnchor.MiddleRight);
            if (!AtHome && GUI.Button(new Rect(1011, 197, 141, 33), "HOME"))
                GoHome();

            Fill(new Rect(124, 241, 1032, 1), new Color(1f, 1f, 1f, 0.15f));

            if (AtHome) DrawHome();
            else
            {
                switch (CurrentApp)
                {
                    case App.DBay: DrawMarket(); break;
                    case App.DepBoys: DrawService(buyPaint); break;
                    case App.DAutoZone: DrawPartsStore(); break;
                    case App.Jobs: DrawJobs(); break;
                    case App.Gazette: DrawGazette(); break;
                }
            }

            Fill(new Rect(126, 572, 1028, 1), new Color(1f, 1f, 1f, 0.16f));
            Text(new Rect(145, 576, 990, 28), notice, 15, Gold, FontStyle.Normal);
        }

        private void DrawHome()
        {
            Text(new Rect(147, 259, 1000, 39), "YOUR STREET. YOUR BUSINESS. YOUR APPS.", 23, Cream, FontStyle.Bold);
            AppTile(155, 308, App.DBay, "D-BAY", "Classifieds & marketplace", new Color32(59, 107, 153, 255));
            AppTile(491, 308, App.DepBoys, "DEP BOYS", "Mechanic service & paint", new Color32(161, 91, 62, 255));
            AppTile(827, 308, App.DAutoZone, "DAUTO ZONE", "Auto parts & upgrades", new Color32(85, 132, 96, 255));
            AppTile(155, 434, App.Jobs, "DK JOBS", "Employment & shops for sale", new Color32(99, 82, 149, 255));
            AppTile(491, 434, App.Gazette, "DK GAZETTE", "Community news & events", new Color32(139, 108, 58, 255));
            Fill(new Rect(829, 443, 318, 97), new Color(0.15f, 0.20f, 0.29f, 0.50f));
            Text(new Rect(843, 454, 283, 47), "MORE APPS COMING", 19, Muted, FontStyle.Bold, TextAnchor.MiddleCenter);
            Text(new Rect(840, 503, 290, 23), "Clubs • GPS • Racing events", 14, Muted, FontStyle.Normal, TextAnchor.MiddleCenter);
        }

        private void AppTile(float x, float y, App app, string title, string subtitle, Color tint)
        {
            Fill(new Rect(x, y, 316, 108), Tile);
            Fill(new Rect(x, y, 9, 108), tint);
            Text(new Rect(x + 21, y + 13, 277, 44), title, 28, Cream, FontStyle.Bold);
            Text(new Rect(x + 21, y + 55, 280, 28), subtitle, 15, Muted);
            if (GUI.Button(new Rect(x + 227, y + 72, 78, 28), "OPEN"))
                OpenApp(app);
        }

        private void DrawMarket()
        {
            Header("D-BAY", "Buy local classified parts; seller postings are coming later.");
            DrawCatalog(true);
        }

        private void DrawPartsStore()
        {
            Header("DAUTO ZONE", "Retail parts catalog. Uses the same saved Garage inventory.");
            DrawCatalog(false);
        }

        private void DrawCatalog(bool classifieds)
        {
            for (int i = 0; i < ClassifiedsMarket.PartCount; i++)
            {
                var part = ClassifiedsMarket.GetPart(i);
                int y = 340 + i * 66;
                Fill(new Rect(154, y, 970, 61), Tile);
                Text(new Rect(171, y + 5, 520, 30), part.Name, 21, Cream, FontStyle.Bold);
                Text(new Rect(171, y + 34, 620, 22), part.Description, 14, Muted);
                if (ClassifiedsMarket.OwnsPart(i))
                    Text(new Rect(934, y + 11, 164, 39), "OWNED", 17, Gold, FontStyle.Bold, TextAnchor.MiddleCenter);
                else if (GUI.Button(new Rect(896, y + 12, 205, 38), "BUY " + ClassifiedsMarket.PartPrice(i) + " CR"))
                {
                    bool success = ClassifiedsMarket.BuyPart(i);
                    notice = success ? part.Name + " purchased. Install through DEP BOYS."
                        : "Cannot purchase " + part.Name + ": check credits or ownership.";
                }
            }

            if (classifieds)
                Text(new Rect(168, 543, 850, 23), "Prototype listings only. Player-created advertisements are not connected.", 14, Muted);
        }

        private void DrawService(Action buyPaint)
        {
            Header("DEP BOYS", "Mechanic installs and paint. Buy components at D-BAY or DAUTO ZONE.");
            Fill(new Rect(154, 337, 970, 55), Tile);
            Text(new Rect(173, 342, 610, 28), "CUSTOM PAINT SHOP", 20, Cream, FontStyle.Bold);
            Text(new Rect(173, 368, 570, 19), "Apply the next saved Garage paint color.", 14, Muted);
            if (GUI.Button(new Rect(908, 347, 195, 36), "PAINT " + LocalGarage.PaintCost + " CR"))
            {
                int before = ClassifiedsMarket.Credits;
                buyPaint?.Invoke();
                notice = ClassifiedsMarket.Credits < before
                    ? "New paint purchased. Garage and race car share the saved color."
                    : "Paint unavailable: insufficient credits.";
            }
            for (int i = 0; i < ClassifiedsMarket.PartCount; i++)
            {
                var part = ClassifiedsMarket.GetPart(i);
                int y = 404 + i * 54;
                Fill(new Rect(154, y, 970, 49), Tile);
                Text(new Rect(173, y + 5, 465, 33), part.Name, 19, Cream, FontStyle.Bold);
                if (ClassifiedsMarket.InstalledPart(i))
                    Text(new Rect(893, y + 8, 208, 30), "INSTALLED", 17, Gold, FontStyle.Bold, TextAnchor.MiddleCenter);
                else if (!ClassifiedsMarket.OwnsPart(i))
                    Text(new Rect(810, y + 8, 285, 30), "BUY PART FIRST", 16, Muted, FontStyle.Bold, TextAnchor.MiddleCenter);
                else if (GUI.Button(new Rect(875, y + 7, 220, 35),
                         (ClassifiedsMarket.OwnsShop ? "SELF-INSTALL " : "INSTALL ") +
                         ClassifiedsMarket.LaborPrice(i) + " CR"))
                {
                    notice = ClassifiedsMarket.PayToInstall(i)
                        ? part.Name + " installed. Labor and ownership saved."
                        : "Installation failed: check credits and saved ownership.";
                }
            }
        }

        private void DrawJobs()
        {
            Header("DK JOBS", "Employment and business opportunities across the district.");
            Fill(new Rect(154, 330, 970, 91), Tile);
            Text(new Rect(170, 340, 601, 30), "MECHANIC WANTED", 21, Cream, FontStyle.Bold);
            Text(new Rect(170, 374, 620, 24), "Local application only; wages and shifts arrive later.", 15, Muted);
            if (ClassifiedsMarket.MechanicApplicationSent)
                Text(new Rect(886, 349, 219, 49), "APPLICATION SENT", 16, Gold, FontStyle.Bold, TextAnchor.MiddleCenter);
            else if (GUI.Button(new Rect(903, 353, 194, 38), "APPLY"))
                notice = ClassifiedsMarket.ApplyForMechanicJob()
                    ? "Mechanic application saved on this device."
                    : "Application already on record.";

            Fill(new Rect(154, 433, 970, 91), Tile);
            Text(new Rect(170, 442, 602, 30), "MECHANIC SHOP FOR SALE", 21, Cream, FontStyle.Bold);
            Text(new Rect(170, 473, 638, 24), "Wholesale parts and self-install privileges. 500 CR.", 15, Muted);
            if (ClassifiedsMarket.OwnsShop)
                Text(new Rect(884, 454, 213, 43), "SHOP OWNED", 18, Gold, FontStyle.Bold, TextAnchor.MiddleCenter);
            else if (GUI.Button(new Rect(903, 454, 194, 38), "BUY 500 CR"))
                notice = ClassifiedsMarket.BuyMechanicShop()
                    ? "Mechanic shop purchased. Wholesale access unlocked."
                    : "You need 500 saved game credits to own this shop.";
        }

        private void DrawGazette()
        {
            Header("DK GAZETTE", "Local news, racing community and upcoming district events.");
            Fill(new Rect(154, 328, 970, 88), Tile);
            Text(new Rect(178, 341, 898, 33), "WELCOME TO DRIP KINGS", 25, Gold, FontStyle.Bold);
            Text(new Rect(178, 375, 898, 28), "Build your car. Earn credits. Find parts and jobs on your tablet.", 17, Cream);
            Fill(new Rect(154, 428, 970, 91), Tile);
            Text(new Rect(178, 442, 890, 27), "RACE BULLETIN", 21, Cream, FontStyle.Bold);
            Text(new Rect(178, 473, 892, 29), "Live player news and event submissions are planned—not yet online.", 16, Muted);
        }

        private static void Header(string title, string subtitle)
        {
            Text(new Rect(150, 259, 780, 42), title, 31, Gold, FontStyle.Bold);
            Text(new Rect(152, 298, 960, 23), subtitle, 16, Muted);
        }

        private static void Fill(Rect area, Color color)
        {
            Color old = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(area, Texture2D.whiteTexture);
            GUI.color = old;
        }

        private static void Text(Rect area, string content, int size, Color color,
            FontStyle weight = FontStyle.Normal, TextAnchor anchor = TextAnchor.MiddleLeft)
        {
            GUIStyle style = new GUIStyle(GUI.skin.label)
            {
                fontSize = size,
                fontStyle = weight,
                alignment = anchor,
                wordWrap = false
            };
            style.normal.textColor = color;
            GUI.Label(area, content, style);
        }
    }
}
